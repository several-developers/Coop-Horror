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
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Entities.Monsters.SpikySlime
{
    [GenerateSerializationForType(typeof(SFXType))]
    public class SpikySlimeEntity : SoundProducerNavmeshMonsterEntity<SpikySlimeEntity.SFXType>, INoiseListener
    {
        public enum SFXType
        {
            // None = 0,
            CalmMovement = 1,
            AngryMovement = 2,
            Calming = 3,
            Angry = 4,
            Attack = 5,
            Stab = 6
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
        private SizeController _sizeController;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void StartServerOnly() => EnterWanderingState();

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void DetectNoise(Vector3 noisePosition, float noiseLoudness) =>
            _aggressionSystem.DetectNoise(noisePosition, noiseLoudness);

        public void UpdateAggressionAnimation(int aggressionMeter) => UpdateAggressionAnimationRpc(aggressionMeter);

        public void PlayAttackAnimation() => PlayAttackAnimationRpc();

        public void PlayHideSpikesAnimation() => PlayHideSpikesAnimationRpc();

        public static IReadOnlyList<SpikySlimeEntity> GetAllSpikySlimes() => AllSpikySlimes;
        public SpikySlimeAIConfigMeta GetAIConfig() => _spikySlimeAIConfig;
        public SpikySlimeReferences GetReferences() => _references;
        public AggressionSystem GetAggressionSystem() => _aggressionSystem;
        public AnimationController GetAnimationController() => _animationController;

        public override MonsterType GetMonsterType() =>
            MonsterType.SpikySlime;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void InitAll()
        {
            InitSystems();

            // LOCAL METHODS: -----------------------------

            void InitSystems()
            {
                SoundReproducer = new SpikySlimeSoundReproducer(soundProducer: this, _spikySlimeAIConfig);
                _sizeController = new SizeController(spikySlimeEntity: this);
                _animationController = new AnimationController(spikySlimeEntity: this);
            }
        }

        protected override void InitServerOnly()
        {
            AllSpikySlimes.Add(item: this);

            SetupSystems();
            SetupStates();
            ChangeSlimeSize();

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
                _attackSystem = new AttackSystem(spikySlimeEntity: this);
            }

            void SetupStates()
            {
                WanderingState wanderingState = new(spikySlimeEntity: this);

                _spikySlimeStateMachine.AddState(wanderingState);
            }

            void ChangeSlimeSize()
            {
                float slimeScale = _sizeController.GetRandomSlimeScale();
                ChangeSlimeSizeRpc(slimeScale);
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

        // RPC: -----------------------------------------------------------------------------------

        [Rpc(target: SendTo.Everyone)]
        private void ChangeSlimeSizeRpc(float slimeScale) =>
            _sizeController.ChangeSize(slimeScale);

        [Rpc(target: SendTo.Everyone)]
        private void UpdateAggressionAnimationRpc(int aggressionMeter) =>
            _animationController.UpdateAggressionAnimation(aggressionMeter);

        [Rpc(target: SendTo.Everyone)]
        private void PlayAttackAnimationRpc() =>
            _animationController.PlayAttackAnimation();

        [Rpc(target: SendTo.Everyone)]
        private void PlayHideSpikesAnimationRpc() =>
            _animationController.PlayHideSpikesAnimation();

        // DEBUG BUTTONS: -------------------------------------------------------------------------
        
        [Title(Constants.DebugButtons)]
        [Button(buttonSize: 25), DisableInEditorMode]
        private void DebugStartAttack() => StartAttack();
    }
}