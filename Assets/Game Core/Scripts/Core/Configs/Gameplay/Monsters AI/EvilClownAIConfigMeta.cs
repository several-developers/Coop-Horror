﻿using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Configs.Gameplay.Enemies
{
    public class EvilClownAIConfigMeta : MonsterAIConfigMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        [TitleGroup(title: CommonSettings)]
        [BoxGroup(CommonGroup, showLabel: false), SerializeField, Min(0f)]
        private float _test;
        
        [BoxGroup(CommonGroup), SerializeField, Min(0f)]
        private float _test2;
        
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

        // PROPERTIES: ----------------------------------------------------------------------------

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
        
        // FIELDS: --------------------------------------------------------------------------------
        
        private const string CommonSettings = "Common Settings";
        private const string IdleStateSettings = "Idle State Settings";
        private const string WanderingStateSettings = "Wandering State Settings";
        private const string ChaseStateSettings = "Chase State Settings";
        private const string AttackStateSettings = "Attack State Settings";
        
        private const string CommonGroup = CommonSettings + "/Group";
        private const string IdleStateGroup = IdleStateSettings + "/Group";
        private const string WanderingStateGroup = WanderingStateSettings + "/Group";
        private const string ChaseStateGroup = ChaseStateSettings + "/Group";
        private const string AttackStateGroup = AttackStateSettings + "/Group";
    }
}