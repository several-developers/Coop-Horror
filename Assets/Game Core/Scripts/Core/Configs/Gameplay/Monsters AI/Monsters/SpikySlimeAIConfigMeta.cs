﻿using System;
using DG.Tweening;
using GameCore.Enums.Gameplay;
using Sirenix.OdinInspector;
using Sonity;
using UnityEngine;

namespace GameCore.Configs.Gameplay.Enemies
{
    public class SpikySlimeAIConfigMeta : MonsterAIConfigMeta
    {
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
        
        [TitleGroup(SFXTitle)]
        [BoxGroup(SFXGroup, showLabel: false), SerializeField, Required]
        private SoundEvent _footstepsSE;

        // FIELDS: --------------------------------------------------------------------------------

        private const string AggressionSystemSettings = "Aggression System";
        private const string AttackSystemSettings = "Attack System";
        
        private const string AggressionSystemGroup =  AggressionSystemSettings + "/Group";
        private const string AttackSystemGroup =  AttackSystemSettings + "/Group";
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public AggressionSystemConfig GetAggressionSystemConfig() => _aggressionSystemConfig;
        public AttackSystemConfig GetAttackSystemConfig() => _attackSystemConfig;
        public WanderingConfig GetWanderingConfig() => _wanderingConfig;
        public AnimationConfig GetAnimationConfig() => _animationConfig;
        
        public override MonsterType GetMonsterType() =>
            MonsterType.SpikySlime;

        // INNER CLASSES: -------------------------------------------------------------------------

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
            private float _damageFromSpikes = 20f;

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

            public float DamageFromSpikes => _damageFromSpikes;
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
    }
}