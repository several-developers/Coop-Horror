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

        [TitleGroup(title: DebugSettings)]
        [BoxGroup(DebugGroup, showLabel: false), SerializeField]
        [InfoBox(message: DisableAttackWarning, InfoMessageType.Error, nameof(_disableAttack))]
        private bool _disableAttack;
        
        [TitleGroup(title: SuspicionSystemSettings)]
        [BoxGroup(SuspicionSystemGroup, showLabel: false), SerializeField, LabelText(ConfigTitle)]
        private SuspicionSystemConfig _suspicionSystemConfig;
        
        [TitleGroup(title: SuspicionStateSettings)]
        [BoxGroup(SuspicionStateGroup, showLabel: false), SerializeField, LabelText(ConfigTitle)]
        private SuspicionStateConfig _suspicionStateConfig;

        [TitleGroup(title: WanderingSettings)]
        [BoxGroup(WanderingGroup, showLabel: false), SerializeField, LabelText(ConfigTitle)]
        private WanderingConfig _wanderingConfig;
        
        [TitleGroup(title: LookAroundSettings)]
        [BoxGroup(LookAroundGroup, showLabel: false), SerializeField, LabelText(ConfigTitle)]
        private WanderingConfig _lookAroundConfig;

        [TitleGroup(title: CombatSettings)]
        [BoxGroup(CombatGroup, showLabel: false), SerializeField, LabelText(ConfigTitle)]
        private CombatConfig _combatConfig;
        
        [TitleGroup(title: CageBirdSettings)]
        [BoxGroup(CageBirdGroup, showLabel: false), SerializeField, LabelText(ConfigTitle)]
        private CageBirdConfig _cageBirdConfig;

        [TitleGroup(title: AnimationSettings)]
        [BoxGroup(AnimationGroup, showLabel: false), SerializeField, LabelText(ConfigTitle)]
        private AnimationConfig _animationConfig;

        [TitleGroup(SFXTitle)]
        [BoxGroup(SFXGroup, showLabel: false), SerializeField, Required]
        private SoundEvent _whisperingSE;
        
        [BoxGroup(SFXGroup), SerializeField, Required]
        private SoundEvent _whispersSE;
        
        [BoxGroup(SFXGroup), SerializeField, Required]
        private SoundEvent _swingSE;
        
        [BoxGroup(SFXGroup), SerializeField, Required]
        private SoundEvent _slashSE;
        
        [BoxGroup(SFXGroup), SerializeField, Required]
        private SoundEvent _birdChirpSE;
        
        [BoxGroup(SFXGroup), SerializeField, Required]
        private SoundEvent _birdScreamSE;

        // PROPERTIES: ----------------------------------------------------------------------------

        public bool DisableAttack => _disableAttack;
        
        public SoundEvent WhisperingSE => _whisperingSE;
        public SoundEvent WhispersSE => _whispersSE;
        public SoundEvent SwingSE => _swingSE;
        public SoundEvent SlashSE => _slashSE;
        public SoundEvent BirdChirpSE => _birdChirpSE;
        public SoundEvent BirdScreamSE => _birdScreamSE;

        // FIELDS: --------------------------------------------------------------------------------

        private const string LookAroundSettings = "Look Around Config";
        private const string SuspicionSystemSettings = "Suspicion System Config";
        private const string SuspicionStateSettings = "Suspicion State Config";
        private const string CombatSettings = "Combat Config";
        private const string CageBirdSettings = "Cage Bird Config";
        
        private const string LookAroundGroup = LookAroundSettings + "/Group";
        private const string SuspicionSystemGroup = SuspicionSystemSettings + "/Group";
        private const string SuspicionStateGroup = SuspicionStateSettings + "/Group";
        private const string CombatGroup = CombatSettings + "/Group";
        private const string CageBirdGroup = CageBirdSettings + "/Group";

        private const string DisableAttackWarning = "Warning! This must be disabled for the release.";

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public SuspicionSystemConfig GetSuspicionSystemConfig() => _suspicionSystemConfig;
        public SuspicionStateConfig GetSuspicionStateConfig() => _suspicionStateConfig;
        public WanderingConfig GetWanderingConfig() => _wanderingConfig;
        public WanderingConfig GetLookAroundConfig() => _lookAroundConfig;
        public CombatConfig GetCombatConfig() => _combatConfig;
        public CageBirdConfig GetCageBirdConfig() => _cageBirdConfig;
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

            [SerializeField, Min(0f), SuffixLabel(Seconds, overlay: true)]
            private float _suspicionMeterDecreaseTime = 3f;

            // PROPERTIES: ----------------------------------------------------------------------------
            
            public float MinLoudnessToReact => _minLoudnessToReact;
            public int SuspicionMeterToAggro => _suspicionMeterToAggro;
            public int SuspicionMeterMaxAmount => _suspicionMeterMaxAmount;
            public int SuspicionMeterAfterInstantAggro => _suspicionMeterAfterInstantAggro;
            public float SuspicionMeterDecreaseTime => _suspicionMeterDecreaseTime;
        }

        [Serializable]
        public class SuspicionStateConfig
        {
            // MEMBERS: -------------------------------------------------------------------------------
            
            [SerializeField, Min(0f)]
            private float _suspicionMoveSpeed = 5f;
            
            [SerializeField, Min(0f)]
            private float _suspicionAcceleration = 20f;

            [SerializeField, MinMaxSlider(minValue: 0f, maxValue: 10f, showFields: true)]
            private Vector2 _whispersDelay;

            // PROPERTIES: ----------------------------------------------------------------------------

            public float SuspicionMoveSpeed => _suspicionMoveSpeed;
            public float SuspicionAcceleration => _suspicionAcceleration;
            public Vector2 WhispersDelay => _whispersDelay;
        }

        [Serializable]
        public class CombatConfig
        {
            // MEMBERS: -------------------------------------------------------------------------------

            [SerializeField, Min(0)]
            private float _damage = 50f;
            
            [SerializeField, Min(0f), SuffixLabel("seconds", overlay: true)]
            private float _attackCooldown = 2f;

            [SerializeField, Min(0f)]
            private float _attackDistance = 1.5f;
            
            [SerializeField, Min(0f)]
            private float _triggerRadius = 1f;

            [SerializeField]
            private LayerMask _layerMask;

            // PROPERTIES: ----------------------------------------------------------------------------

            public float Damage => _damage;
            public float AttackCooldown => _attackCooldown;
            public float AttackDistance => _attackDistance;
            public float TriggerRadius => _triggerRadius;
            public LayerMask LayerMask => _layerMask;
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
        public class CageBirdConfig
        {
            // MEMBERS: -------------------------------------------------------------------------------

            [SerializeField, MinMaxSlider(minValue: 0f, maxValue: 15f, showFields: true)]
            private Vector2 _chirpDelay;

            // PROPERTIES: ----------------------------------------------------------------------------

            public Vector2 ChirpDelay => _chirpDelay;
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