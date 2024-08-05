using System.Collections.Generic;
using GameCore.Configs.Gameplay.Balance;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Monsters.BlindCreature.States;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Systems.Noise;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using GameCore.Infrastructure.Providers.Gameplay.MonstersAI;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Entities.Monsters.BlindCreature
{
    public class BlindCreatureEntity : MonsterEntityBase, INoiseListener
    {
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

        // FIELDS: --------------------------------------------------------------------------------

        private static readonly List<BlindCreatureEntity> AllBlindCreatures = new();

        private BlindCreatureAIConfigMeta _blindCreatureAIConfig;
        private BalanceConfigMeta _balanceConfig;

        private StateMachine _blindCreatureStateMachine;
        private SuspicionSystem _suspicionSystem;
        private PlayerEntity _targetPlayer;

        private bool _isDead;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Start()
        {
            if (!IsServerOnly)
                return;

            // TEMP
            if (!IsSpawned)
                NetworkObject.Spawn();

            EnterIdleState();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetTargetPlayer(PlayerEntity playerEntity) =>
            _targetPlayer = playerEntity;

        public void DetectNoise(Vector3 noisePosition, float noiseLoudness) =>
            _suspicionSystem.DetectNoise(noisePosition, noiseLoudness);

        public void EnterIdleState() => ChangeState<IdleState>();

        public void EnterWanderingState() => ChangeState<WanderingState>();
        
        public void EnterMoveToSuspicionPlaceState() => ChangeState<MoveToSuspicionPlaceState>();
        
        public void EnterLookAroundSuspicionPlaceState() => ChangeState<LookAroundSuspicionPlaceState>();

        public static IReadOnlyList<BlindCreatureEntity> GetAllBlindCreatures() => AllBlindCreatures;

        public BlindCreatureAIConfigMeta GetAIConfig() => _blindCreatureAIConfig;

        public SuspicionSystem GetSuspicionSystem() => _suspicionSystem;

        public override MonsterType GetMonsterType() =>
            MonsterType.BlindCreature;

        // PROTECTED METHODS: ---------------------------------------------------------------------

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

        protected override void TickServerOnly() =>
            _blindCreatureStateMachine.Tick();

        protected override void DespawnServerOnly() =>
            AllBlindCreatures.Remove(item: this);

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ChangeState<TState>() where TState : IState =>
            _blindCreatureStateMachine.ChangeState<TState>();
    }
}