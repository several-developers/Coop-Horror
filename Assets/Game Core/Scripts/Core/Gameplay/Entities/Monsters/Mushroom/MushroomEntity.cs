using System.Collections.Generic;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Monsters.Mushroom.States;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Systems.Noise;
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
    public class MushroomEntity : SoundProducerNavmeshMonsterEntity<MushroomEntity.SFXType>, INoiseListener
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
        
        [BoxGroup(Constants.References, showLabel: false), SerializeField]
        private MushroomReferences _references;

        // PROPERTIES: ----------------------------------------------------------------------------

        public bool IsHatDamaged => _isHatDamaged.Value;
        public bool IsSneaking { get; private set; }

        // FIELDS: --------------------------------------------------------------------------------

        private static readonly List<MushroomEntity> AllMushrooms = new();

        private readonly NetworkVariable<bool> _isHatDamaged = new(writePerm: Constants.OwnerPermission);
        private readonly NetworkVariable<bool> _isHiding = new(writePerm: Constants.OwnerPermission);

        private MushroomAIConfigMeta _mushroomAIConfig;

        private StateMachineBase _mushroomStateMachine;
        private AnimationController _animationController;
        private SuspicionSystem _suspicionSystem;
        private HatHealingTimer _hatHealingTimer;

        private PlayerEntity _interestTarget;
        private PlayerEntity _playerAbuser;
        private bool _isHatNew = true;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void StartServerOnly() => EnterIdleState();

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void DetectNoise(Vector3 noisePosition, float noiseLoudness) =>
            _suspicionSystem.DetectNoise(noisePosition, noiseLoudness);

        public void DamageHat() => SetHatStateRpc(isHatDamaged: true);

        public void RegenerateHat() => SetHatStateRpc(isHatDamaged: false);

        public void ToggleHidingState(bool isHiding) => ToggleHidingStateRpc(isHiding);

        public void SetEmotion(Emotion emotion) => SetEmotionRpc(emotion);

        public void SetInterestTarget(PlayerEntity interestTarget) =>
            _interestTarget = interestTarget;

        public void SetSneakingState(bool isSneaking) =>
            IsSneaking = isSneaking;
        
        [Button]
        public void EnterIdleState() => ChangeState<IdleState>();
        
        [Button]
        public void EnterWanderingState() => ChangeState<WanderingState>();
        
        [Button]
        public void EnterHidingState() => ChangeState<HidingState>();
        
        [Button]
        public void EnterRunawayState() => ChangeState<RunawayState>();
        
        [Button]
        public void EnterMoveToInterestTargetState() => ChangeState<MoveToInterestTargetState>();
        
        [Button]
        public void EnterLookAtInterestTargetState() => ChangeState<LookAtInterestTargetState>();
        
        public MushroomAIConfigMeta GetAIConfig() => _mushroomAIConfig;

        public MushroomReferences GetReferences() => _references;

        public AnimationController GetAnimationController() => _animationController;

        public SuspicionSystem GetSuspicionSystem() => _suspicionSystem;

        public PlayerEntity GetInterestTarget() => _interestTarget;

        public bool TryGetCurrentState(out IState state) =>
            _mushroomStateMachine.TryGetCurrentState(out state);

        public override MonsterType GetMonsterType() =>
            MonsterType.Mushroom;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void InitAll()
        {
            _animationController = new AnimationController(mushroomEntity: this);

            _isHatDamaged.OnValueChanged += OnHatDamageStateChanged;
            _isHiding.OnValueChanged += OnHidingStateChanged;
        }

        protected override void InitServerOnly()
        {
            AllMushrooms.Add(item: this);
            
            InitSystems();
            SetupStates();

            _references.PlayerTrigger.OnPlayerEnterEvent += OnPlayerSteppedOnHat;

            _hatHealingTimer.OnTimerEndedEvent += OnHatHealed;

            // LOCAL METHODS: -----------------------------

            void InitSystems()
            {
                _mushroomStateMachine = new StateMachineBase();
                _suspicionSystem = new SuspicionSystem(mushroomEntity: this);
                _hatHealingTimer = new HatHealingTimer(mushroomEntity: this);
                
                _suspicionSystem.Start();

                _mushroomStateMachine.OnStateChangedEvent += state =>
                {
                    string log = Log.HandleLog($"New state '<gb>{state.GetType().Name}</gb>'");
                    Debug.Log(log);
                };
            }

            void SetupStates()
            {
                IdleState idleState = new(mushroomEntity: this);
                WanderingState wanderingState = new(mushroomEntity: this);
                HidingState hidingState = new(mushroomEntity: this);
                RunawayState runawayState = new(mushroomEntity: this);
                MoveToInterestTargetState moveToInterestTargetState = new(mushroomEntity: this);
                LookAtInterestTargetState lookAtInterestTargetState = new(mushroomEntity: this);
                
                _mushroomStateMachine.AddState(idleState);
                _mushroomStateMachine.AddState(wanderingState);
                _mushroomStateMachine.AddState(hidingState);
                _mushroomStateMachine.AddState(runawayState);
                _mushroomStateMachine.AddState(moveToInterestTargetState);
                _mushroomStateMachine.AddState(lookAtInterestTargetState);
            }
        }

        protected override void TickServerOnly()
        {
            _suspicionSystem.Tick();
            _animationController.Tick();
            _mushroomStateMachine.Tick();
        }

        protected override void DespawnAll()
        {
            _isHatDamaged.OnValueChanged -= OnHatDamageStateChanged;
            _isHiding.OnValueChanged -= OnHidingStateChanged;
            
            _references.PlayerTrigger.OnPlayerEnterEvent -= OnPlayerSteppedOnHat;
        }

        protected override void DespawnServerOnly()
        {
            AllMushrooms.Remove(item: this);
            _hatHealingTimer.StopTimer();
            
            if (_playerAbuser != null)
                _playerAbuser.OnDeathEvent -= OnAbuserDeath;
            
            _hatHealingTimer.OnTimerEndedEvent -= OnHatHealed;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void ChangeState<TState>() where TState : IState =>
            _mushroomStateMachine.ChangeState<TState>();

        // RPC: -----------------------------------------------------------------------------------

        [Rpc(target: SendTo.Owner)]
        private void SetHatStateRpc(bool isHatDamaged) =>
            _isHatDamaged.Value = isHatDamaged;

        [Rpc(target: SendTo.Owner)]
        private void ToggleHidingStateRpc(bool isHiding) =>
            _isHiding.Value = isHiding;

        [Rpc(target: SendTo.Everyone)]
        private void SetEmotionRpc(Emotion emotion) =>
            _animationController.SetEmotion(emotion);

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnHatDamageStateChanged(bool previousValue, bool newValue)
        {
            _animationController.SetHatState(newValue);

            Emotion emotion = newValue ? Emotion.Angry : Emotion.Happy;
            SetEmotion(emotion);
        }

        private void OnHidingStateChanged(bool previousValue, bool newValue)
        {
            if (newValue)
                EnterHidingState();
            else
                EnterIdleState(); // TEMP
        }

        private void OnPlayerSteppedOnHat(PlayerEntity playerEntity)
        {
            if (_isHatDamaged.Value)
                return;
            
            DamageHat();
            
            if (!_isHatNew)
            {
                // Death
                return;
            }

            _hatHealingTimer.StartTimer();
            
            _isHatNew = false;
            _playerAbuser = playerEntity;
            
            _playerAbuser.OnDeathEvent += OnAbuserDeath;
        }

        private void OnAbuserDeath()
        {
            Debug.Log("------------ ABUSER DIED MUHAHAHAHAH --------------");
            SetEmotion(Emotion.Sigma);
        }

        private void OnHatHealed() => RegenerateHat();

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