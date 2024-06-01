using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Configs.Gameplay.Enemies
{
    public class BeetleAIConfigMeta : MonsterAIConfigMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [TitleGroup(title: CommonSettings)]
        [BoxGroup(CommonGroup, showLabel: false), SerializeField, Min(0f)]
        private float _fireExitInteractionDistance = 0.5f;
        
        [BoxGroup(CommonGroup), SerializeField, Min(0f)]
        private float _fireExitInteractionDuration = 0.5f;
        
        [BoxGroup(CommonGroup), SerializeField, Min(0f)]
        private float _fireExitDistanceCheckInterval = 0.2f;
        
        [BoxGroup(CommonGroup), SerializeField, Min(0f)]
        private float _moveToDungeonSpeed = 1f;

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

        [TitleGroup(title: TriggerStateSettings)]
        [BoxGroup(TriggerStateGroup, showLabel: false), SerializeField, Min(0f)]
        private float _triggerDistanceCheckInterval = 0.2f;
        
        [BoxGroup(TriggerStateGroup), SerializeField, Min(0f)]
        private float _triggerDistance = 5f;

        [BoxGroup(TriggerStateGroup), SerializeField, Min(0f)]
        private float _aggressionScale = 100f;

        [BoxGroup(TriggerStateGroup), SerializeField, Min(0f)]
        private float _aggressionIncreaseSpeed = 5f;
        
        [BoxGroup(TriggerStateGroup), SerializeField, Min(0f)]
        private float _aggressionDecreaseSpeed = 2f;

        [TitleGroup(title: ScreamStateSettings)]
        [BoxGroup(ScreamStateGroup, showLabel: false), SerializeField, Min(0f)]
        private float _screamDistance = 15f;

        [BoxGroup(ScreamStateGroup), SerializeField, Min(0f)]
        private float _screamCooldown = 5f;

        [TitleGroup(title: ChaseStateSettings)]
        [BoxGroup(ChaseStateGroup, showLabel: false), SerializeField, Min(0f)]
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

        [BoxGroup(AttackStateGroup), SerializeField, Min(0f)]
        private float _damage = 20f;
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public float FireExitInteractionDistance => _fireExitInteractionDistance;
        public float FireExitInteractionDuration => _fireExitInteractionDuration;
        public float FireExitDistanceCheckInterval => _fireExitDistanceCheckInterval;
        public float MoveToDungeonSpeed => _moveToDungeonSpeed;
        
        public float WanderingMinDelay => _wanderingMinDelay;
        public float WanderingMaxDelay => _wanderingMaxDelay;
        
        public float WanderingMinSpeed => _wanderingMinSpeed;
        public float WanderingMaxSpeed => _wanderingMaxSpeed;
        public float WanderingMinDistance => _wanderingMinDistance;
        public float WanderingMaxDistance => _wanderingMaxDistance;

        public float TriggerDistanceCheckInterval => _triggerDistanceCheckInterval;
        public float TriggerDistance => _triggerDistance;
        public float AggressionScale => _aggressionScale;
        public float AggressionIncreaseSpeed => _aggressionIncreaseSpeed;
        public float AggressionDecreaseSpeed => _aggressionDecreaseSpeed;

        public float ScreamDistance => _screamDistance;
        public float ScreamCooldown => _screamCooldown;

        public float ChasePositionCheckInterval => _chasePositionCheckInterval;
        public float ChaseSpeed => _chaseSpeed;
        public float ChaseDistanceCheckInterval => _chaseDistanceCheckInterval;
        public float ChaseStoppingDistance => _chaseStoppingDistance;
        public float MaxChaseDistance => _maxChaseDistance;
        public float ChaseEndDelay => _chaseEndDelay;
        
        // FIELDS: --------------------------------------------------------------------------------

        private const string CommonSettings = "Common Settings";
        private const string IdleStateSettings = "Idle State Settings";
        private const string WanderingStateSettings = "Wandering State Settings";
        private const string TriggerStateSettings = "Trigger State Settings";
        private const string ScreamStateSettings = "Scream State Settings";
        private const string ChaseStateSettings = "Chase State Settings";
        private const string AttackStateSettings = "Attack State Settings";
        
        private const string CommonGroup = CommonSettings + "/Group";
        private const string IdleStateGroup = IdleStateSettings + "/Group";
        private const string WanderingStateGroup = WanderingStateSettings + "/Group";
        private const string TriggerStateGroup = TriggerStateSettings + "/Group";
        private const string ScreamStateGroup = ScreamStateSettings + "/Group";
        private const string ChaseStateGroup = ChaseStateSettings + "/Group";
        private const string AttackStateGroup = AttackStateSettings + "/Group";
    }
}