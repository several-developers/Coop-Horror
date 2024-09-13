using System.Collections.Generic;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Monsters.Mushroom.States;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Systems.Noise;
using GameCore.Gameplay.Systems.SoundReproducer;
using GameCore.Infrastructure.Providers.Gameplay.MonstersAI;
using GameCore.Infrastructure.StateMachine;
using GameCore.Observers.Gameplay.Time;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using Zenject;
using Random = System.Random;

namespace GameCore.Gameplay.Entities.Monsters.Mushroom
{
    [GenerateSerializationForType(typeof(SFXType))]
    public class MushroomEntity : SoundProducerNavmeshMonsterEntity<MushroomEntity.SFXType>, INoiseListener
    {
        public enum SFXType
        {
            // None = 0,
            Footsteps = 1,
            HatExplosion = 2,
            HatRegeneration = 3,
            Whispering = 4,
            SitDown = 5,
            StandUp = 6
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
        private void Construct(IMonstersAIConfigsProvider monstersAIConfigsProvider, ITimeObserver timeObserver)
        {
            _timeObserver = timeObserver;
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

        private ITimeObserver _timeObserver;
        private MushroomAIConfigMeta _mushroomAIConfig;

        private StateMachineBase _mushroomStateMachine;
        private AnimationController _animationController;
        private SuspicionSystem _suspicionSystem;
        private HatHealingTimer _hatHealingTimer;
        private WhisperingSystem _whisperingSystem;

        private PlayerEntity _interestTarget;
        private PlayerEntity _playerAbuser;
        private bool _isHatNew = true;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void StartServerOnly() => EnterIdleState();

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void DetectNoise(Vector3 noisePosition, float noiseLoudness) =>
            _suspicionSystem.DetectNoise(noisePosition, noiseLoudness);

        public void SetEmotion(Emotion emotion) => SetEmotionRpc(emotion);

        public void SetInterestTarget(PlayerEntity interestTarget) =>
            _interestTarget = interestTarget;

        public void SetSneakingState(bool isSneaking) =>
            IsSneaking = isSneaking;

        public void EnterIdleState() => ChangeState<IdleState>();

        public void EnterWanderingState() => ChangeState<WanderingState>();

        public void EnterHidingState() => ChangeState<HidingState>();

        public void EnterRunawayState() => ChangeState<RunawayState>();

        public void EnterMoveToInterestTargetState() => ChangeState<MoveToInterestTargetState>();

        public void EnterLookAtInterestTargetState() => ChangeState<LookAtInterestTargetState>();

        public static IReadOnlyList<MushroomEntity> GetAllMushrooms() => AllMushrooms;

        public MushroomAIConfigMeta GetAIConfig() => _mushroomAIConfig;

        public MushroomReferences GetReferences() => _references;

        public AnimationController GetAnimationController() => _animationController;

        public SuspicionSystem GetSuspicionSystem() => _suspicionSystem;

        public WhisperingSystem GetWhisperingSystem() => _whisperingSystem;

        public PlayerEntity GetInterestTarget() => _interestTarget;

        public bool TryGetCurrentState(out IState state) =>
            _mushroomStateMachine.TryGetCurrentState(out state);

        public override MonsterType GetMonsterType() =>
            MonsterType.Mushroom;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void InitAll()
        {
            SoundReproducer = new MushroomSoundReproducer(this, _mushroomAIConfig);
            _animationController = new AnimationController(mushroomEntity: this);

            _isHatDamaged.OnValueChanged += OnHatDamageStateChanged;
            _isHiding.OnValueChanged += OnHidingStateChanged;

            _timeObserver.OnMinutePassedEvent += OnMinutePassed;
        }

        protected override void InitServerOnly()
        {
            AllMushrooms.Add(item: this);

            InitSystems();
            SetupStates();

            _references.PlayerTrigger.OnPlayerEnterEvent += OnPlayerSteppedOnHat;

            _references.FootstepsSystem.OnFootstepPerformedEvent += OnFootstepPerformed;

            _hatHealingTimer.OnTimerEndedEvent += OnHatHealed;

            // LOCAL METHODS: -----------------------------

            void InitSystems()
            {
                _mushroomStateMachine = new StateMachineBase();
                _suspicionSystem = new SuspicionSystem(mushroomEntity: this);
                _hatHealingTimer = new HatHealingTimer(mushroomEntity: this);
                _whisperingSystem = new WhisperingSystem(mushroomEntity: this);

                _suspicionSystem.Start();
                _whisperingSystem.Start();
                _whisperingSystem.Pause();
                
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
                DeathState deathState = new(mushroomEntity: this);
                MoveToInterestTargetState moveToInterestTargetState = new(mushroomEntity: this);
                LookAtInterestTargetState lookAtInterestTargetState = new(mushroomEntity: this);

                _mushroomStateMachine.AddState(idleState);
                _mushroomStateMachine.AddState(wanderingState);
                _mushroomStateMachine.AddState(hidingState);
                _mushroomStateMachine.AddState(runawayState);
                _mushroomStateMachine.AddState(deathState);
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

            _timeObserver.OnMinutePassedEvent -= OnMinutePassed;
        }

        protected override void DespawnServerOnly()
        {
            AllMushrooms.Remove(item: this);
            _hatHealingTimer.StopTimer();

            if (_playerAbuser != null)
                _playerAbuser.OnDeathEvent -= OnAbuserDeath;
                
            _references.PlayerTrigger.OnPlayerEnterEvent -= OnPlayerSteppedOnHat;
                
            _references.FootstepsSystem.OnFootstepPerformedEvent -= OnFootstepPerformed;
            
            _hatHealingTimer.OnTimerEndedEvent -= OnHatHealed;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void DamageHat()
        {
            CreateSporesTrigger();
            SetHatStateRpc(isHatDamaged: true);
        }

        private void RegenerateHat() => SetHatStateRpc(isHatDamaged: false);

        private void ToggleHidingState(bool isHiding) => ToggleHidingStateRpc(isHiding);

        private void CreateSporesTrigger()
        {
            var random = new Random();
            uint randomSeed = (uint)random.Next(0, int.MaxValue);

            CreateSporesTriggerRpc(randomSeed, _isHatNew);
        }

        private void HandleHatDamageState(bool isDamaged)
        {
            _animationController.SetHatState(isDamaged);

            Emotion emotion = isDamaged ? Emotion.Angry : Emotion.Happy;
            SetEmotion(emotion);

            ParticleSystem sporesPS = _references.SporesPS;

            if (isDamaged)
            {
                PlaySound(SFXType.HatExplosion).Forget();
                sporesPS.Stop();
            }
            else
            {
                PlaySound(SFXType.HatRegeneration).Forget();
                sporesPS.Play();
            }
        }

        private void HandleHidingState(bool isHiding)
        {
            if (isHiding)
                EnterHidingState();
            else
                EnterIdleState(); // TEMP
        }

        private void HandlePlayerSteppedOnHat(PlayerEntity playerEntity)
        {
            if (_isHatDamaged.Value)
                return;

            DamageHat();

            if (!_isHatNew)
            {
                EnterDeathState();
                return;
            }

            _hatHealingTimer.StartTimer();

            _isHatNew = false;
            _playerAbuser = playerEntity;

            _playerAbuser.OnDeathEvent += OnAbuserDeath;
        }

        private void CheckSporesLighting()
        {
            if (EntityLocation == EntityLocation.Dungeon)
                return;

            MushroomAIConfigMeta.CommonConfig commonConfig = _mushroomAIConfig.GetCommonConfig();
            TimePeriod sporesGlowingTime = commonConfig.SporesGlowTimePeriod;
            int currentTimeInMinutes = _timeObserver.GetCurrentTimeInMinutes();
            bool isSporesGlowingTimeValid = sporesGlowingTime.IsTimeValid(currentTimeInMinutes);

            ParticleSystem sporesPS = _references.SporesPS;
            ParticleSystem.LightsModule lightsModule = sporesPS.lights;
            lightsModule.enabled = isSporesGlowingTimeValid;
        }

        private void EnterDeathState() => ChangeState<DeathState>();

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

        [Rpc(target: SendTo.Everyone)]
        private void CreateSporesTriggerRpc(uint seed, bool isHatNew)
        {
            MushroomAIConfigMeta.SporesConfig sporesConfig = _mushroomAIConfig.GetSporesConfig();
            MushroomSporesTrigger sporesTriggerPrefab = _references.SporesTriggerPrefab;
            Vector3 spawnPosition = transform.position;
            spawnPosition.y += sporesConfig.SporesOffsetY;

            MushroomSporesTrigger mushroomSporesTrigger =
                Instantiate(sporesTriggerPrefab, spawnPosition, Quaternion.identity);

            mushroomSporesTrigger.Setup(sporesConfig, seed, isHatNew);
            mushroomSporesTrigger.Start();
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnHatDamageStateChanged(bool previousValue, bool newValue) => HandleHatDamageState(newValue);

        private void OnHidingStateChanged(bool previousValue, bool newValue) => HandleHidingState(newValue);

        private void OnPlayerSteppedOnHat(PlayerEntity playerEntity) => HandlePlayerSteppedOnHat(playerEntity);

        private void OnAbuserDeath()
        {
            Debug.Log("------------ ABUSER DIED MUHAHAHAHAH --------------");
            SetEmotion(Emotion.Sigma);
        }

        private void OnFootstepPerformed(string colliderTag) => PlaySound(SFXType.Footsteps).Forget();

        private void OnHatHealed() => RegenerateHat();

        private void OnMinutePassed() => CheckSporesLighting();

        // DEBUG BUTTONS: -------------------------------------------------------------------------

        [Title(Constants.DebugButtons)]
        [Button(buttonSize: 25), DisableInEditorMode]
        private void DebugDamageHat() => DamageHat();

        [Button(buttonSize: 25), DisableInEditorMode]
        private void DebugRegenerateHat() => RegenerateHat();

        [Button(buttonSize: 25, ButtonStyle.FoldoutButton), DisableInEditorMode]
        private void DebugToggleHidingState(bool isHiding) => ToggleHidingState(isHiding);

        [Button(buttonSize: 25), DisableInEditorMode, PropertySpace(spaceBefore: 10)]
        public void DebugEnterIdleState() => EnterIdleState();

        [Button(buttonSize: 25), DisableInEditorMode]
        public void DebugEnterWanderingState() => EnterWanderingState();

        [Button(buttonSize: 25), DisableInEditorMode]
        public void DebugEnterHidingState() => EnterHidingState();

        [Button(buttonSize: 25), DisableInEditorMode]
        public void DebugEnterRunawayState() => EnterRunawayState();

        [Button(buttonSize: 25), DisableInEditorMode]
        public void DebugEnterDeathState() => EnterDeathState();

        [Button(buttonSize: 25), DisableInEditorMode]
        public void DebugEnterMoveToInterestTargetState() => EnterMoveToInterestTargetState();

        [Button(buttonSize: 25), DisableInEditorMode]
        public void DebugEnterLookAtInterestTargetState() => EnterLookAtInterestTargetState();
    }
}