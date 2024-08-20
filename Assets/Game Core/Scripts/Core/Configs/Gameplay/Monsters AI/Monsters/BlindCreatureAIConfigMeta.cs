using System;
using DG.Tweening;
using GameCore.Enums.Gameplay;
using Sirenix.OdinInspector;
using Sonity;
using UnityEngine;

namespace GameCore.Configs.Gameplay.Enemies
{
    public class BlindCreatureAIConfigMeta : MonsterAIConfigMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [TitleGroup(title: SuspicionSystemSettings)]
        [BoxGroup(SuspicionSystemGroup, showLabel: false), SerializeField, LabelText(ConfigTitle)]
        private SuspicionSystemConfig _suspicionSystemConfig;

        [TitleGroup(title: WanderingSettings)]
        [BoxGroup(WanderingGroup, showLabel: false), SerializeField, LabelText(ConfigTitle)]
        private WanderingConfig _wanderingConfig;

        [TitleGroup(title: AnimationSettings)]
        [BoxGroup(AnimationGroup, showLabel: false), SerializeField, LabelText(ConfigTitle)]
        private AnimationConfig _animationConfig;

        [TitleGroup(title: SuspicionMovementStateSettings)]
        [BoxGroup(SuspicionMovementStateGroup, showLabel: false), SerializeField, Min(0f)]
        private float _suspicionMoveSpeed = 5f;

        [TitleGroup(title: AttackStateSettings)]
        [BoxGroup(AttackStateGroup, showLabel: false), SerializeField, Min(0f)]
        private float _attackDistance = 1f;

        [BoxGroup(AttackStateGroup), SerializeField, Min(0f)]
        private float _attackCooldown = 2f;
        
        [Title(SFXTitle)]
        [SerializeField, Required]
        private SoundEvent _birdTweetSE;
        
        [SerializeField, Required]
        private SoundEvent _birdScreamSE;

        // PROPERTIES: ----------------------------------------------------------------------------
        
        public float SuspicionMoveSpeed => _suspicionMoveSpeed;

        public float AttackDistance => _attackDistance;
        public float AttackCooldown => _attackCooldown;
        
        // SFX
        public SoundEvent BirdTweetSE => _birdTweetSE;
        public SoundEvent BirdScreamSE => _birdScreamSE;

        // FIELDS: --------------------------------------------------------------------------------

        private const string ConfigTitle = "Config";
        private const string CommonSettings = "Common Settings";
        private const string WanderingSettings = "Wandering Config";
        private const string SuspicionMovementStateSettings = "Suspicion Movement State Settings";
        private const string AttackStateSettings = "Attack State Settings";
        private const string SuspicionSystemSettings = "Suspicion System Config";
        private const string AnimationSettings = "Animation Config";
        
        private const string CommonGroup = CommonSettings + "/Group";
        private const string WanderingGroup = WanderingSettings + "/Group";
        private const string SuspicionMovementStateGroup = SuspicionMovementStateSettings + "/Group";
        private const string AttackStateGroup = AttackStateSettings + "/Group";
        private const string SuspicionSystemGroup = SuspicionSystemSettings + "/Group";
        private const string AnimationGroup = AnimationSettings + "/Group";
        
        private const string SFXTitle = "SFX";

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public SuspicionSystemConfig GetSuspicionSystemConfig() => _suspicionSystemConfig;
        public WanderingConfig GetWanderingConfig() => _wanderingConfig;
        public AnimationConfig GetAnimationConfig() => _animationConfig;
        
        public override MonsterType GetMonsterType() =>
            MonsterType.BlindCreature;

        // INNER CLASSES: -------------------------------------------------------------------------

        #region Inner Classes

        [Serializable]
        public class SuspicionSystemConfig
        {
            // MEMBERS: -------------------------------------------------------------------------------
            
            [SerializeField, Min(0f)]
            private float _minLoudnessToReact = 0.1f;
        
            [SerializeField, Min(0)]
            [Tooltip("Значение Шкалы Подозрения для перехода в агрессивное состояние.")]
            private int _suspicionMeterToAggro = 8;

            [SerializeField, Min(0)]
            private int _suspicionMeterMaxAmount = 15;

            [SerializeField, Min(0)]
            [Tooltip("Значение Шкалы Подозрения после моментального перехода в агрессивное состояние.")]
            private int _suspicionMeterAfterInstantAggro = 11;

            [SerializeField, Min(0f), SuffixLabel("seconds", overlay: true)]
            private float _suspicionMeterDecreaseTime = 3f;

            // PROPERTIES: ----------------------------------------------------------------------------
            
            public float MinLoudnessToReact => _minLoudnessToReact;
            public int SuspicionMeterToAggro => _suspicionMeterToAggro;
            public int SuspicionMeterMaxAmount => _suspicionMeterMaxAmount;
            public int SuspicionMeterAfterInstantAggro => _suspicionMeterAfterInstantAggro;
            public float SuspicionMeterDecreaseTime => _suspicionMeterDecreaseTime;
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
        
            [SerializeField, Min(0f)]
            private float _minDelay = 0.5f;
        
            [SerializeField, Min(0f)]
            private float _maxDelay = 5f;

            // PROPERTIES: ----------------------------------------------------------------------------
            
            public float MinSpeed => _minSpeed;
            public float MaxSpeed => _maxSpeed;
            public float MinDistance => _minDistance;
            public float MaxDistance => _maxDistance;
            public float MinDelay => _minDelay;
            public float MaxDelay => _maxDelay;
        }

        [Serializable]
        public class AnimationConfig
        {
            // MEMBERS: -------------------------------------------------------------------------------

            [SerializeField, Min(0f)]
            private float _dampTime = 0.15f;

            [SerializeField]
            private Vector2 _levitatingRange;

            [SerializeField]
            private Vector2 _levitatingDuration;

            [SerializeField]
            private Ease _levitatingEase = Ease.InOutQuad;

            // PROPERTIES: ----------------------------------------------------------------------------

            public float DampTime => _dampTime;
            public Vector2 LevitatingRange => _levitatingRange;
            public Vector2 LevitatingDuration => _levitatingDuration;
            public Ease LevitatingEase => _levitatingEase;
        }
        
        #endregion
    }
}