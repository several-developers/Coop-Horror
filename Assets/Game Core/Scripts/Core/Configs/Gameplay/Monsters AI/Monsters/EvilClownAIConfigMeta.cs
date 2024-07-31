using System;
using GameCore.Enums.Gameplay;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Configs.Gameplay.Enemies
{
    public class EvilClownAIConfigMeta : MonsterAIConfigMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        [TitleGroup(title: CommonSettings)]
        [BoxGroup(CommonGroup, showLabel: false), SerializeField, Min(0f)]
        private float _fireExitInteractionDistance = 0.5f;
        
        [BoxGroup(CommonGroup), SerializeField, Min(0f)]
        private float _fireExitInteractionDuration = 0.5f;
        
        [TitleGroup(title: ChaseStateSettings)]
        [BoxGroup(ChaseStateGroup, showLabel: false), SerializeField, Min(0f)]
        private float _chaseDelay = 3f;
        
        [BoxGroup(ChaseStateGroup), SerializeField, Min(0f)]
        private float _chasePositionCheckInterval = 0.1f;
        
        [BoxGroup(ChaseStateGroup), SerializeField, Min(0f)]
        private float _chaseSpeed = 5f;

        [BoxGroup(ChaseStateGroup), SerializeField, Min(0f)]
        private float _chaseDistanceCheckInterval = 0.1f;
        
        [BoxGroup(ChaseStateGroup), SerializeField, Min(0f)]
        private float _chaseStoppingDistance = 0.5f;
        
        [BoxGroup(ChaseStateGroup), SerializeField, Min(0f)]
        private float _maxChaseDistance = 10f;

        [BoxGroup(ChaseStateGroup), SerializeField, Min(0f)]
        private float _chaseEndDelay = 5f;
        
        [TitleGroup(title: AttackStateSettings)]
        [BoxGroup(AttackStateGroup, showLabel: false), SerializeField, Min(0f)]
        private float _attackDistance = 1f;

        [BoxGroup(AttackStateGroup), SerializeField, Min(0f)]
        private float _attackCooldown = 2f;

        [TitleGroup(title: WanderingStateSettings)]
        [BoxGroup(WanderingStateGroup, showLabel: false), SerializeField, Min(0f)]
        private float _wanderingMinSpeed = 1f;
        
        [BoxGroup(WanderingStateGroup), SerializeField, Min(0f)]
        private float _wanderingMaxSpeed = 2.5f;
        
        [BoxGroup(WanderingStateGroup), SerializeField, Min(0f)]
        private float _wanderingMinDistance = 1f;
        
        [BoxGroup(WanderingStateGroup), SerializeField, Min(0f)]
        private float _wanderingMaxDistance = 15f;

        [TitleGroup(AnimationSettingsTitle)]
        [BoxGroup(AnimationGroup, showLabel: false), SerializeField, HideLabel, InlineProperty]
        private AnimationSettings _animationConfig;

        // PROPERTIES: ----------------------------------------------------------------------------

        public float FireExitInteractionDistance => _fireExitInteractionDistance;
        public float FireExitInteractionDuration => _fireExitInteractionDuration;
        
        public float ChaseDelay => _chaseDelay;
        public float ChasePositionCheckInterval => _chasePositionCheckInterval;
        public float ChaseSpeed => _chaseSpeed;
        public float ChaseDistanceCheckInterval => _chaseDistanceCheckInterval;
        public float ChaseStoppingDistance => _chaseStoppingDistance;
        public float MaxChaseDistance => _maxChaseDistance;
        public float ChaseEndDelay => _chaseEndDelay;
        
        public float AttackDistance => _attackDistance;
        public float AttackCooldown => _attackCooldown;
        
        public float WanderingMinSpeed => _wanderingMinSpeed;
        public float WanderingMaxSpeed => _wanderingMaxSpeed;
        public float WanderingMinDistance => _wanderingMinDistance;
        public float WanderingMaxDistance => _wanderingMaxDistance;

        public AnimationSettings AnimationConfig => _animationConfig;
        
        // FIELDS: --------------------------------------------------------------------------------
        
        private const string CommonSettings = "Common Settings";
        private const string IdleStateSettings = "Idle State Settings";
        private const string WanderingStateSettings = "Wandering State Settings";
        private const string ChaseStateSettings = "Chase State Settings";
        private const string AttackStateSettings = "Attack State Settings";
        private const string AnimationSettingsTitle = "Animation Settings";
        
        private const string CommonGroup = CommonSettings + "/Group";
        private const string IdleStateGroup = IdleStateSettings + "/Group";
        private const string WanderingStateGroup = WanderingStateSettings + "/Group";
        private const string ChaseStateGroup = ChaseStateSettings + "/Group";
        private const string AttackStateGroup = AttackStateSettings + "/Group";
        private const string AnimationGroup = AnimationSettingsTitle + "/Group";

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public override MonsterType GetMonsterType() => MonsterType.EvilClown;
        
        // INNER CLASSES: -------------------------------------------------------------------------

        #region Inner Classes

        [Serializable]
        public class AnimationSettings
        {
            // MEMBERS: -------------------------------------------------------------------------------

            [SerializeField, Min(0f)]
            private float _distanceCheckInterval = 0.2f;

            [SerializeField, Min(0f)]
            private float _typeChangeDuration = 0.15f;
            
            [SerializeField, Min(0f)]
            private float _runningFirstTypeDistance = 25f;

            [SerializeField, Min(0f)]
            private float _runningSecondTypeDistance = 15f;

            // PROPERTIES: ----------------------------------------------------------------------------

            public float DistanceCheckInterval => _distanceCheckInterval;
            public float TypeChangeDuration => _typeChangeDuration;
            public float RunningFirstTypeDistance => _runningFirstTypeDistance;
            public float RunningSecondTypeDistance => _runningSecondTypeDistance;
        }

        #endregion
    }
}