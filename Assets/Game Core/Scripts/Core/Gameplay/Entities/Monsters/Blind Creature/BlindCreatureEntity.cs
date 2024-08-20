using System;
using System.Collections.Generic;
using GameCore.Configs.Gameplay.Balance;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Monsters.BlindCreature.States;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Systems.Noise;
using GameCore.Gameplay.Systems.SoundReproducer;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using GameCore.Infrastructure.Providers.Gameplay.MonstersAI;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Entities.Monsters.BlindCreature
{
    public class BlindCreatureEntity : SoundProducerNavmeshMonsterEntity<BlindCreatureEntity.SFXType>, INoiseListener
    {
        public enum SFXType
        {
            // _ = 0,
            Growl = 1,
            
            BirdTweet = 5,
            BirdScream = 6
        }
        
        public enum BirdReactionType
        {
            //_ = 0,
            Tweet = 1,
            StartScreaming = 2,
            StopScreaming = 3,
            Die = 4
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

        [BoxGroup(ReferencesTitle, showLabel: false), SerializeField]
        private References _references;

        // FIELDS: --------------------------------------------------------------------------------

        private const string ReferencesTitle = "References";

        private static readonly List<BlindCreatureEntity> AllBlindCreatures = new();
        
        private BlindCreatureAIConfigMeta _blindCreatureAIConfig;
        private BalanceConfigMeta _balanceConfig;

        private StateMachine _blindCreatureStateMachine;
        private SuspicionSystem _suspicionSystem;
        private AnimationController _animationController;
        private CageBirdController _cageBirdController;
        private BlindCreatureSoundReproducer _soundReproducer;
        private PlayerEntity _targetPlayer;

        private bool _isDead;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void StartServerOnly()
        {
            // TEMP
            if (!IsSpawned)
                NetworkObject.Spawn();

            EnterIdleState();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void SetTargetPlayer(PlayerEntity playerEntity) =>
            _targetPlayer = playerEntity;

        public void TriggerBird(BirdReactionType reactionType)
        {
            
        }
        
        public void DetectNoise(Vector3 noisePosition, float noiseLoudness) =>
            _suspicionSystem.DetectNoise(noisePosition, noiseLoudness);

        public void EnterIdleState() => ChangeState<IdleState>();

        public void EnterWanderingState() => ChangeState<WanderingState>();
        
        public void EnterMoveToSuspicionPlaceState() => ChangeState<MoveToSuspicionPlaceState>();
        
        public void EnterLookAroundSuspicionPlaceState() => ChangeState<LookAroundSuspicionPlaceState>();

        public static IReadOnlyList<BlindCreatureEntity> GetAllBlindCreatures() => AllBlindCreatures;

        public BlindCreatureAIConfigMeta GetAIConfig() => _blindCreatureAIConfig;

        public References GetReferences() => _references;

        public SuspicionSystem GetSuspicionSystem() => _suspicionSystem;

        public override MonsterType GetMonsterType() =>
            MonsterType.BlindCreature;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void InitAll()
        {
            _soundReproducer = new BlindCreatureSoundReproducer(soundProducer: this, _blindCreatureAIConfig);
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

                string log = Log.HandleLog($"New state: <gb>{stateName}</gb>");
                Debug.Log(log);
            };

            _suspicionSystem.OnSuspicionMeterChangedEvent += suspicionMeter =>
            {
                _suspicionTMP.text = $"Suspicion: {suspicionMeter}";
            };

            // LOCAL METHODS: -----------------------------

            void InitSystems()
            {
                _blindCreatureStateMachine = new StateMachine();
                _suspicionSystem = new SuspicionSystem(blindCreatureEntity: this, _balanceConfig);
                _animationController = new AnimationController(blindCreatureEntity: this);
                
                _animationController.Start();
            }

            void SetupStates()
            {
                IdleState idleState = new(blindCreatureEntity: this);
                WanderingState wanderingState = new(blindCreatureEntity: this);
                MoveToSuspicionPlaceState moveToSuspicionPlaceState = new(blindCreatureEntity: this);
                LookAroundSuspicionPlaceState lookAroundSuspicionPlaceState = new(blindCreatureEntity: this);

                _blindCreatureStateMachine.AddState(idleState);
                _blindCreatureStateMachine.AddState(wanderingState);
                _blindCreatureStateMachine.AddState(moveToSuspicionPlaceState);
                _blindCreatureStateMachine.AddState(lookAroundSuspicionPlaceState);
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
            private Animator _creatureAnimator;

            [SerializeField, Required]
            private Animator _birdAnimator;

            [SerializeField, Required]
            private NetworkAnimator _networkAnimator;

            [SerializeField, Required]
            private Transform _modelPivot;
            
            [SerializeField, Required]
            private GameObject _calmFace;
            
            [SerializeField, Required]
            private GameObject _angryFace;
            
            [SerializeField, Required]
            private GameObject _calmCape;
            
            [SerializeField, Required]
            private GameObject _angryCape;

            // PROPERTIES: ----------------------------------------------------------------------------

            public Animator CreatureAnimator => _creatureAnimator;
            public Animator BirdAnimator => _birdAnimator;
            public NetworkAnimator NetworkAnimator => _networkAnimator;
            public Transform ModelPivot => _modelPivot;
            public GameObject CalmFace => _calmFace;
            public GameObject AngryFace => _angryFace;
            public GameObject CalmCape => _calmCape;
            public GameObject AngryCape => _angryCape;
        }
    }
}