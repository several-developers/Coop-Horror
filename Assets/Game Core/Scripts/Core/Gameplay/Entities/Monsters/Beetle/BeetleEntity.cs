using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Entities.Monsters.Beetle.States;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Network;
using GameCore.Infrastructure.Providers.Gameplay.MonstersAI;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Entities.Monsters.Beetle
{
    public class BeetleEntity : MonsterEntityBase
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IMonstersAIConfigsProvider monstersAIConfigsProvider) =>
            _beetleAIConfig = monstersAIConfigsProvider.GetBeetleAIConfig();

        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField, Required]
        private TextMeshPro _stateTMP;
        
        [SerializeField, Required]
        private TextMeshPro _aggressionTMP;
        
        [SerializeField, Required]
        private TextMeshPro _rageTMP;

        // PROPERTIES: ----------------------------------------------------------------------------

        public TextMeshPro AggressionTMP => _aggressionTMP;
        public TextMeshPro RageTMP => _rageTMP;
        
        // FIELDS: --------------------------------------------------------------------------------

        private BeetleAIConfigMeta _beetleAIConfig;
        private StateMachine _beetleStateMachine;
        private AggressionSystem _aggressionSystem;
        private PlayerEntity _targetPlayer;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Start()
        {
            if (!IsSpawned && NetworkHorror.IsTrueServer)
                NetworkObject.Spawn();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetTargetPlayer(PlayerEntity playerEntity) =>
            _targetPlayer = playerEntity;

        public void EnterIdleState() => ChangeState<IdleState>();

        public void EnterWanderingState() => ChangeState<WanderingState>();
        
        public void EnterTriggerState() => ChangeState<TriggerState>();

        public void EnterScreamState() => ChangeState<ScreamState>();
        
        public void EnterChaseState() => ChangeState<ChaseState>();
        
        public void EnterAttackState() => ChangeState<AttackState>();

        public AggressionSystem GetAggressionSystem() => _aggressionSystem;

        public PlayerEntity GetTargetPlayer() => _targetPlayer;
        
        public bool TryGetCurrentState(out IState state) =>
            _beetleStateMachine.TryGetCurrentState(out state);

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void InitServerOnly()
        {
            InitSystems();
            SetupStates();
            EnterIdleState();
        }

        protected override void TickServerOnly()
        {
            _aggressionSystem.Tick();
            _beetleStateMachine.Tick();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void InitSystems()
        {
            _beetleStateMachine = new StateMachine();
            _aggressionSystem = new AggressionSystem(beetleEntity: this, _beetleAIConfig);
            
            _beetleStateMachine.OnStateChangedEvent += state =>
            {
                string stateName = state.GetType().Name.GetNiceName();
                _stateTMP.text = $"State: {stateName}";

                string log = Log.HandleLog($"New state = <gb>{stateName}</gb>");
                Debug.Log(log);
            };
        }
        
        private void SetupStates()
        {
            IdleState idleState = new(beetleEntity: this, _beetleAIConfig);
            WanderingState wanderingState = new(beetleEntity: this, _beetleAIConfig);
            TriggerState triggerState = new(beetleEntity: this, _beetleAIConfig);
            ScreamState screamState = new(beetleEntity: this);
            ChaseState chaseState = new(beetleEntity: this, _beetleAIConfig);
            AttackState attackState = new(beetleEntity: this, _beetleAIConfig);
            DeathState deathState = new(beetleEntity: this);

            _beetleStateMachine.AddState(idleState);
            _beetleStateMachine.AddState(wanderingState);
            _beetleStateMachine.AddState(triggerState);
            _beetleStateMachine.AddState(screamState);
            _beetleStateMachine.AddState(chaseState);
            _beetleStateMachine.AddState(attackState);
            _beetleStateMachine.AddState(deathState);
        }
        
        private void ChangeState<TState>() where TState : IState =>
            _beetleStateMachine.ChangeState<TState>();
    }
}