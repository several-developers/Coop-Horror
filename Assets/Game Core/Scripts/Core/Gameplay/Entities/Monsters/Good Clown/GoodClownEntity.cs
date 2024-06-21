﻿using System.Collections.Generic;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Entities.Monsters.GoodClown.States;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Level;
using GameCore.Infrastructure.Providers.Gameplay.MonstersAI;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations.Rigging;
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
            _clownTransformationSystem = new TransformationSystem(goodClownEntity: this);
            _hunterSystem = new HunterSystem(goodClownEntity: this);
            _clownUtilities = new GoodClownUtilities(goodClownEntity: this, _animator);
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

        // FIELDS: --------------------------------------------------------------------------------

        private static readonly List<GoodClownEntity> AllGoodClowns = new();

        private ILevelProvider _levelProvider;
        private GoodClownAIConfigMeta _goodClownAIConfig;

        private StateMachine _goodClownStateMachine;
        private PlayerEntity _targetPlayer;
        private TransformationSystem _clownTransformationSystem;
        private HunterSystem _hunterSystem;
        private GoodClownUtilities _clownUtilities;

        private bool _isReleasedBalloon;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetTargetPlayer(PlayerEntity playerEntity) =>
            _targetPlayer = playerEntity;

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

        public static IReadOnlyList<GoodClownEntity> GetAllGoodClowns() => AllGoodClowns;

        public GoodClownAIConfigMeta GetGoodClownAIConfig() => _goodClownAIConfig;
        
        public PlayerEntity GetTargetPlayer() => _targetPlayer;

        public HunterSystem GetHunterSystem() => _hunterSystem;

        public GoodClownUtilities GetClownUtilities() => _clownUtilities;
        
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
            //DecideStateByLocation();
            EnterSearchForTargetState();
            
            // LOCAL METHODS: -----------------------------

            void InitSystems()
            {
                _goodClownStateMachine = new StateMachine();

                _goodClownStateMachine.OnStateChangedEvent += state =>
                {
                    string stateName = state.GetType().Name.Replace(oldValue: "State", newValue: "").GetNiceName();
                    _stateTMP.text = $"State: {stateName}";
                };
            }

            void SetupStates()
            {
                SearchForTargetState searchForTargetState = new(goodClownEntity: this);
                FollowTargetState followTargetState = new(goodClownEntity: this);
                WanderingAroundTargetState wanderingAroundTargetState = new(goodClownEntity: this);
                HuntingIdleState huntingIdleState = new(goodClownEntity: this);
                HuntingChaseState huntingChaseState = new(goodClownEntity: this);
                RespawnAsEvilClownState respawnAsEvilClownState = new(goodClownEntity: this);

                _goodClownStateMachine.AddState(searchForTargetState);
                _goodClownStateMachine.AddState(followTargetState);
                _goodClownStateMachine.AddState(wanderingAroundTargetState);
                _goodClownStateMachine.AddState(huntingIdleState);
                _goodClownStateMachine.AddState(huntingChaseState);
                _goodClownStateMachine.AddState(respawnAsEvilClownState);
            }
        }

        protected override void TickServerOnly() =>
            _goodClownStateMachine.Tick();

        protected override void DespawnServerOnly() =>
            AllGoodClowns.Remove(item: this);

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ChangeState<TState>() where TState : IState =>
            _goodClownStateMachine.ChangeState<TState>();

        // RPC: -----------------------------------------------------------------------------------

        [ServerRpc(RequireOwnership = false)]
        public void ReleaseBalloonServerRpc()
        {
            if (_isReleasedBalloon)
                return;

            _isReleasedBalloon = true;
            ReleaseBalloonClientRpc();
        }

        [ClientRpc]
        private void ReleaseBalloonClientRpc()
        {
            _rightHandRig.weight = 0f;
            
            if (_balloon != null)
                _balloon.Release();
        }
    }
}