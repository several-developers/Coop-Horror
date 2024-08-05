using System;
using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.EntitiesSystems.MovementLogics
{
    public class FollowPositionMovementLogic
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public FollowPositionMovementLogic(Transform transform, NavMeshAgent agent)
        {
            _transform = transform;
            _agent = agent;
        }

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnStuckEvent = delegate { };
        public event Action OnArrivedEvent = delegate { };
        public event Func<Vector3> GetTargetPositionEvent = () => Vector3.zero;
        public event Func<float> GetMinDistanceEvent = () => MinDistance;

        private const float MinDistance = 0.5f;
        
        private readonly Transform _transform;
        private readonly NavMeshAgent _agent;
        
        private Vector3 _previousTargetPoint;
        private Vector3 _targetPoint;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Tick()
        {
            if (IsArrivedToTargetPoint())
            {
                if (IsSameTargetPoint())
                    return;
                
                OnArrivedEvent.Invoke();
                return;
            }
            
            if (IsStuck())
                OnStuckEvent.Invoke();
        }
        
        public bool TryUpdateTargetPoint()
        {
            bool isOnNavMesh = IsOnNavMesh();

            if (isOnNavMesh)
                SetDestinationPoint();

            return isOnNavMesh;
        }
        
        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void SetDestinationPoint()
        {
            _targetPoint = GetTargetPositionEvent.Invoke();
            _agent.destination = _targetPoint;
        }
        
        private bool IsArrivedToTargetPoint()
        {
            Vector3 position = _transform.position;
            float distance = Vector3.Distance(a: position, b: _targetPoint);
            float minDistance = GetMinDistanceEvent.Invoke();
            bool isArrived = distance < minDistance;

            return isArrived;
        }
        
        private bool IsSameTargetPoint()
        {
            bool isSame = _previousTargetPoint == _targetPoint;
            _previousTargetPoint = _targetPoint;
            return isSame;
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