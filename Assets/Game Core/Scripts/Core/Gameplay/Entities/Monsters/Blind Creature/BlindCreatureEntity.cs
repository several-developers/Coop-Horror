using System;
using System.Collections.Generic;
using GameCore.Configs.Gameplay.Balance;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Monsters.BlindCreature.States;
using GameCore.Gameplay.Other;
using GameCore.Gameplay.Systems.Noise;
using GameCore.Gameplay.Systems.SoundReproducer;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using GameCore.Infrastructure.Providers.Gameplay.MonstersAI;
using GameCore.Infrastructure.StateMachine;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Entities.Monsters.BlindCreature
{
    [GenerateSerializationForType(typeof(SFXType))]
    public class BlindCreatureEntity : SoundProducerNavmeshMonsterEntity<BlindCreatureEntity.SFXType>, INoiseListener
    {
        public enum SFXType
        {
            // _ = 0,
            Whispering = 1,
            Swing = 2,
            Slash = 3,
            Whispers = 4,
            BirdChirp = 5,
            BirdScream = 6
        }
        
        public enum BirdReactionType
        {
            //_ = 0,
            StartScreaming = 1,
            StopScreaming = 2,
            Die = 3
        }
        
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(
            IMonstersAIConfigsProvider monstersAIConfigsProvider,
            IGameplayConfigsProvider gameplayConfigsProvider
        )
        {
            _blindCreatureAIConfig = monstersAIConfigsProvider.GetBlindCreatureAIConfig();
            _balanceConfig = gameplayConfigsProvider.GetBalanceConfig();
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField, Required, Space(height: 5)]
        private TextMeshPro _stateTMP;
        
        [SerializeField, Required]
        private TextMeshPro _suspicionTMP;

        [BoxGroup(Constants.References, showLabel: false), SerializeField]
        private References _references;

        // FIELDS: --------------------------------------------------------------------------------

        private static readonly List<BlindCreatureEntity> AllBlindCreatures = new();

        private BlindCreatureAIConfigMeta _blindCreatureAIConfig;
        private BalanceConfigMeta _balanceConfig;

        private StateMachineBase _blindCreatureStateMachine;
        private SuspicionSystem _suspicionSystem;
        private CombatSystem _combatSystem;
        private AnimationController _animationController;
        private CageBirdController _cageBirdController;

        private bool _isDead;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void StartServerOnly()
        {
            // TEMP
            if (!IsSpawned)
                NetworkObject.Spawn();

            EnterIdleState();
            PlaySound(SFXType.Whispering);
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        [Button(ButtonStyle.FoldoutButton), DisableInEditorMode]
        public void TriggerBird(BirdReactionType reactionType) => TriggerBirdServerRPC(reactionType);

        public void DetectNoise(Vector3 noisePosition, float noiseLoudness) =>
            _suspicionSystem.DetectNoise(noisePosition, noiseLoudness);

        public void DecideStateAfterNoiseDetect()
        {
            bool isAggressive = _suspicionSystem.IsAggressive();
            bool isTargetAtRange = _combatSystem.IsTargetAtRange();
            bool isAttackOnCooldown = _combatSystem.IsAttackOnCooldown();
            bool canAttack = isAggressive && isTargetAtRange && !isAttackOnCooldown;
            
            if (canAttack)
                EnterAttackSuspicionPlaceState();
            else
                EnterMoveToSuspicionPlaceState();
        }
        
        public void EnterIdleState() => ChangeState<IdleState>();

        public void EnterWanderingState() => ChangeState<WanderingState>();
        
        public void EnterMoveToSuspicionPlaceState() => ChangeState<MoveToSuspicionPlaceState>();
        
        public void EnterLookAroundSuspicionPlaceState() => ChangeState<LookAroundSuspicionPlaceState>();
        
        public void EnterAttackSuspicionPlaceState() => ChangeState<AttackSuspicionPlaceState>();

        public static IReadOnlyList<BlindCreatureEntity> GetAllBlindCreatures() => AllBlindCreatures;

        public BlindCreatureAIConfigMeta GetAIConfig() => _blindCreatureAIConfig;

        public References GetReferences() => _references;

        public SuspicionSystem GetSuspicionSystem() => _suspicionSystem;

        public CombatSystem GetCombatSystem() => _combatSystem;

        public override MonsterType GetMonsterType() =>
            MonsterType.BlindCreature;

        public bool IsDead() => _isDead;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void InitAll()
        {
            SoundReproducer = new BlindCreatureSoundReproducer(soundProducer: this, _blindCreatureAIConfig);
            _cageBirdController = new CageBirdController(blindCreatureEntity: this);
        }

        protected override void InitServerOnly()
        {
            AllBlindCreatures.Add(item: this);

            InitSystems();
            SetupStates();

            _blindCreatureStateMachine.OnStateChangedEvent += state =>
            {
                string stateName = state.GetType().Name.Replace(oldValue: "State", newValue: "").GetNiceName();
                _stateTMP.text = $"State: {stateName}";
            };

            _suspicionSystem.OnSuspicionMeterChangedEvent += suspicionMeter =>
            {
                _suspicionTMP.text = $"Suspicion: {suspicionMeter}";
            };

            // LOCAL METHODS: -----------------------------

            void InitSystems()
            {
                _blindCreatureStateMachine = new StateMachineBase();
                _suspicionSystem = new SuspicionSystem(blindCreatureEntity: this, _balanceConfig);
                _animationController = new AnimationController(blindCreatureEntity: this);
                _combatSystem = new CombatSystem(blindCreatureEntity: this, _animationController);

                _animationController.Start();
                _cageBirdController.StartChirping();

                BlindCreatureAttackTrigger attackTrigger = _references.AttackTrigger;
                attackTrigger.IsServerEvent += IsOwnerServer;
                attackTrigger.OnTriggerEnterEvent += OnAttackTriggerEnter;
            }

            void SetupStates()
            {
                IdleState idleState = new(blindCreatureEntity: this);
                WanderingState wanderingState = new(blindCreatureEntity: this);
                MoveToSuspicionPlaceState moveToSuspicionPlaceState = new(blindCreatureEntity: this);
                LookAroundSuspicionPlaceState lookAroundSuspicionPlaceState = new(blindCreatureEntity: this);
                AttackSuspicionPlaceState attackSuspicionPlaceState = new(blindCreatureEntity: this);

                _blindCreatureStateMachine.AddState(idleState);
                _blindCreatureStateMachine.AddState(wanderingState);
                _blindCreatureStateMachine.AddState(moveToSuspicionPlaceState);
                _blindCreatureStateMachine.AddState(lookAroundSuspicionPlaceState);
                _blindCreatureStateMachine.AddState(attackSuspicionPlaceState);
            }
        }

        protected override void TickServerOnly()
        {
            _blindCreatureStateMachine.Tick();
            _animationController.Tick();
        }

        protected override void DespawnServerOnly() =>
            AllBlindCreatures.Remove(item: this);

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void ChangeState<TState>() where TState : IState =>
            _blindCreatureStateMachine.ChangeState<TState>();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnAttackTriggerEnter(Collider other) =>
            _combatSystem.CheckAttackTrigger(other);

        // RPC: -----------------------------------------------------------------------------------

        [ServerRpc(RequireOwnership = false)]
        private void TriggerBirdServerRPC(BirdReactionType reactionType) => TriggerBirdClientRpc(reactionType);

        [ClientRpc]
        private void TriggerBirdClientRpc(BirdReactionType reactionType) =>
            _cageBirdController.TriggerBird(reactionType);

        // INNER CLASSES: -------------------------------------------------------------------------

        [Serializable]
        public class References
        {
            // MEMBERS: -------------------------------------------------------------------------------

            [SerializeField, Required]
            private BlindCreatureAttackTrigger _attackTrigger;
            
            [SerializeField, Required]
            private Animator _creatureAnimator;

            [SerializeField, Required]
            private Animator _birdAnimator;

            [SerializeField, Required]
            private NetworkAnimator _networkAnimator;

            [SerializeField, Required]
            private AnimationObserver _animationObserver;

            [SerializeField, Required]
            private Transform _modelPivot;
            
            [SerializeField, Required]
            private Transform _attackPoint;
            
            [SerializeField, Required]
            private GameObject _calmFace;
            
            [SerializeField, Required]
            private GameObject _angryFace;
            
            [SerializeField, Required]
            private GameObject _calmCape;
            
            [SerializeField, Required]
            private GameObject _angryCape;

            // PROPERTIES: ----------------------------------------------------------------------------

            public BlindCreatureAttackTrigger AttackTrigger => _attackTrigger;
            public Animator CreatureAnimator => _creatureAnimator;
            public Animator BirdAnimator => _birdAnimator;
            public NetworkAnimator NetworkAnimator => _networkAnimator;
            public AnimationObserver AnimationObserver => _animationObserver;
            public Transform ModelPivot => _modelPivot;
            public Transform AttackPoint => _attackPoint;
            public GameObject CalmFace => _calmFace;
            public GameObject AngryFace => _angryFace;
            public GameObject CalmCape => _calmCape;
            public GameObject AngryCape => _angryCape;
        }
    }
}