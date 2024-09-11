using System;
using DG.Tweening;
using GameCore.Enums.Gameplay;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Configs.Gameplay.Enemies
{
    public class MushroomAIConfigMeta : MonsterAIConfigMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [TitleGroup(title: WanderingSettings)]
        [BoxGroup(WanderingGroup, showLabel: false), SerializeField]
        private WanderingConfig _wanderingConfig;
        
        [TitleGroup(title: AnimationSettings)]
        [BoxGroup(AnimationGroup, showLabel: false), SerializeField]
        private AnimationConfig _animationConfig;
        
        // FIELDS: --------------------------------------------------------------------------------

        private const string DebugSettings = "Debug Settings";
        private const string ConfigTitle = "Config";
        private const string SFXTitle = "SFX";
        private const string CommonSettings = "Common Settings";
        private const string WanderingSettings = "Wandering Config";
        private const string LookAroundSettings = "Look Around Config";
        private const string SuspicionSystemSettings = "Suspicion System Config";
        private const string SuspicionStateSettings = "Suspicion State Config";
        private const string AnimationSettings = "Animation Config";
        private const string CombatSettings = "Combat Config";
        private const string CageBirdSettings = "Cage Bird Config";
        
        private const string SFXGroup = SFXTitle + "/Group";
        private const string DebugGroup = DebugSettings + "/Group";
        private const string CommonGroup = CommonSettings + "/Group";
        private const string WanderingGroup = WanderingSettings + "/Group";
        private const string LookAroundGroup = LookAroundSettings + "/Group";
        private const string SuspicionSystemGroup = SuspicionSystemSettings + "/Group";
        private const string SuspicionStateGroup = SuspicionStateSettings + "/Group";
        private const string AnimationGroup = AnimationSettings + "/Group";
        private const string CombatGroup = CombatSettings + "/Group";
        private const string CageBirdGroup = CageBirdSettings + "/Group";

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public WanderingConfig GetWanderingConfig() => _wanderingConfig;

        public AnimationConfig GetAnimationConfig() => _animationConfig;
        
        public override MonsterType GetMonsterType() =>
            MonsterType.Mushroom;

        // INNER CLASSES: -------------------------------------------------------------------------

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
        public class AnimationConfig
        {
            // MEMBERS: -------------------------------------------------------------------------------

            [SerializeField, Range(0f, 1f)]
            private float _dampTime = 0.15f;

            [SerializeField, Space(height: 10)]
            private float _modelSittingY = -0.36f;

            [SerializeField, Min(0f)]
            private float _modelSitDownDuration = 0.35f;
            
            [SerializeField, Min(0f)]
            private float _modelStandUpDuration = 0.35f;

            [SerializeField, Min(0f)]
            private float _modelSitDownDelay = 0.2f;
            
            [SerializeField, Min(0f)]
            private float _modelStandUpDelay;
            
            [SerializeField, Min(0f)]
            private float _sitDownAnimationMultiplier = 1f;
            
            [SerializeField, Min(0f)]
            private float _standUpAnimationMultiplier = 1f;

            [SerializeField]
            private Ease _modelSitDownEase = Ease.InOutQuad;
            
            [SerializeField]
            private Ease _modelStandUpEase = Ease.InOutQuad;
            
            [SerializeField, Min(0f), Space(height: 10)]
            private float _hatExplosionDuration = 0.35f;

            [SerializeField, Min(0f)]
            private float _hatRegenerationDuration = 1f;

            [SerializeField]
            private Ease _hatExplosionEase = Ease.InOutQuad;
            
            [SerializeField]
            private Ease _hatRegenerationEase = Ease.InOutQuad;

            // PROPERTIES: ----------------------------------------------------------------------------

            public float DampTime => _dampTime;
            
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
        }
    }
}