using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Configs.Gameplay.Enemies
{
    public class GoodClownAIConfigMeta : MonsterAIConfigMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        [TitleGroup(title: HunterSystemSettingsTitle)]
        [BoxGroup(HunterSystemGroup, showLabel: false), SerializeField, HideLabel, InlineProperty]
        private HunterSystemSettings _hunterSystemConfig;

        [TitleGroup(TransformationSettingsTitle)]
        [BoxGroup(TransformationGroup, showLabel: false), SerializeField, HideLabel, InlineProperty]
        private TransformationSettings _transformationConfig;

        [TitleGroup(title: FollowTargetSettingsTitle)]
        [BoxGroup(FollowTargetGroup, showLabel: false), SerializeField, HideLabel, InlineProperty]
        private FollowTargetSettings _followTargetConfig;

        [TitleGroup(title: WanderingSettingsTitle)]
        [BoxGroup(WanderingGroup, showLabel: false), SerializeField, HideLabel, InlineProperty]
        private WanderingSettings _wanderingConfig;

        [TitleGroup(title: HuntingIdleSettingsTitle)]
        [BoxGroup(HuntingIdleGroup, showLabel: false), SerializeField, HideLabel, InlineProperty]
        private HuntingIdleSettings _huntingIdleConfig;

        [TitleGroup(title: HuntingChaseSettingsTitle)]
        [BoxGroup(HuntingChaseGroup, showLabel: false), SerializeField, HideLabel, InlineProperty]
        private HuntingChaseSettings _huntingChaseConfig;

        // PROPERTIES: ----------------------------------------------------------------------------

        public TransformationSettings TransformationConfig => _transformationConfig;
        public FollowTargetSettings FollowTargetConfig => _followTargetConfig;
        public WanderingSettings WanderingConfig => _wanderingConfig;
        public HunterSystemSettings HunterSystemConfig => _hunterSystemConfig;
        public HuntingIdleSettings HuntingIdleConfig => _huntingIdleConfig;
        public HuntingChaseSettings HuntingChaseConfig => _huntingChaseConfig;

        // FIELDS: --------------------------------------------------------------------------------

        private const string CommonSettings = "Common Settings";
        private const string HunterSystemSettingsTitle = "Hunter System Settings";
        private const string TransformationSettingsTitle = "Transformation Settings";
        private const string FollowTargetSettingsTitle = "Follow Target State Settings";
        private const string WanderingSettingsTitle = "Wandering State Settings";
        private const string HuntingIdleSettingsTitle = "Hunting Idle State Settings";
        private const string HuntingChaseSettingsTitle = "Hunting Chase State Settings";
        private const string SleepingSettingsTitle = "Sleeping State Settings";

        private const string CommonGroup = CommonSettings + "/Group";
        private const string HunterSystemGroup = HunterSystemSettingsTitle + "/Group";
        private const string TransformationGroup = TransformationSettingsTitle + "/Group";
        private const string FollowTargetGroup = FollowTargetSettingsTitle + "/Group";
        private const string WanderingGroup = WanderingSettingsTitle + "/Group";
        private const string HuntingIdleGroup = HuntingIdleSettingsTitle + "/Group";
        private const string HuntingChaseGroup = HuntingChaseSettingsTitle + "/Group";
        private const string SleepingGroup = SleepingSettingsTitle + "/Group";

        // INNER CLASSES: -------------------------------------------------------------------------

        #region Inner Classes

        [Serializable]
        public class HunterSystemSettings
        {
            // MEMBERS: -------------------------------------------------------------------------------

            [SerializeField]
            [InfoBox(message: WarningMessage, InfoMessageType.Error, visibleIfMemberName: nameof(_disableHunting))]
            private bool _disableHunting;

            [SerializeField, Min(0f)]
            [Tooltip("Длительность охоты, после окончания которой клоун становится злым.")]
            private float _huntingDuration = 15f;
            
            [SerializeField, Min(0f)]
            [Tooltip("Интервал проверки доступности охоты.")]
            private float _huntingCheckInterval = 0.5f;

            [SerializeField, Min(0f)]
            [Tooltip("Интервал проверки возможности трансформации в злую версию.")]
            private float _transformationCheckInterval = 0.1f;

            [SerializeField, Min(0f)]
            [Tooltip("Дистанция до цели при которой доступна охота.")]
            private float _targetDistanceForHunt = 20f;

            [SerializeField, Min(0f)]
            [Tooltip("Дистанция до других игроков, чтобы охота была доступна.")]
            private float _distanceToOtherPlayersForHunt = 40f;

            // PROPERTIES: ----------------------------------------------------------------------------

            public bool DisableHunting => _disableHunting;
            public float HuntingDuration => _huntingDuration;
            public float HuntingCheckInterval => _huntingCheckInterval;
            public float TransformationCheckInterval => _transformationCheckInterval;
            public float TargetDistanceForHunt => _targetDistanceForHunt;
            public float DistanceToOtherPlayersForHunt => _distanceToOtherPlayersForHunt;

            // FIELDS: --------------------------------------------------------------------------------

            private const string WarningMessage = "Warning! This should be disabled!";
        }

        [Serializable]
        public class TransformationSettings
        {
            // MEMBERS: -------------------------------------------------------------------------------
            
            [SerializeField, Min(0f)]
            [Tooltip("Задержка перед трансформацией.")]
            private float _transformationDelay = 5f;

            // PROPERTIES: ----------------------------------------------------------------------------

            public float TransformationDelay => _transformationDelay;
        }

        [Serializable]
        public class FollowTargetSettings
        {
            // MEMBERS: -------------------------------------------------------------------------------
            
            [SerializeField, Min(0f)]
            private float _positionCheckInterval = 0.1f;

            [SerializeField, Min(0f)]
            private float _moveSpeed = 5f;

            [SerializeField, Min(0f)]
            private float _distanceCheckInterval = 0.1f;

            [SerializeField, Min(0f)]
            private float _stoppingDistance = 0.5f;

            [SerializeField, Min(0f)]
            private float _reachDistance = 4f;

            [SerializeField, Min(0f)]
            private float _maxFollowDistance = 10f;

            [SerializeField, Min(0f)]
            private float _followEndDelay = 5f;

            // PROPERTIES: ----------------------------------------------------------------------------
            
            public float PositionCheckInterval => _positionCheckInterval;
            public float MoveSpeed => _moveSpeed;
            public float DistanceCheckInterval => _distanceCheckInterval;
            public float StoppingDistance => _stoppingDistance;
            public float ReachDistance => _reachDistance;
            public float MaxFollowDistance => _maxFollowDistance;
            public float FollowEndDelay => _followEndDelay;
        }

        [Serializable]
        public class WanderingSettings
        {
            // MEMBERS: -------------------------------------------------------------------------------
            
            [SerializeField, Min(0f)]
            [Tooltip("Минимальная скорость блуждания.")]
            private float _minSpeed = 1f;

            [SerializeField, Min(0f)]
            [Tooltip("Максимальная скорость блуждания.")]
            private float _maxSpeed = 2.5f;

            [SerializeField, Min(0f)]
            [Tooltip("Минимальная дистанция от цели.")]
            private float _minDistance = 2f;

            [SerializeField, Min(0f)]
            [Tooltip("Максимальная дистанция от цели.")]
            private float _maxDistance = 10f;

            [SerializeField, Min(0f)]
            [Tooltip("Минимальная задержка перед новой позицией блуждания.")]
            private float _minDelay = 0.5f;

            [SerializeField, Min(0f)]
            [Tooltip("Максимальная задержка перед новой позицией блуждания.")]
            private float _maxDelay = 5f;

            [SerializeField, Min(0f)]
            private float _distanceToBreakWandering = 15f;

            [SerializeField, Min(0f)]
            private float _wanderingDistanceBreakCheckInterval = 0.25f;

            // PROPERTIES: ----------------------------------------------------------------------------
            
            public float MinSpeed => _minSpeed;
            public float MaxSpeed => _maxSpeed;
            public float MinDistance => _minDistance;
            public float MaxDistance => _maxDistance;
            public float MinDelay => _minDelay;
            public float MaxDelay => _maxDelay;
            public float DistanceToBreakWandering => _distanceToBreakWandering;
            public float WanderingDistanceBreakCheckInterval => _wanderingDistanceBreakCheckInterval;
        }

        [Serializable]
        public class HuntingIdleSettings
        {
            // MEMBERS: -------------------------------------------------------------------------------

            [SerializeField, Min(0f)]
            private float _minDistanceToChase = 10f;

            [SerializeField, Min(0f)]
            private float _distanceCheckInterval = 0.2f;

            [SerializeField, Min(0f)]
            private float _rotationSpeed = 5f;

            // PROPERTIES: ----------------------------------------------------------------------------

            public float MinDistanceToChase => _minDistanceToChase;
            public float DistanceCheckInterval => _distanceCheckInterval;
            public float RotationSpeed => _rotationSpeed;
        }
        
        [Serializable]
        public class HuntingChaseSettings
        {
            // MEMBERS: -------------------------------------------------------------------------------
            
            [SerializeField, Min(0f)]
            private float _positionCheckInterval = 0.1f;

            [SerializeField, Min(0f)]
            private float _moveSpeed = 5f;

            [SerializeField, Min(0f)]
            private float _distanceCheckInterval = 0.1f;

            [SerializeField, Min(0f)]
            private float _stoppingDistance = 0.5f;

            [SerializeField, Min(0f)]
            private float _reachDistance = 4f;

            [SerializeField, Min(0f)]
            private float _maxChaseDistance = 10f;

            [SerializeField, Min(0f)]
            private float _chaseEndDelay = 5f;

            // PROPERTIES: ----------------------------------------------------------------------------
            
            public float PositionCheckInterval => _positionCheckInterval;
            public float MoveSpeed => _moveSpeed;
            public float DistanceCheckInterval => _distanceCheckInterval;
            public float StoppingDistance => _stoppingDistance;
            public float ReachDistance => _reachDistance;
            public float MaxChaseDistance => _maxChaseDistance;
            public float ChaseEndDelay => _chaseEndDelay;
        }

        #endregion
    }
}