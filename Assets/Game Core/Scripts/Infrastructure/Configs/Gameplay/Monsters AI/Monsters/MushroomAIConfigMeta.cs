using System;
using DG.Tweening;
using GameCore.Enums.Gameplay;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using Sonity;
using UnityEngine;

namespace GameCore.Infrastructure.Configs.Gameplay.Enemies
{
    public class MushroomAIConfigMeta : MonsterAIConfigMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [TitleGroup(title: CommonSettings)]
        [BoxGroup(CommonGroup, showLabel: false), SerializeField, LabelText(ConfigTitle)]
        private CommonConfig _commonConfig;
        
        [TitleGroup(title: SporesSettings)]
        [BoxGroup(SporesGroup, showLabel: false), SerializeField, LabelText(ConfigTitle)]
        private SporesConfig _sporesConfig;
        
        [TitleGroup(title: WanderingSettings)]
        [BoxGroup(WanderingGroup, showLabel: false), SerializeField, LabelText(ConfigTitle)]
        private WanderingConfig _wanderingConfig;
        
        [TitleGroup(title: SuspicionSystemSettings)]
        [BoxGroup(SuspicionSystemGroup, showLabel: false), SerializeField, LabelText(ConfigTitle)]
        private SuspicionSystemConfig _suspicionSystemConfig;
        
        [TitleGroup(title: MoveToInterestTargetSettings)]
        [BoxGroup(MoveToInterestTargetGroup, showLabel: false), SerializeField, LabelText(ConfigTitle)]
        private MoveToInterestTargetConfig _moveToInterestTargetConfig;
        
        [TitleGroup(title: AnimationSettings)]
        [BoxGroup(AnimationGroup, showLabel: false), SerializeField, LabelText(ConfigTitle)]
        private AnimationConfig _animationConfig;
        
        [TitleGroup(SFXTitle)]
        [BoxGroup(SFXGroup, showLabel: false), SerializeField, Required]
        private SoundEvent _footstepsSE;
        
        [BoxGroup(SFXGroup), SerializeField, Required]
        private SoundEvent _hatExplosionSE;
        
        [BoxGroup(SFXGroup), SerializeField, Required]
        private SoundEvent _hatRegenerationSE;
        
        [BoxGroup(SFXGroup), SerializeField, Required]
        private SoundEvent _whisperingSE;

        [BoxGroup(SFXGroup), SerializeField, Required]
        private SoundEvent _sitDownSE;
        
        [BoxGroup(SFXGroup), SerializeField, Required]
        private SoundEvent _standUpSE;

        // PROPERTIES: ----------------------------------------------------------------------------

        public SoundEvent FootstepsSE => _footstepsSE;
        public SoundEvent HatExplosionSE => _hatExplosionSE;
        public SoundEvent HatRegenerationSE => _hatRegenerationSE;
        public SoundEvent WhisperingSE => _whisperingSE;
        public SoundEvent SitDownSE => _sitDownSE;
        public SoundEvent StandUpSE => _standUpSE;
        
        // FIELDS: --------------------------------------------------------------------------------

        private const string SporesSettings = "Spores Settings";
        private const string SuspicionSystemSettings = "Suspicion System Config";
        private const string MoveToInterestTargetSettings = "Move to Interest Target Config";
        
        private const string SporesGroup = SporesSettings + "/Group";
        private const string SuspicionSystemGroup = SuspicionSystemSettings + "/Group";
        private const string MoveToInterestTargetGroup = MoveToInterestTargetSettings + "/Group";

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void OnEnable()
        {
            base.OnEnable();
            _commonConfig.UpdateSporesGlowTimeText();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public CommonConfig GetCommonConfig() => _commonConfig;
        public SporesConfig GetSporesConfig() => _sporesConfig;
        public WanderingConfig GetWanderingConfig() => _wanderingConfig;
        public SuspicionSystemConfig GetSuspicionSystemConfig() => _suspicionSystemConfig;
        public MoveToInterestTargetConfig GetMoveToInterestTargetConfig() => _moveToInterestTargetConfig;
        public AnimationConfig GetAnimationConfig() => _animationConfig;
        
        public override MonsterType GetMonsterType() =>
            MonsterType.Mushroom;

        // INNER CLASSES: -------------------------------------------------------------------------

        #region Inner Classes

        [Serializable]
        public class CommonConfig
        {
            // MEMBERS: -------------------------------------------------------------------------------

            [SerializeField, Min(0f), SuffixLabel("seconds", overlay: true)]
            private float _hatRegenerationDelay = 10f;

            [SerializeField, Min(0f)]
            private float _runawaySpeed = 3f;

            [SerializeField, Min(0f)]
            private float _sitDownSoundDelay;
            
            [SerializeField, MinMaxSlider(minValue: 0f, maxValue: 12f, showFields: true)]
            private Vector2 _whisperingInterval;
            
            [SerializeField]
            private TimePeriod _sporesGlowTimePeriod;

            // PROPERTIES: ----------------------------------------------------------------------------

            public float HatRegenerationDelay => _hatRegenerationDelay;
            public float RunawaySpeed => _runawaySpeed;
            public float SitDownSoundDelay => _sitDownSoundDelay;
            public Vector2 WhisperingInterval => _whisperingInterval;
            public TimePeriod SporesGlowTimePeriod => _sporesGlowTimePeriod;

            // PUBLIC METHODS: ------------------------------------------------------------------------

            public void UpdateSporesGlowTimeText() =>
                _sporesGlowTimePeriod.UpdateTimeText();
        }

        [Serializable]
        public class SporesConfig
        {
            // MEMBERS: -------------------------------------------------------------------------------

            [SerializeField, Min(0f), SuffixLabel("seconds", overlay: true)]
            [Tooltip("Длительность спор, второй раз дляться в 2 раза меньше.")]
            private float _sporesDuration = 10f;

            [SerializeField]
            [Tooltip("Отступ по Y при создании облака спор.")]
            private float _sporesOffsetY;

            // PROPERTIES: ----------------------------------------------------------------------------

            public float SporesDuration => _sporesDuration;
            public float SporesOffsetY => _sporesOffsetY;
        }
        
        [Serializable]
        public class WanderingConfig
        {
            // MEMBERS: -------------------------------------------------------------------------------
            
            [SerializeField, Min(0f)]
            private float _minSpeed = 1f;
        
            [SerializeField, Min(0f)]
            private float _maxSpeed = 2.5f;
        
            [SerializeField, Min(0f)]
            private float _minDistance = 1f;
        
            [SerializeField, Min(0f)]
            private float _maxDistance = 15f;
        
            [SerializeField, Min(0f), SuffixLabel("seconds", overlay: true)]
            private float _minDelay = 0.5f;
        
            [SerializeField, Min(0f), SuffixLabel("seconds", overlay: true)]
            private float _maxDelay = 5f;

            [SerializeField, Min(0f)]
            private float _acceleration = 8f;

            // PROPERTIES: ----------------------------------------------------------------------------
            
            public float MinSpeed => _minSpeed;
            public float MaxSpeed => _maxSpeed;
            public float MinDistance => _minDistance;
            public float MaxDistance => _maxDistance;
            public float MinDelay => _minDelay;
            public float MaxDelay => _maxDelay;
            public float Acceleration => _acceleration;
        }

        [Serializable]
        public class SuspicionSystemConfig
        {
            // MEMBERS: -------------------------------------------------------------------------------

            [SerializeField, Min(0f), SuffixLabel("seconds", overlay: true)]
            private float _checkNearbyPlayersInterval = 0.2f;

            [SerializeField, Min(0f), SuffixLabel("meters", overlay: true)]
            private float _visionRange = 10f;

            [SerializeField, Min(0f)]
            private float _minLoudnessToReact;

            [SerializeField, Min(0f), SuffixLabel("seconds", overlay: true)]
            [Tooltip("Проявлять интерес если игрок не двигался указанное время.")]
            private float _interestAfterPlayerAfk = 3f;

            [SerializeField, Min(0f), SuffixLabel("seconds", overlay: true)]
            [Tooltip("Кол-во времени для побега от игрока после испуга.")]
            private float _retreatingTime = 3f;

            [SerializeField, Min(0f)]
            private float _distanceToHide = 5f;

            // PROPERTIES: ----------------------------------------------------------------------------

            public float CheckNearbyPlayersInterval => _checkNearbyPlayersInterval;
            public float VisionRange => _visionRange;
            public float MinLoudnessToReact => _minLoudnessToReact;
            public float InterestAfterPlayerAfk => _interestAfterPlayerAfk;
            public float RetreatingTime => _retreatingTime;
            public float DistanceToHide => _distanceToHide;
        }

        [Serializable]
        public class MoveToInterestTargetConfig
        {
            // MEMBERS: -------------------------------------------------------------------------------

            [SerializeField, MinMaxSlider(minValue: 0f, maxValue: 10f, showFields: true)]
            private Vector2 _moveSpeed;

            [SerializeField, Min(0f), SuffixLabel("seconds", overlay: true)]
            private float _positionCheckInterval = 0.2f;

            [SerializeField, Min(0f), SuffixLabel("seconds", overlay: true)]
            private float _distanceCheckInterval = 0.2f;

            [SerializeField, Min(0f), SuffixLabel("meters", overlay: true)]
            private float _targetReachDistance = 2f;

            // PROPERTIES: ----------------------------------------------------------------------------
            
            public Vector2 MoveSpeed => _moveSpeed;
            public float PositionCheckInterval => _positionCheckInterval;
            public float DistanceCheckInterval => _distanceCheckInterval;
            public float TargetReachDistance => _targetReachDistance;
        }

        [Serializable]
        public class AnimationConfig
        {
            // MEMBERS: -------------------------------------------------------------------------------

            [BoxGroup(CommonSettings), SerializeField, Range(0f, 1f)]
            private float _dampTime = 0.15f;
            
            [BoxGroup(CommonSettings), SerializeField, Min(0f), SuffixLabel("seconds", overlay: true)]
            private float _sigmaFaceResetDelay = 20f;

            
            [BoxGroup(HidingGroup),SerializeField]
            private float _modelSittingY = -0.36f;

            [BoxGroup(HidingGroup), SerializeField, Min(0f), SuffixLabel("seconds", overlay: true)]
            private float _modelSitDownDuration = 0.35f;
            
            [BoxGroup(HidingGroup), SerializeField, Min(0f), SuffixLabel("seconds", overlay: true)]
            private float _modelStandUpDuration = 0.35f;

            [BoxGroup(HidingGroup), SerializeField, Min(0f), SuffixLabel("seconds", overlay: true)]
            private float _modelSitDownDelay = 0.2f;
            
            [BoxGroup(HidingGroup), SerializeField, Min(0f), SuffixLabel("seconds", overlay: true)]
            private float _modelStandUpDelay;
            
            [BoxGroup(HidingGroup), SerializeField, Min(0f)]
            private float _sitDownAnimationMultiplier = 1f;
            
            [BoxGroup(HidingGroup), SerializeField, Min(0f)]
            private float _standUpAnimationMultiplier = 1f;

            [BoxGroup(HidingGroup), SerializeField]
            private Ease _modelSitDownEase = Ease.InOutQuad;
            
            [BoxGroup(HidingGroup), SerializeField]
            private Ease _modelStandUpEase = Ease.InOutQuad;
            
            
            [BoxGroup(ExplosionGroup), SerializeField, Min(0f), SuffixLabel("seconds", overlay: true)]
            private float _hatExplosionDuration = 0.35f;

            [BoxGroup(ExplosionGroup), SerializeField, Min(0f), SuffixLabel("seconds", overlay: true)]
            private float _hatRegenerationDuration = 1f;

            [BoxGroup(ExplosionGroup), SerializeField]
            private Ease _hatExplosionEase = Ease.InOutQuad;
            
            [BoxGroup(ExplosionGroup), SerializeField]
            private Ease _hatRegenerationEase = Ease.InOutQuad;

            // PROPERTIES: ----------------------------------------------------------------------------

            public float DampTime => _dampTime;
            public float SigmaFaceResetDelay => _sigmaFaceResetDelay;
            
            public float ModelSittingY => _modelSittingY;
            public float ModelSitDownDuration => _modelSitDownDuration;
            public float ModelStandUpDuration => _modelStandUpDuration;
            public float ModelSitDownDelay => _modelSitDownDelay;
            public float ModelStandUpDelay => _modelStandUpDelay;
            public float SitDownAnimationMultiplier => _sitDownAnimationMultiplier;
            public float StandUpAnimationMultiplier => _standUpAnimationMultiplier;
            public Ease ModelSitDownEase => _modelSitDownEase;
            public Ease ModelStandUpEase => _modelStandUpEase;
            
            public float HatExplosionDuration => _hatExplosionDuration;
            public float HatRegenerationDuration => _hatRegenerationDuration;
            public Ease HatExplosionEase => _hatExplosionEase;
            public Ease HatRegenerationEase => _hatRegenerationEase;

            // FIELDS: --------------------------------------------------------------------------------

            private const string HidingGroup = "Hiding";
            private const string ExplosionGroup = "Hat Explosion";
        }
        
        #endregion
    }
}