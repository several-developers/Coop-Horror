using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace GameCore.Gameplay.EntitiesSystems.MovementLogics
{
    public class WanderingMovementLogic
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public WanderingMovementLogic(Transform transform, NavMeshAgent agent)
        {
            _transform = transform;
            _agent = agent;
        }

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnStuckEvent = delegate { };
        public event Action OnArrivedEvent = delegate { };
        public event Func<float> OnGetWanderingMinDistanceInnerEvent = () => 0f;
        public event Func<float> OnGetWanderingMaxDistanceInnerEvent = () => 1f;

        private const float MinDistance = 0.5f;

        private readonly Transform _transform;
        private readonly NavMeshAgent _agent;

        private Vector3 _targetPoint;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Tick()
        {
            if (IsStuck())
            {
                OnStuckEvent.Invoke();
                return;
            }

            if (!IsArrivedToTargetPoint())
                return;

            OnArrivedEvent.Invoke();
        }

        public bool TrySetDestinationPoint()
        {
            bool isOnNavMesh = IsOnNavMesh();

            if (isOnNavMesh)
                SetDestinationPoint();

            return isOnNavMesh;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SetDestinationPoint()
        {
            _targetPoint = GetRandomPosition();
            _agent.destination = _targetPoint;
        }

        private Vector3 GetRandomPosition()
        {
            float minDistance = OnGetWanderingMinDistanceInnerEvent.Invoke();
            float maxDistance = OnGetWanderingMaxDistanceInnerEvent.Invoke();
            float distance = Random.Range(minDistance, maxDistance);

            Vector2 circle = Random.insideUnitCircle;
            circle *= distance;

            Vector3 circlePosition = new(x: circle.x, y: 0f, z: circle.y);
            Vector3 thisPosition = _transform.position;
            Vector3 newPosition = circlePosition + thisPosition;

            return newPosition;
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