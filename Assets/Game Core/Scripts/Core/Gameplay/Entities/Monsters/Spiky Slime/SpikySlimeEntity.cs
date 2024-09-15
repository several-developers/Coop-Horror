using System.Collections.Generic;
using GameCore.Configs.Gameplay.Balance;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Monsters.SpikySlime.States;
using GameCore.Gameplay.Systems.Noise;
using GameCore.Gameplay.Systems.SoundReproducer;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using GameCore.Infrastructure.Providers.Gameplay.MonstersAI;
using GameCore.Infrastructure.StateMachine;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Entities.Monsters.SpikySlime
{
    public class SpikySlimeEntity : SoundProducerNavmeshMonsterEntity<SpikySlimeEntity.SFXType>, INoiseListener
    {
        public enum SFXType
        {
        }

        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(
            IGameplayConfigsProvider gameplayConfigsProvider,
            IMonstersAIConfigsProvider monstersAIConfigsProvider
        )
        {
            _balanceConfig = gameplayConfigsProvider.GetConfig<BalanceConfigMeta>();
            _spikySlimeAIConfig = monstersAIConfigsProvider.GetConfig<SpikySlimeAIConfigMeta>();
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [BoxGroup(Constants.References, showLabel: false), SerializeField]
        private SpikySlimeReferences _references;

        // FIELDS: --------------------------------------------------------------------------------

        private static readonly List<SpikySlimeEntity> AllSpikySlimes = new();

        private BalanceConfigMeta _balanceConfig;
        private SpikySlimeAIConfigMeta _spikySlimeAIConfig;

        private StateMachineBase _spikySlimeStateMachine;
        private AggressionSystem _aggressionSystem;
        private AttackSystem _attackSystem;
        private AnimationController _animationController;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void StartServerOnly() =>
            EnterWanderingState();

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void DetectNoise(Vector3 noisePosition, float noiseLoudness) =>
            _aggressionSystem.DetectNoise(noisePosition, noiseLoudness);

        public static IReadOnlyList<SpikySlimeEntity> GetAllSpikySlimes() => AllSpikySlimes;
        public SpikySlimeAIConfigMeta GetAIConfig() => _spikySlimeAIConfig;
        public SpikySlimeReferences GetReferences() => _references;
        public AggressionSystem GetAggressionSystem() => _aggressionSystem;

        public override MonsterType GetMonsterType() =>
            MonsterType.SpikySlime;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void InitServerOnly()
        {
            AllSpikySlimes.Add(item: this);

            SetupSystems();
            SetupStates();

            _aggressionSystem.OnAggressionMeterChangedEvent += aggressionMeter =>
            {
                _references.InfoTMP.text = $"Aggression Meter: {aggressionMeter}";
            };

            _aggressionSystem.OnStartAttackEvent += StartAttack;

            // LOCAL METHODS: -----------------------------

            void SetupSystems()
            {
                _spikySlimeStateMachine = new StateMachineBase();
                _aggressionSystem = new AggressionSystem(spikySlimeEntity: this, _balanceConfig);
                _attackSystem = new AttackSystem(spikySlimeEntity: this, _aggressionSystem);
                _animationController = new AnimationController(spikySlimeEntity: this);
            }

            void SetupStates()
            {
                WanderingState wanderingState = new(spikySlimeEntity: this);

                _spikySlimeStateMachine.AddState(wanderingState);
            }
        }

        protected override void TickServerOnly()
        {
            _attackSystem.Tick();
            _spikySlimeStateMachine.Tick();
        }

        protected override void DespawnServerOnly()
        {
            AllSpikySlimes.Remove(item: this);
            
            _aggressionSystem.OnStartAttackEvent -= StartAttack;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void StartAttack() =>
            _attackSystem.StartAttack();

        private void EnterWanderingState() => ChangeState<WanderingState>();

        private void ChangeState<TState>() where TState : IState =>
            _spikySlimeStateMachine.ChangeState<TState>();

        // DEBUG BUTTONS: -------------------------------------------------------------------------
        
        [Title(Constants.DebugButtons)]
        [Button(buttonSize: 25), DisableInEditorMode]
        private void DebugStartAttack() => StartAttack();
    }
}