using GameCore.Enums.Gameplay;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Configs.Gameplay.Enemies
{
    public class BlindCreatureAIConfigMeta : MonsterAIConfigMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [TitleGroup(title: SuspicionSystemSettings)]
        [BoxGroup(SuspicionSystemGroup, showLabel: false), SerializeField, Min(0f)]
        private float _minLoudnessToReact = 0.1f;
        
        [BoxGroup(SuspicionSystemGroup), SerializeField, Min(0)]
        private int _suspicionMeterToAggro = 8;

        [BoxGroup(SuspicionSystemGroup), SerializeField, Min(0)]
        private int _suspicionMeterMaxAmount = 15;

        [BoxGroup(SuspicionSystemGroup), SerializeField, Min(0)]
        private int _instantAggroSuspicionMeterAmount = 11;

        [BoxGroup(SuspicionSystemGroup), SerializeField, Min(0f)]
        private float _suspicionMeterDecreaseTime = 3f;

        [TitleGroup(title: IdleStateSettings)]
        [BoxGroup(IdleStateGroup, showLabel: false), SerializeField, Min(0f)]
        private float _wanderingMinDelay = 0.5f;
        
        [BoxGroup(IdleStateGroup), SerializeField, Min(0f)]
        private float _wanderingMaxDelay = 5f;

        [TitleGroup(title: WanderingStateSettings)]
        [BoxGroup(WanderingStateGroup, showLabel: false), SerializeField, Min(0f)]
        private float _wanderingMinSpeed = 1f;
        
        [BoxGroup(WanderingStateGroup), SerializeField, Min(0f)]
        private float _wanderingMaxSpeed = 2.5f;
        
        [BoxGroup(WanderingStateGroup), SerializeField, Min(0f)]
        private float _wanderingMinDistance = 1f;
        
        [BoxGroup(WanderingStateGroup), SerializeField, Min(0f)]
        private float _wanderingMaxDistance = 15f;
        
        [TitleGroup(title: SuspicionMovementStateSettings)]
        [BoxGroup(SuspicionMovementStateGroup, showLabel: false), SerializeField, Min(0f)]
        private float _suspicionMoveSpeed = 5f;

        [TitleGroup(title: AttackStateSettings)]
        [BoxGroup(AttackStateGroup, showLabel: false), SerializeField, Min(0f)]
        private float _attackDistance = 1f;

        [BoxGroup(AttackStateGroup), SerializeField, Min(0f)]
        private float _attackCooldown = 2f;

        // PROPERTIES: ----------------------------------------------------------------------------

        // Suspicion System
        public float MinLoudnessToReact => _minLoudnessToReact;
        public int SuspicionMeterToAggro => _suspicionMeterToAggro;
        public int SuspicionMeterMaxAmount => _suspicionMeterMaxAmount;
        public int InstantAggroSuspicionMeterAmount => _instantAggroSuspicionMeterAmount;
        public float SuspicionMeterDecreaseTime => _suspicionMeterDecreaseTime;

        public float WanderingMinDelay => _wanderingMinDelay;
        public float WanderingMaxDelay => _wanderingMaxDelay;
        
        public float WanderingMinSpeed => _wanderingMinSpeed;
        public float WanderingMaxSpeed => _wanderingMaxSpeed;
        public float WanderingMinDistance => _wanderingMinDistance;
        public float WanderingMaxDistance => _wanderingMaxDistance;

        public float SuspicionMoveSpeed => _suspicionMoveSpeed;

        public float AttackDistance => _attackDistance;
        public float AttackCooldown => _attackCooldown;

        // FIELDS: --------------------------------------------------------------------------------

        private const string CommonSettings = "Common Settings";
        private const string IdleStateSettings = "Idle State Settings";
        private const string WanderingStateSettings = "Wandering State Settings";
        private const string SuspicionMovementStateSettings = "Suspicion Movement State Settings";
        private const string AttackStateSettings = "Attack State Settings";
        private const string SuspicionSystemSettings = "Suspicion System Settings";
        
        private const string CommonGroup = CommonSettings + "/Group";
        private const string IdleStateGroup = IdleStateSettings + "/Group";
        private const string WanderingStateGroup = WanderingStateSettings + "/Group";
        private const string SuspicionMovementStateGroup = SuspicionMovementStateSettings + "/Group";
        private const string AttackStateGroup = AttackStateSettings + "/Group";
        private const string SuspicionSystemGroup = SuspicionSystemSettings + "/Group";

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public override MonsterType GetMonsterType() => MonsterType.BlindCreature;
    }
}