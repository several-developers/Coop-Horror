using GameCore.Configs.Gameplay.Enemies;
using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.Entities.Monsters.Beetle.States
{
    public class WanderingState : IEnterState, ITickableState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public WanderingState(BeetleEntity beetleEntity, BeetleAIConfigMeta beetleAIConfig)
        {
            _beetleEntity = beetleEntity;
            _beetleAIConfig = beetleAIConfig;
            _transform = beetleEntity.transform;
            _agent = beetleEntity.GetAgent();
        }

        // FIELDS: --------------------------------------------------------------------------------

        private const float MinDistance = 0.5f;
        
        private readonly BeetleEntity _beetleEntity;
        private readonly BeetleAIConfigMeta _beetleAIConfig;
        private readonly Transform _transform;
        private readonly NavMeshAgent _agent;

        private Vector3 _targetPoint;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            EnableAgent();

            if (IsOnNavMesh())
                SetDestinationPoint();
            else
                EnterIdleState();
        }

        public void Tick()
        {
            if (IsStuck())
            {
                EnterIdleState();
                return;
            }

            if (!IsArrivedToTargetPoint())
                return;
            
            EnterIdleState();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SetDestinationPoint()
        {
            _targetPoint = GetRandomPosition();
            _agent.destination = _targetPoint;
        }

        private void EnableAgent()
        {
            _agent.enabled = true;
            _agent.speed = GetWanderingSpeed();
        }

        private void EnterIdleState() =>
            _beetleEntity.EnterIdleState();
        
        private Vector3 GetRandomPosition()
        {
            float minDistance = _beetleAIConfig.WanderingMinDistance;
            float maxDistance = _beetleAIConfig.WanderingMaxDistance;
            float distance = Random.Range(minDistance, maxDistance);
            
            Vector2 circle = Random.insideUnitCircle;
            circle *= distance;

            Vector3 circlePosition = new(x: circle.x, y: 0f, z: circle.y);
            Vector3 beetlePosition = _transform.position;
            Vector3 newBeetlePosition = circlePosition + beetlePosition;

            return newBeetlePosition;
        }

        private float GetWanderingSpeed()
        {
            float minSpeed = _beetleAIConfig.WanderingMinSpeed;
            float maxSpeed = _beetleAIConfig.WanderingMaxSpeed;
            float speed = Random.Range(minSpeed, maxSpeed);
            return speed;
        }
        
        private bool IsArrivedToTargetPoint()
        {
            Vector3 position = _transform.position;
            float distance = Vector3.Distance(a: position, b: _targetPoint);
            bool isArrived = distance < MinDistance;
            
            return isArrived;
        }

        private bool IsStuck()
        {
            NavMeshPathStatus pathStatus = _agent.pathStatus;
            bool isStatusValid = pathStatus == NavMeshPathStatus.PathComplete && _agent.hasPath;
            float velocity = _agent.velocity.magnitude;
            bool isStuck = !isStatusValid && velocity < 0.1f;
            return isStuck;
        }
        
        private bool IsOnNavMesh() =>
            _agent.isOnNavMesh;
    }
}