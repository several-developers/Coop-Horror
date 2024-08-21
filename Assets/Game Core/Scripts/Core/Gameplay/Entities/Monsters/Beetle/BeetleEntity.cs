using System.Collections.Generic;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Monsters.Beetle.States;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Level;
using GameCore.Gameplay.Systems.Health;
using GameCore.Infrastructure.Providers.Gameplay.MonstersAI;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Entities.Monsters.Beetle
{
    public class BeetleEntity : NavmeshMonsterEntityBase, IDamageable
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

        private bool _isDead;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void StartServerOnly()
        {
            // TEMP
            if (!IsSpawned)
                NetworkObject.Spawn();
            
            DecideStateByLocation();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void TakeDamage(float damage, IEntity source = null) =>
            _healthSystem.TakeDamage(damage);

        public void KillInstant() =>
            _healthSystem.Kill();

        public void SetTargetPlayer(PlayerEntity playerEntity) =>
            _targetPlayer = playerEntity;

        public void DecideStateByLocation()
        {
            switch (EntityLocation)
            {
                case EntityLocation.Surface:
                    EnterMoveToSurfaceFireExitState();
                    break;
                
                case EntityLocation.Stairs:
                    EnterMoveToDungeonFireExitState();
                    break;
                
                case EntityLocation.Dungeon:
                    EnterIdleState();
                    break;
            }
        }
        
        public void EnterIdleState() => ChangeState<IdleState>();

        public void EnterWanderingState() => ChangeState<WanderingState>();

        public void EnterTriggerState() => ChangeState<TriggerState>();

        public void EnterScreamState() => ChangeState<ScreamState>();

        public void EnterChaseState() => ChangeState<ChaseState>();

        public void EnterAttackState() => ChangeState<AttackState>();

        public static IReadOnlyList<BeetleEntity> GetAllBeetles() => AllBeetles;

        public BeetleAIConfigMeta GetAIConfig() => _beetleAIConfig;
        
        public AggressionSystem GetAggressionSystem() => _aggressionSystem;

        public PlayerEntity GetTargetPlayer() => _targetPlayer;

        public override MonsterType GetMonsterType() =>
            MonsterType.Beetle;

        public bool TryGetCurrentState(out IState state) =>
            _beetleStateMachine.TryGetCurrentState(out state);
        
        public bool IsDead() => _isDead;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void InitServerOnly()
        {
            AllBeetles.Add(item: this);

            InitSystems();
            SetupStates();

            _healthSystem.OnHealthChangedEvent += OnHealthChanged;

            _beetleStateMachine.OnStateChangedEvent += state =>
            {
                string stateName = state.GetType().Name.Replace(oldValue: "State", newValue: "").GetNiceName();
                _stateTMP.text = $"State: {stateName}";
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
                IdleState idleState = new(beetleEntity: this);
                WanderingState wanderingState = new(beetleEntity: this);
                TriggerState triggerState = new(beetleEntity: this);
                ScreamState screamState = new(beetleEntity: this);
                ChaseState chaseState = new(beetleEntity: this);
                AttackState attackState = new(beetleEntity: this);
                DeathState deathState = new(beetleEntity: this);
                MoveToSurfaceFireExitState moveToSurfaceFireExitState = new(beetleEntity: this, _levelProvider);
                MoveToDungeonFireExitState moveToDungeonFireExitState = new(beetleEntity: this, _levelProvider);

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

        protected override void DespawnServerOnly()
        {
            AllBeetles.Remove(item: this);
            
            _healthSystem.OnHealthChangedEvent -= OnHealthChanged;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void EnterDeathState() => ChangeState<DeathState>();
        
        private void EnterMoveToSurfaceFireExitState() => ChangeState<MoveToSurfaceFireExitState>();

        private void EnterMoveToDungeonFireExitState() => ChangeState<MoveToDungeonFireExitState>();
        
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