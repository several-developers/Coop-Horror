using System.Collections.Generic;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Dungeons;
using GameCore.Gameplay.Entities.Monsters.Beetle.States;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.EntitiesSystems.Health;
using GameCore.Gameplay.Level;
using GameCore.Gameplay.Network;
using GameCore.Infrastructure.Providers.Gameplay.MonstersAI;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Entities.Monsters.Beetle
{
    public class BeetleEntity : MonsterEntityBase, IDamageable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(ILevelProvider levelProvider, IMonstersAIConfigsProvider monstersAIConfigsProvider)
        {
            _levelProvider = levelProvider;
            _beetleAIConfig = monstersAIConfigsProvider.GetBeetleAIConfig();
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField, Required]
        private HealthSystem _healthSystem;
        
        [SerializeField, Required, Space(height: 5)]
        private TextMeshPro _stateTMP;

        [SerializeField, Required]
        private TextMeshPro _aggressionTMP;

        [SerializeField, Required]
        private TextMeshPro _rageTMP;

        // PROPERTIES: ----------------------------------------------------------------------------

        public TextMeshPro AggressionTMP => _aggressionTMP;
        public TextMeshPro RageTMP => _rageTMP;

        // FIELDS: --------------------------------------------------------------------------------

        private static readonly List<BeetleEntity> AllBeetles = new();

        private ILevelProvider _levelProvider;

        private BeetleAIConfigMeta _beetleAIConfig;
        private StateMachine _beetleStateMachine;
        private AggressionSystem _aggressionSystem;
        private PlayerEntity _targetPlayer;

        private Floor _dungeonFloor;
        private bool _isOutside = true;
        private bool _isDead;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();
            CheckIfOutside();
        }

        private void Start()
        {
            if (!IsSpawned && NetworkHorror.IsTrueServer)
                NetworkObject.Spawn();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void TakeDamage(float damage, IEntity source = null) =>
            _healthSystem.TakeDamage(damage);

        public void SetTargetPlayer(PlayerEntity playerEntity) =>
            _targetPlayer = playerEntity;

        public void ToggleOutsideState(bool isOutside) =>
            _isOutside = isOutside;

        public void EnterIdleState() => ChangeState<IdleState>();

        public void EnterWanderingState() => ChangeState<WanderingState>();

        public void EnterTriggerState() => ChangeState<TriggerState>();

        public void EnterScreamState() => ChangeState<ScreamState>();

        public void EnterChaseState() => ChangeState<ChaseState>();

        public void EnterAttackState() => ChangeState<AttackState>();

        public void EnterMoveToSurfaceFireExitState() => ChangeState<MoveToSurfaceFireExitState>();

        public void EnterMoveToDungeonFireExitState() => ChangeState<MoveToDungeonFireExitState>();

        public static IReadOnlyList<BeetleEntity> GetAllBeetles() => AllBeetles;

        public AggressionSystem GetAggressionSystem() => _aggressionSystem;

        public PlayerEntity GetTargetPlayer() => _targetPlayer;

        public Floor GetDungeonFloor() => _dungeonFloor;

        public bool TryGetCurrentState(out IState state) =>
            _beetleStateMachine.TryGetCurrentState(out state);
        
        public bool IsDead() => _isDead;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void InitServerOnly()
        {
            AllBeetles.Add(item: this);

            InitSystems();
            SetupStates();

            if (_isOutside)
                EnterMoveToSurfaceFireExitState();
            else
                EnterIdleState();

            _healthSystem.OnHealthChangedEvent += OnHealthChanged;

            _beetleStateMachine.OnStateChangedEvent += state =>
            {
                string stateName = state.GetType().Name.GetNiceName();
                _stateTMP.text = $"State: {stateName}";

                // string log = Log.HandleLog($"New state = <gb>{stateName}</gb>");
                // Debug.Log(log);
            };

            // LOCAL METHODS: -----------------------------
            
            void InitSystems()
            {
                _beetleStateMachine = new StateMachine();
                _aggressionSystem = new AggressionSystem(beetleEntity: this, _beetleAIConfig);

                float health = _beetleAIConfig.Health;
                _healthSystem.Setup(health);
            }
            
            void SetupStates()
            {
                IdleState idleState = new(beetleEntity: this, _beetleAIConfig);
                WanderingState wanderingState = new(beetleEntity: this, _beetleAIConfig);
                TriggerState triggerState = new(beetleEntity: this, _beetleAIConfig);
                ScreamState screamState = new(beetleEntity: this, _beetleAIConfig);
                ChaseState chaseState = new(beetleEntity: this, _beetleAIConfig);
                AttackState attackState = new(beetleEntity: this, _beetleAIConfig);
                DeathState deathState = new(beetleEntity: this);

                MoveToSurfaceFireExitState moveToSurfaceFireExitState =
                    new(beetleEntity: this, _beetleAIConfig, _levelProvider);
            
                MoveToDungeonFireExitState moveToDungeonFireExitState =
                    new(beetleEntity: this, _beetleAIConfig, _levelProvider);

                _beetleStateMachine.AddState(idleState);
                _beetleStateMachine.AddState(wanderingState);
                _beetleStateMachine.AddState(triggerState);
                _beetleStateMachine.AddState(screamState);
                _beetleStateMachine.AddState(chaseState);
                _beetleStateMachine.AddState(attackState);
                _beetleStateMachine.AddState(deathState);
                _beetleStateMachine.AddState(moveToSurfaceFireExitState);
                _beetleStateMachine.AddState(moveToDungeonFireExitState);
            }
        }

        protected override void TickServerOnly()
        {
            _aggressionSystem.Tick();
            _beetleStateMachine.Tick();
        }

        protected override void DespawnServerOnly() =>
            _healthSystem.OnHealthChangedEvent -= OnHealthChanged;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CheckIfOutside()
        {
            Transform parent = transform.parent;

            while (parent != null)
            {
                bool isDungeonRootFound = parent.TryGetComponent(out DungeonRoot dungeonRoot);

                parent = parent.parent;

                if (!isDungeonRootFound)
                    continue;

                _dungeonFloor = dungeonRoot.Floor;
                _isOutside = false;
                break;
            }

            if (!_isOutside)
                return;

            int randomFloor = Random.Range(1, 4);
            _dungeonFloor = (Floor)randomFloor;
        }

        private void EnterDeathState() => ChangeState<DeathState>();
        
        private void ChangeState<TState>() where TState : IState =>
            _beetleStateMachine.ChangeState<TState>();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnHealthChanged(HealthData healthData)
        {
            if (_isDead)
                return;
            
            float currentHealth = healthData.CurrentHealth;
            bool isDead = Mathf.Approximately(a: currentHealth, b: 0f);
            
            if (!isDead)
                return;

            _isDead = true;
            EnterDeathState();
        }

        // DEBUG BUTTONS: -------------------------------------------------------------------------

        [Title(Constants.DebugButtons)]
        [Button(buttonSize: 30, ButtonStyle.FoldoutButton), DisableInEditorMode]
        private void DebugToggleOutsideState(bool isOutside) => ToggleOutsideState(isOutside);

        [Button(buttonSize: 30), DisableInEditorMode]
        private void DebugEnterIdleState() => EnterIdleState();

        [Button(buttonSize: 30), DisableInEditorMode]
        private void DebugEnterWanderingState() => EnterWanderingState();

        [Button(buttonSize: 30), DisableInEditorMode]
        private void DebugEnterTriggerState() => EnterTriggerState();

        [Button(buttonSize: 30), DisableInEditorMode]
        private void DebugEnterScreamState() => EnterScreamState();

        [Button(buttonSize: 30), DisableInEditorMode]
        private void DebugEnterChaseState() => EnterChaseState();

        [Button(buttonSize: 30), DisableInEditorMode]
        private void DebugEnterAttackState() => EnterAttackState();

        [Button(buttonSize: 30), DisableInEditorMode]
        private void DebugEnterMoveToStairsState() => EnterMoveToSurfaceFireExitState();

        [Button(buttonSize: 30), DisableInEditorMode]
        private void DebugEnterMoveToDungeonFireExitState() => EnterMoveToDungeonFireExitState();
    }
}