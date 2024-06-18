﻿using System.Collections.Generic;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Enums.Gameplay;
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

        private static readonly List<EvilClownEntity> AllEvilClowns = new();

        private ILevelProvider _levelProvider;
        private EvilClownAIConfigMeta _evilClownAIConfig;

        private StateMachine _evilClownStateMachine;
        private WanderingTimer _wanderingTimer;
        private PlayerEntity _targetPlayer;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetTargetPlayer(PlayerEntity playerEntity) =>
            _targetPlayer = playerEntity;

        public void RunAway()
        {
            switch (EntityLocation)
            {
                case EntityLocation.LocationSurface:
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

        public WanderingTimer GetWanderingTimer() => _wanderingTimer;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void InitServerOnly()
        {
            base.InitServerOnly();

            AllEvilClowns.Add(item: this);

            InitSystems();
            SetupStates();
            //DecideStateByLocation();
            EnterPrepareToChaseState();

            // LOCAL METHODS: -----------------------------

            void InitSystems()
            {
                _evilClownStateMachine = new StateMachine();
                _wanderingTimer = new WanderingTimer(evilClownEntity: this);
                _targetPlayer = PlayerEntity.GetLocalPlayer(); // TEMP!!!!!!!!!!!!!!!!!!

                _evilClownStateMachine.OnStateChangedEvent += state =>
                {
                    string stateName = state.GetType().Name.GetNiceName();
                    _stateTMP.text = $"State: {stateName}";
                };
            }

            void SetupStates()
            {
                PrepareToChaseState idleState = new(evilClownEntity: this);
                ChaseState chaseState = new(evilClownEntity: this);
                AttackState attackState = new(evilClownEntity: this);
                WanderingState wanderingState = new(evilClownEntity: this);
                DespawnState despawnState = new(evilClownEntity: this);
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

        protected override void TickServerOnly() =>
            _evilClownStateMachine.Tick();

        protected override void DespawnServerOnly()
        {
            base.DespawnServerOnly();

            AllEvilClowns.Remove(item: this);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

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
    }
}