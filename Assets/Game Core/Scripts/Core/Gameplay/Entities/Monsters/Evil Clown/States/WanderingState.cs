using GameCore.Configs.Gameplay.Enemies;
using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.Entities.Monsters.EvilClown.States
{
    public class WanderingState : IEnterState, ITickableState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public WanderingState(EvilClownEntity evilClownEntity, EvilClownAIConfigMeta evilClownAIConfig)
        {
            _evilClownEntity = evilClownEntity;
            _evilClownAIConfig = evilClownAIConfig;
            _transform = evilClownEntity.transform;
            _agent = evilClownEntity.GetAgent();
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private const float MinDistance = 0.5f;
        
        private readonly EvilClownEntity _evilClownEntity;
        private readonly EvilClownAIConfigMeta _evilClownAIConfig;
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
                EnterWanderingState();
        }

        public void Tick()
        {
            if (IsStuck())
            {
                EnterWanderingState();
                return;
            }

            if (!IsArrivedToTargetPoint())
                return;
            
            EnterWanderingState();
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

        private Vector3 GetRandomPosition()
        {
            float minDistance = _evilClownAIConfig.WanderingMinDistance;
            float maxDistance = _evilClownAIConfig.WanderingMaxDistance;
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
            float minSpeed = _evilClownAIConfig.WanderingMinSpeed;
            float maxSpeed = _evilClownAIConfig.WanderingMaxSpeed;
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

        private void EnterWanderingState() =>
            _evilClownEntity.EnterWanderingState();
    }
}