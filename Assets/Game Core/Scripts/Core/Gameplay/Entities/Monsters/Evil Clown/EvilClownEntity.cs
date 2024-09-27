using System.Collections.Generic;
using GameCore.Infrastructure.Configs.Gameplay.Enemies;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Monsters.EvilClown.States;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Level;
using GameCore.Gameplay.Systems.Footsteps;
using GameCore.Gameplay.Systems.SoundReproducer;
using GameCore.Infrastructure.Providers.Gameplay.MonstersAI;
using GameCore.Infrastructure.StateMachine;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Zenject;

namespace GameCore.Gameplay.Entities.Monsters.EvilClown
{
    [GenerateSerializationForType(typeof(SFXType))]
    public class EvilClownEntity : SoundProducerNavmeshMonsterEntity<EvilClownEntity.SFXType>
    {
        public enum SFXType
        {
            // _ = 0,
            Footsteps = 1,
            Roar = 2,
            Brainwash = 3,
            Slash = 4
        }

        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(ILevelProvider levelProvider, IMonstersAIConfigsProvider monstersAIConfigsProvider)
        {
            _levelProvider = levelProvider;
            _evilClownAIConfig = monstersAIConfigsProvider.GetConfig<EvilClownAIConfigMeta>();
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField, Required]
        private Animator _animator;

        [SerializeField, Required]
        private MonsterFootstepsSystem _footstepsSystem;

        [SerializeField, Required]
        private Rig _rig;
        
        [SerializeField, Required]
        private Transform _lookAtObject;
        
        [SerializeField, Required]
        private TextMeshPro _stateTMP;

        [SerializeField]
        private Vector3 _lookAtOffset;

        // FIELDS: --------------------------------------------------------------------------------

        private static readonly List<EvilClownEntity> AllEvilClowns = new();

        private ILevelProvider _levelProvider;
        private EvilClownAIConfigMeta _evilClownAIConfig;

        private StateMachineBase _evilClownStateMachine;
        private AnimationController _animationController;
        private WanderingTimer _wanderingTimer;
        private PlayerEntity _targetPlayer;
        private Transform _lookAtTarget;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void StartServerOnly()
        {
            //DecideStateByLocation();
            GetLookAtTargetPlayer();
            EnterPrepareToChaseState();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetTargetPlayer(PlayerEntity playerEntity)
        {
            _targetPlayer = playerEntity;
            SetTargetPlayerByID(playerEntity.OwnerClientId);
        }

        public void RunAway()
        {
            EntityLocation currentLocation = GetCurrentLocation();
            
            switch (currentLocation)
            {
                case EntityLocation.Surface:
                    EnterRunAwayInSurfaceState();
                    break;

                case EntityLocation.Stairs:
                    EnterRunAwayInStairsState();
                    break;

                case EntityLocation.Dungeon:
                    EnterRunAwayInDungeonState();
                    break;

                default:
                    EnterDespawnState();
                    break;
            }
        }

        public void DecideStateByLocation()
        {
            EnterWanderingState(); // TEMP!!!!!!!!!!!!!!!!!!
        }

        [Button]
        public void EnterChaseState() => ChangeState<ChaseState>();

        [Button]
        public void EnterAttackState() => ChangeState<AttackState>();

        [Button]
        public void EnterWanderingState() => ChangeState<WanderingState>();

        public static IReadOnlyList<EvilClownEntity> GetAllEvilClowns() => AllEvilClowns;

        public PlayerEntity GetTargetPlayer() => _targetPlayer;

        public EvilClownAIConfigMeta GetEvilClownAIConfig() => _evilClownAIConfig;

        public AnimationController GetAnimationController() => _animationController;

        public WanderingTimer GetWanderingTimer() => _wanderingTimer;

        public override MonsterType GetMonsterType() =>
            MonsterType.EvilClown;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void InitAll()
        {
            SoundReproducer = new EvilClownSoundReproducer(soundProducer: this, _evilClownAIConfig);

            OnTargetPlayerChangedEvent += OnTargetPlayerChanged;
        }

        protected override void InitServerOnly()
        {
            AllEvilClowns.Add(item: this);

            InitSystems();
            SetupStates();
            
            _footstepsSystem.OnFootstepPerformedEvent += OnFootstepPerformed;

            // LOCAL METHODS: -----------------------------

            void InitSystems()
            {
                _evilClownStateMachine = new StateMachineBase();
                _animationController = new AnimationController(evilClownEntity: this, _animator);
                _wanderingTimer = new WanderingTimer(evilClownEntity: this);

                if (_targetPlayer == null)
                    _targetPlayer = PlayerEntity.GetLocalPlayer(); // TEMP!!!!!!!!!!!!!!!!!!

                _animationController.StartAnimationCheck();

                _evilClownStateMachine.OnStateChangedEvent += state =>
                {
                    string stateName = state.GetType().Name.Replace(oldValue: "State", newValue: "").GetNiceName();
                    _stateTMP.text = $"State: {stateName}";
                };
            }

            void SetupStates()
            {
                PrepareToChaseState idleState = new(evilClownEntity: this, _rig);
                ChaseState chaseState = new(evilClownEntity: this, _levelProvider);
                AttackState attackState = new(evilClownEntity: this);
                WanderingState wanderingState = new(evilClownEntity: this);
                DespawnState despawnState = new(evilClownEntity: this, _rig);
                RunAwayInDungeonState runAwayInDungeonState = new();
                RunAwayInSurfaceState runAwayInSurfaceState = new();
                RunAwayInStairsState runAwayInStairsState = new();

                _evilClownStateMachine.AddState(idleState);
                _evilClownStateMachine.AddState(chaseState);
                _evilClownStateMachine.AddState(attackState);
                _evilClownStateMachine.AddState(wanderingState);
                _evilClownStateMachine.AddState(despawnState);
                _evilClownStateMachine.AddState(runAwayInDungeonState);
                _evilClownStateMachine.AddState(runAwayInSurfaceState);
                _evilClownStateMachine.AddState(runAwayInStairsState);
            }
        }

        protected override void TickAll() => LookAtTarget();

        protected override void TickServerOnly()
        {
            _animationController.Tick();
            _evilClownStateMachine.Tick();
        }

        protected override void DespawnServerOnly()
        {
            AllEvilClowns.Remove(item: this);
            _animationController.StopAnimationCheck();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void GetLookAtTargetPlayer()
        {
            bool isPlayerFound = PlayerEntity.TryGetPlayer(TargetPlayerID, out PlayerEntity playerEntity);

            if (!isPlayerFound)
                return;

            _lookAtTarget = playerEntity.transform;
        }

        private void LookAtTarget()
        {
            if (_lookAtTarget == null)
                return;

            _lookAtObject.position = _lookAtTarget.position + _lookAtOffset;
        }
        
        [Button]
        private void EnterPrepareToChaseState() => ChangeState<PrepareToChaseState>();

        [Button]
        private void EnterDespawnState() => ChangeState<DespawnState>();

        [Button]
        private void EnterRunAwayInDungeonState() => ChangeState<RunAwayInDungeonState>();

        [Button]
        private void EnterRunAwayInSurfaceState() => ChangeState<RunAwayInSurfaceState>();

        [Button]
        private void EnterRunAwayInStairsState() => ChangeState<RunAwayInStairsState>();

        private void ChangeState<TState>() where TState : IState =>
            _evilClownStateMachine.ChangeState<TState>();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnTargetPlayerChanged(ulong playerID)
        {
            
        }
        
        private void OnFootstepPerformed(string colliderTag) => PlaySound(SFXType.Footsteps).Forget();
    }
}