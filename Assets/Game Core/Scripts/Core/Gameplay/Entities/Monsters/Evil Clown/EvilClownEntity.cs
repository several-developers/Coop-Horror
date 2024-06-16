using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Entities.Monsters.Beetle;
using GameCore.Gameplay.Entities.Monsters.EvilClown.States;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Level;
using GameCore.Infrastructure.Providers.Gameplay.MonstersAI;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Entities.Monsters.EvilClown
{
    public class EvilClownEntity : MonsterEntityBase
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(ILevelProvider levelProvider, IMonstersAIConfigsProvider monstersAIConfigsProvider)
        {
            _levelProvider = levelProvider;
            _evilClownAIConfig = monstersAIConfigsProvider.GetEvilClownAIConfig();
        }

        // MEMBERS: -------------------------------------------------------------------------------
        
        [SerializeField, Required]
        private TextMeshPro _stateTMP;

        // FIELDS: --------------------------------------------------------------------------------
        
        private ILevelProvider _levelProvider;
        private EvilClownAIConfigMeta _evilClownAIConfig;

        private StateMachine _evilClownStateMachine;
        private PlayerEntity _targetPlayer;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetTargetPlayer(PlayerEntity playerEntity) =>
            _targetPlayer = playerEntity;

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

        public PlayerEntity GetTargetPlayer() => _targetPlayer;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void InitServerOnly()
        {
            _targetPlayer = PlayerEntity.GetLocalPlayer(); // TEMP!!!!!!!!!!!!!!!!!!
            
            InitSystems();
            SetupStates();
            //DecideStateByLocation();
            EnterPrepareToChaseState();
            
            _evilClownStateMachine.OnStateChangedEvent += state =>
            {
                string stateName = state.GetType().Name.GetNiceName();
                _stateTMP.text = $"State: {stateName}";

                // string log = Log.HandleLog($"New state = <gb>{stateName}</gb>");
                // Debug.Log(log);
            };

            // LOCAL METHODS: -----------------------------
            
            void InitSystems()
            {
                _evilClownStateMachine = new StateMachine();
            }
            
            void SetupStates()
            {
                PrepareToChaseState idleState = new(evilClownEntity: this, _evilClownAIConfig);
                ChaseState chaseState = new(evilClownEntity: this, _evilClownAIConfig);
                AttackState attackState = new(evilClownEntity: this, _evilClownAIConfig);
                WanderingState wanderingState = new(evilClownEntity: this, _evilClownAIConfig);

                _evilClownStateMachine.AddState(idleState);
                _evilClownStateMachine.AddState(chaseState);
                _evilClownStateMachine.AddState(attackState);
                _evilClownStateMachine.AddState(wanderingState);
            }
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        [Button]
        private void EnterPrepareToChaseState() => ChangeState<PrepareToChaseState>();
        
        private void ChangeState<TState>() where TState : IState =>
            _evilClownStateMachine.ChangeState<TState>();
    }
}