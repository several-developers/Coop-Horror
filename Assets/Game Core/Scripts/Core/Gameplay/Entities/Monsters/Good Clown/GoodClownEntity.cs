using System.Collections.Generic;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Entities.Monsters.GoodClown.States;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Level;
using GameCore.Infrastructure.Providers.Gameplay.MonstersAI;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Entities.Monsters.GoodClown
{
    public class GoodClownEntity : MonsterEntityBase
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(ILevelProvider levelProvider, IMonstersAIConfigsProvider monstersAIConfigsProvider)
        {
            _levelProvider = levelProvider;
            _goodClownAIConfig = monstersAIConfigsProvider.GetGoodClownAIConfig();
            _clownUtilities = new GoodClownUtilities(this, _animator);
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField, Required]
        private Animator _animator;
        
        [SerializeField, Required]
        private GameObject _balloonObject;
        
        [SerializeField, Required]
        private TextMeshPro _stateTMP;

        // FIELDS: --------------------------------------------------------------------------------

        private static readonly List<GoodClownEntity> AllGoodClowns = new();

        private ILevelProvider _levelProvider;
        private GoodClownAIConfigMeta _goodClownAIConfig;

        private StateMachine _goodClownStateMachine;
        private PlayerEntity _targetPlayer;
        private GoodClownUtilities _clownUtilities;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetTargetPlayer(PlayerEntity playerEntity) =>
            _targetPlayer = playerEntity;

        [Button]
        public void EnterSearchForTargetState() => ChangeState<SearchForTargetState>();
        
        [Button]
        public void EnterMoveToTargetState() => ChangeState<MoveToTargetState>();

        [Button]
        public void EnterWanderingAroundTargetState() => ChangeState<WanderingAroundTargetState>();
        
        public static IReadOnlyList<GoodClownEntity> GetAllGoodClowns() => AllGoodClowns;

        public GoodClownAIConfigMeta GetGoodClownAIConfig() => _goodClownAIConfig;
        
        public PlayerEntity GetTargetPlayer() => _targetPlayer;

        public GoodClownUtilities GetClownUtilities() => _clownUtilities;
        
        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void InitServerOnly()
        {
            base.InitServerOnly();

            AllGoodClowns.Add(item: this);

            InitSystems();
            SetupStates();
            //DecideStateByLocation();
            EnterSearchForTargetState();
            
            // LOCAL METHODS: -----------------------------

            void InitSystems()
            {
                _goodClownStateMachine = new StateMachine();

                _goodClownStateMachine.OnStateChangedEvent += state =>
                {
                    string stateName = state.GetType().Name.GetNiceName();
                    _stateTMP.text = $"State: {stateName}";

                    string log = Log.HandleLog($"Good Clown new state = <gb>{stateName}</gb>");
                    Debug.Log(log);
                };
            }

            void SetupStates()
            {
                IdleState idleState = new(goodClownEntity: this);
                SearchForTargetState searchForTargetState = new(goodClownEntity: this);
                MoveToTargetState moveToTargetState = new(goodClownEntity: this);
                WanderingAroundTargetState wanderingAroundTargetState = new(goodClownEntity: this);

                _goodClownStateMachine.AddState(idleState);
                _goodClownStateMachine.AddState(searchForTargetState);
                _goodClownStateMachine.AddState(moveToTargetState);
                _goodClownStateMachine.AddState(wanderingAroundTargetState);
            }
        }

        protected override void TickServerOnly() =>
            _goodClownStateMachine.Tick();

        protected override void DespawnServerOnly()
        {
            base.DespawnServerOnly();

            AllGoodClowns.Remove(item: this);
        }
        
        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ChangeState<TState>() where TState : IState =>
            _goodClownStateMachine.ChangeState<TState>();
    }
}