using System.Collections.Generic;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Monsters.Mushroom.States;
using GameCore.Gameplay.Systems.SoundReproducer;
using GameCore.Infrastructure.Providers.Gameplay.MonstersAI;
using GameCore.Infrastructure.StateMachine;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Entities.Monsters.Mushroom
{
    [GenerateSerializationForType(typeof(SFXType))]
    public class MushroomEntity : SoundProducerNavmeshMonsterEntity<MushroomEntity.SFXType>
    {
        public enum SFXType
        {
            
        }

        public enum Emotion
        {
            Regular = 0,
            Happy = 1,
            Angry = 2,
            Scared = 3,
            Interested = 4,
            Sigma = 5,
            Dead = 6
        }

        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IMonstersAIConfigsProvider monstersAIConfigsProvider)
        {
            _mushroomAIConfig = monstersAIConfigsProvider.GetConfig<MushroomAIConfigMeta>();
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField, Range(0, 1)]
        private int _happiness = 1;
        
        [SerializeField, Range(0, 1)]
        private int _isSneaking;

        public int Happiness => _happiness;
        public int IsSneaking => _isSneaking;

        [BoxGroup(Constants.References, showLabel: false), SerializeField]
        private MushroomReferences _references;
        
        // FIELDS: --------------------------------------------------------------------------------

        private static readonly List<MushroomEntity> AllMushrooms = new();

        private readonly NetworkVariable<bool> _isHatDamaged = new(writePerm: Constants.OwnerPermission);
        private readonly NetworkVariable<bool> _isHiding = new(writePerm: Constants.OwnerPermission);

        private MushroomAIConfigMeta _mushroomAIConfig;

        private StateMachineBase _mushroomStateMachine;
        private AnimationController _animationController;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void StartServerOnly() => EnterIdleState();

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void DamageHat() => ChangeHatStateRpc(isHatDamaged: true);

        public void RegenerateHat() => ChangeHatStateRpc(isHatDamaged: false);

        public void ToggleHidingState(bool isHiding) => ToggleHidingStateRpc(isHiding);

        [Button]
        public void EnterIdleState() => ChangeState<IdleState>();
        
        [Button]
        public void EnterWanderingState() => ChangeState<WanderingState>();
        
        [Button]
        public void EnterHidingState() => ChangeState<HidingState>();
        
        public MushroomAIConfigMeta GetAIConfig() => _mushroomAIConfig;

        public MushroomReferences GetReferences() => _references;

        public AnimationController GetAnimationController() => _animationController;

        public override MonsterType GetMonsterType() =>
            MonsterType.Mushroom;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void InitAll()
        {
            _animationController = new AnimationController(mushroomEntity: this);

            _isHatDamaged.OnValueChanged += OnHatStateChanged;
            _isHiding.OnValueChanged += OnHidingStateChanged;
        }

        protected override void InitServerOnly()
        {
            AllMushrooms.Add(item: this);
            
            InitSystems();
            SetupStates();

            // LOCAL METHODS: -----------------------------

            void InitSystems()
            {
                _mushroomStateMachine = new StateMachineBase();

                _mushroomStateMachine.OnStateChangedEvent += state =>
                {
                    string log = Log.HandleLog($"New state '<gb>{state}</gb>'");
                    Debug.Log(log);
                };
            }

            void SetupStates()
            {
                IdleState idleState = new(mushroomEntity: this);
                WanderingState wanderingState = new(mushroomEntity: this);
                HidingState hidingState = new(mushroomEntity: this);
                
                _mushroomStateMachine.AddState(idleState);
                _mushroomStateMachine.AddState(wanderingState);
                _mushroomStateMachine.AddState(hidingState);
            }
        }

        protected override void TickServerOnly()
        {
            _animationController.Tick();
            _mushroomStateMachine.Tick();
        }

        protected override void DespawnAll()
        {
            _isHatDamaged.OnValueChanged -= OnHatStateChanged;
            _isHiding.OnValueChanged -= OnHidingStateChanged;
        }

        protected override void DespawnServerOnly() =>
            AllMushrooms.Remove(item: this);

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void ChangeState<TState>() where TState : IState =>
            _mushroomStateMachine.ChangeState<TState>();

        // RPC: -----------------------------------------------------------------------------------

        [Rpc(target: SendTo.Owner)]
        private void ChangeHatStateRpc(bool isHatDamaged) =>
            _isHatDamaged.Value = isHatDamaged;

        [Rpc(target: SendTo.Owner)]
        private void ToggleHidingStateRpc(bool isHiding) =>
            _isHiding.Value = isHiding;

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnHatStateChanged(bool previousValue, bool newValue) =>
            _animationController.ChangeHatState(newValue);

        private void OnHidingStateChanged(bool previousValue, bool newValue)
        {
            if (newValue)
                EnterHidingState();
            else
                EnterIdleState(); // TEMP
        }

        // DEBUG BUTTONS: -------------------------------------------------------------------------

        [Title(Constants.DebugButtons)]
        [Button(buttonSize: 30), DisableInEditorMode]
        private void DebugDamageHat() => DamageHat();
        
        [Button(buttonSize: 30), DisableInEditorMode]
        private void DebugRegenerateHat() => RegenerateHat();

        [Button(buttonSize: 30, ButtonStyle.FoldoutButton), DisableInEditorMode]
        private void DebugToggleHidingState(bool isHiding) => ToggleHidingState(isHiding);
    }
}