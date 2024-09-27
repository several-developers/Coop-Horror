using System.Collections.Generic;
using GameCore.Infrastructure.Configs.Gameplay.Enemies;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Monsters.GoodClown.States;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Factories.Monsters;
using GameCore.Gameplay.Level;
using GameCore.Infrastructure.Providers.Gameplay.MonstersAI;
using GameCore.Infrastructure.StateMachine;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Zenject;

namespace GameCore.Gameplay.Entities.Monsters.GoodClown
{
    public class GoodClownEntity : NavmeshMonsterEntityBase
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(
            ILevelProvider levelProvider,
            IMonstersFactory monstersFactory,
            IMonstersAIConfigsProvider monstersAIConfigsProvider
        )
        {
            _levelProvider = levelProvider;
            _monstersFactory = monstersFactory;
            _goodClownAIConfig = monstersAIConfigsProvider.GetConfig<GoodClownAIConfigMeta>();
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField, Required]
        private Animator _animator;

        [SerializeField, Required]
        private Balloon _balloon;

        [SerializeField, Required]
        private TwoBoneIKConstraint _rightHandRig;

        [SerializeField, Required]
        private TextMeshPro _stateTMP;

        // PROPERTIES: ----------------------------------------------------------------------------

        public bool IsInnocent { get; private set; } = true;

        // FIELDS: --------------------------------------------------------------------------------

        private static readonly List<GoodClownEntity> AllGoodClowns = new();

        private ILevelProvider _levelProvider;
        private IMonstersFactory _monstersFactory;
        private GoodClownAIConfigMeta _goodClownAIConfig;

        private StateMachineBase _goodClownStateMachine;
        private PlayerEntity _targetPlayer;
        private HunterSystem _hunterSystem;
        private GoodClownUtilities _clownUtilities;

        private bool _isBalloonReleased;
        private bool _isBalloonReset;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void StartServerOnly()
        {
            // TEMP
            if (!IsSpawned)
                NetworkObject.Spawn();
            
            //DecideStateByLocation();
            EnterSearchForTargetState();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetTargetPlayer(PlayerEntity playerEntity) =>
            _targetPlayer = playerEntity;

        public void ToggleInnocent(bool isInnocent) =>
            IsInnocent = isInnocent;

        [Button]
        public void EnterSearchForTargetState() => ChangeState<SearchForTargetState>();

        [Button]
        public void EnterFollowTargetState() => ChangeState<FollowTargetState>();

        [Button]
        public void EnterWanderingAroundTargetState() => ChangeState<WanderingAroundTargetState>();

        [Button]
        public void EnterHuntingIdleState() => ChangeState<HuntingIdleState>();

        [Button]
        public void EnterHuntingChaseState() => ChangeState<HuntingChaseState>();

        [Button]
        public void EnterRespawnAsEvilClownState() => ChangeState<RespawnAsEvilClownState>();

        [Button]
        public void EnterDeathState() => ChangeState<DeathState>();

        public static IReadOnlyList<GoodClownEntity> GetAllGoodClowns() => AllGoodClowns;

        public GoodClownAIConfigMeta GetGoodClownAIConfig() => _goodClownAIConfig;

        public PlayerEntity GetTargetPlayer() => _targetPlayer;

        public HunterSystem GetHunterSystem() => _hunterSystem;

        public GoodClownUtilities GetClownUtilities() => _clownUtilities;

        public override MonsterType GetMonsterType() =>
            MonsterType.GoodClown;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void InitAll()
        {
            InitBalloon();

            // LOCAL METHODS: -----------------------------

            void InitBalloon()
            {
                _balloon.Init();
                _rightHandRig.weight = 1f;
            }
        }

        protected override void InitServerOnly()
        {
            AllGoodClowns.Add(item: this);

            InitSystems();
            SetupStates();

            // LOCAL METHODS: -----------------------------

            void InitSystems()
            {
                _goodClownStateMachine = new StateMachineBase();
                _hunterSystem = new HunterSystem(goodClownEntity: this);
                _clownUtilities = new GoodClownUtilities(goodClownEntity: this, _animator);

                _goodClownStateMachine.OnStateChangedEvent += state =>
                {
                    string stateName = state.GetType().Name.Replace(oldValue: "State", newValue: "").GetNiceName();
                    _stateTMP.text = $"State: {stateName}";
                };
            }

            void SetupStates()
            {
                SearchForTargetState searchForTargetState = new(goodClownEntity: this);
                FollowTargetState followTargetState = new(goodClownEntity: this, _levelProvider);
                WanderingAroundTargetState wanderingAroundTargetState = new(goodClownEntity: this);
                HuntingIdleState huntingIdleState = new(goodClownEntity: this);
                HuntingChaseState huntingChaseState = new(goodClownEntity: this, _levelProvider);
                RespawnAsEvilClownState respawnAsEvilClownState = new(goodClownEntity: this, _monstersFactory);
                DeathState deathState = new(goodClownEntity: this);

                _goodClownStateMachine.AddState(searchForTargetState);
                _goodClownStateMachine.AddState(followTargetState);
                _goodClownStateMachine.AddState(wanderingAroundTargetState);
                _goodClownStateMachine.AddState(huntingIdleState);
                _goodClownStateMachine.AddState(huntingChaseState);
                _goodClownStateMachine.AddState(respawnAsEvilClownState);
                _goodClownStateMachine.AddState(deathState);
            }
        }

        protected override void TickServerOnly()
        {
            _hunterSystem.Tick();
            _goodClownStateMachine.Tick();
        }

        protected override void DespawnServerOnly()
        {
            AllGoodClowns.Remove(item: this);
            _hunterSystem.Destroy();
            _balloon.Reset();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ChangeState<TState>() where TState : IState =>
            _goodClownStateMachine.ChangeState<TState>();

        // RPC: -----------------------------------------------------------------------------------

        [ServerRpc(RequireOwnership = false)]
        public void ReleaseBalloonServerRpc()
        {
            if (_isBalloonReleased)
                return;

            _isBalloonReleased = true;
            ReleaseBalloonClientRpc();
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void ResetBalloonServerRpc()
        {
            if (_isBalloonReset)
                return;

            _isBalloonReset = true;
            ResetBalloonClientRpc();
        }

        [ClientRpc]
        private void ReleaseBalloonClientRpc()
        {
            _rightHandRig.weight = 0f;

            if (_balloon != null)
                _balloon.Release();
        }

        [ClientRpc]
        private void ResetBalloonClientRpc()
        {
            if (_balloon != null)
                _balloon.Reset();
        }
    }
}