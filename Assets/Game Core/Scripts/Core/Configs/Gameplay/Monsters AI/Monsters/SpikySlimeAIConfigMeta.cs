using System;
using System.Collections.Generic;
using DG.Tweening;
using GameCore.Enums.Gameplay;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using Sonity;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameCore.Configs.Gameplay.Enemies
{
    public class SpikySlimeAIConfigMeta : MonsterAIConfigMeta
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public SpikySlimeAIConfigMeta() =>
            _sizeConfigs = new List<SizeConfig>();

        // MEMBERS: -------------------------------------------------------------------------------

        [TitleGroup(title: AggressionSystemSettings)]
        [BoxGroup(AggressionSystemGroup, showLabel: false), SerializeField, LabelText(ConfigTitle)]
        private AggressionSystemConfig _aggressionSystemConfig;

        [TitleGroup(title: AttackSystemSettings)]
        [BoxGroup(AttackSystemGroup, showLabel: false), SerializeField, LabelText(ConfigTitle)]
        private AttackSystemConfig _attackSystemConfig;

        [TitleGroup(title: WanderingSettings)]
        [BoxGroup(WanderingGroup, showLabel: false), SerializeField, LabelText(ConfigTitle)]
        private WanderingConfig _wanderingConfig;

        [TitleGroup(title: AnimationSettings)]
        [BoxGroup(AnimationGroup, showLabel: false), SerializeField, LabelText(ConfigTitle)]
        private AnimationConfig _animationConfig;

        [SerializeField, Space(height: 10)]
        [ListDrawerSettings(ListElementLabelName = "Label")]
        private List<SizeConfig> _sizeConfigs;

        [TitleGroup(SFXTitle)]
        [BoxGroup(SFXGroup, showLabel: false), SerializeField, Required]
        private SoundEvent _calmMovementSE;

        [BoxGroup(SFXGroup), SerializeField, Required]
        private SoundEvent _angryMovementSE;

        [BoxGroup(SFXGroup), SerializeField, Required]
        private SoundEvent _calmingSE;

        [BoxGroup(SFXGroup), SerializeField, Required]
        private SoundEvent _angrySE;

        [BoxGroup(SFXGroup), SerializeField, Required]
        private SoundEvent _attackSE;

        [BoxGroup(SFXGroup), SerializeField, Required]
        private SoundEvent _stabSE;

        // PROPERTIES: ----------------------------------------------------------------------------

        public SoundEvent CalmMovementSE => _calmMovementSE;
        public SoundEvent AngryMovementSE => _angryMovementSE;
        public SoundEvent CalmingSE => _calmingSE;
        public SoundEvent AngrySE => _angrySE;
        public SoundEvent AttackSE => _attackSE;
        public SoundEvent StabSE => _stabSE;

        // FIELDS: --------------------------------------------------------------------------------

        private const string AggressionSystemSettings = "Aggression System";
        private const string AttackSystemSettings = "Attack System";

        private const string AggressionSystemGroup = AggressionSystemSettings + "/Group";
        private const string AttackSystemGroup = AttackSystemSettings + "/Group";

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public AggressionSystemConfig GetAggressionSystemConfig() => _aggressionSystemConfig;
        public AttackSystemConfig GetAttackSystemConfig() => _attackSystemConfig;
        public WanderingConfig GetWanderingConfig() => _wanderingConfig;
        public AnimationConfig GetAnimationConfig() => _animationConfig;

        public override MonsterType GetMonsterType() =>
            MonsterType.SpikySlime;

        public SizeConfig GetRandomSizeConfig(EntityLocation entityLocation)
        {
            int sizeConfigsAmount = _sizeConfigs.Count;

            if (sizeConfigsAmount == 0)
            {
                Debug.LogError(message: "Size Config list is empty!");
                return null;
            }

            var chances = new double[sizeConfigsAmount];

            for (int i = 0; i < sizeConfigsAmount; i++)
            {
                SizeConfig sizeConfig = _sizeConfigs[i];
                bool isConfigValid = true;

                if (sizeConfig.OnlySurface)
                    isConfigValid = entityLocation == EntityLocation.Surface;

                if (!isConfigValid)
                    continue;
                    
                chances[i] = _sizeConfigs[i].Chance;
            }

            int randomIndex = GlobalUtilities.GetRandomIndex(chances);
            return _sizeConfigs[randomIndex];
        }

        // INNER CLASSES: -------------------------------------------------------------------------

        #region Inner Classes

        [Serializable]
        public class AggressionSystemConfig
        {
            // MEMBERS: -------------------------------------------------------------------------------

            [SerializeField, Min(0f)]
            private float _minLoudnessToReact = 0.1f;

            [SerializeField, Min(0)]
            [Tooltip("Значение Шкалы Агрессии для перехода в агрессивное состояние.")]
            private int _aggressionMeterToAggro = 8;

            [SerializeField, Min(0f), SuffixLabel(Seconds, overlay: true)]
            private float _aggressionMeterDecreaseTime = 3f;

            // PROPERTIES: ----------------------------------------------------------------------------

            public float MinLoudnessToReact => _minLoudnessToReact;
            public int AggressionMeterToAggro => _aggressionMeterToAggro;
            public float AggressionMeterDecreaseTime => _aggressionMeterDecreaseTime;
        }

        [Serializable]
        public class AttackSystemConfig
        {
            // MEMBERS: -------------------------------------------------------------------------------

            [SerializeField, Min(0f)]
            private float _spikesDamage = 20f;

            [SerializeField, Min(0f)]
            private float _spikesDamageInterval = 0.2f;

            [SerializeField, Min(0f)]
            private float _instantKillDuration = 0.5f;

            [SerializeField, Min(0f)]
            private float _spikesDuration = 5f;

            [SerializeField, Min(0f)]
            private float _showSpikesAnimationDuration = 0.25f;

            [SerializeField, Min(0f)]
            private float _hideSpikesAnimationDuration = 1f;

            [SerializeField]
            private Ease _showSpikesAnimationEase = Ease.InOutQuad;

            [SerializeField]
            private Ease _hideSpikesAnimationEase = Ease.InOutQuad;

            // PROPERTIES: ----------------------------------------------------------------------------

            public float SpikesDamage => _spikesDamage;
            public float SpikesDamageInterval => _spikesDamageInterval;
            public float InstantKillDuration => _instantKillDuration;
            public float SpikesDuration => _spikesDuration;
            public float ShowSpikesAnimationDuration => _showSpikesAnimationDuration;
            public float HideSpikesAnimationDuration => _hideSpikesAnimationDuration;
            public Ease ShowSpikesAnimationEase => _showSpikesAnimationEase;
            public Ease HideSpikesAnimationEase => _hideSpikesAnimationEase;
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
        public class AnimationConfig
        {
            // MEMBERS: -------------------------------------------------------------------------------

            [SerializeField, Min(0f)]
            private float _calmMultiplier = 0.001f;

            [SerializeField, Min(0f)]
            private float _angryMultiplier = 0.0015f;

            [SerializeField, Min(0f)]
            private float _calmAnimationSpeed = 2f;

            [SerializeField, Min(0f)]
            private float _angryAnimationSpeed = 10f;

            [SerializeField, Min(0f)]
            private float _animationDuration = 0.15f;

            [SerializeField]
            private Ease _animationEase = Ease.InOutQuad;

            // PROPERTIES: ----------------------------------------------------------------------------

            public float CalmMultiplier => _calmMultiplier;
            public float AngryMultiplier => _angryMultiplier;
            public float CalmAnimationSpeed => _calmAnimationSpeed;
            public float AngryAnimationSpeed => _angryAnimationSpeed;
            public float AnimationDuration => _animationDuration;
            public Ease AnimationEase => _animationEase;
        }

        [Serializable]
        public class SizeConfig
        {
            // MEMBERS: -------------------------------------------------------------------------------

            [SerializeField, MinMaxSlider(minValue: 0.1f, maxValue: 5f, showFields: true)]
            private Vector2 _scale;

            [SerializeField, Range(0, 100)]
            private int _chance;

            [SerializeField]
            private bool _onlySurface;

            // PROPERTIES: ----------------------------------------------------------------------------

            public Vector2 Scale => _scale;
            public int Chance => _chance;
            public bool OnlySurface => _onlySurface;

            private string Label => $"'Scale: {_scale}',   'Percent: {_chance}%',   'Only Surface: {_onlySurface}'";
        }

        #endregion
    }
}