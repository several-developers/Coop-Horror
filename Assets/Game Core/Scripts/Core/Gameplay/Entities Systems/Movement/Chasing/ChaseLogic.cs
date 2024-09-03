using System;
using System.Collections;
using GameCore.Gameplay.Entities;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Systems.Utilities;
using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.Systems.Movement
{
    public class ChaseLogic
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ChaseLogic(IEntity entity, NavMeshAgent agent)
        {
            MonoBehaviour coroutineRunner = entity.GetMonoBehaviour();
            Transform = entity.GetTransform();
            Entity = entity;
            Agent = agent;
            _chaseRoutine = new CoroutineHelper(coroutineRunner);
            _distanceCheckRoutine = new CoroutineHelper(coroutineRunner);
            _chasingEndTimerRoutine = new CoroutineHelper(coroutineRunner);
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        protected IEntity Entity { get; }
        protected Transform Transform { get; }
        protected NavMeshAgent Agent { get; }

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnTargetNotFoundEvent = delegate { };
        public event Action<PlayerEntity> OnTargetReachedEvent = delegate { };
        public event Action OnChaseEndedEvent = delegate { };
        public event Func<PlayerEntity> GetTargetPlayerEvent = () => null;
        public event Func<Vector3> GetTargetPositionEvent;
        public event Func<float> GetChasePositionCheckIntervalEvent = () => 0.25f;
        public event Func<float> GetChaseDistanceCheckIntervalEvent = () => 0.25f;
        public event Func<float> GetChaseEndDelayEvent = () => 3f;
        public event Func<float> GetMaxChaseDistanceEvent = () => 1f;
        public event Func<float> GetTargetReachDistanceEvent = () => 0.5f;

        private readonly CoroutineHelper _chaseRoutine;
        private readonly CoroutineHelper _distanceCheckRoutine;
        private readonly CoroutineHelper _chasingEndTimerRoutine;

        private Vector3 _targetPoint;
        private bool _isStopChasingTimerEnabled;
        private bool _customTargetPoint;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public virtual void Start()
        {
            _chaseRoutine.GetRoutineEvent += ChaseCO;
            _distanceCheckRoutine.GetRoutineEvent += DistanceCheckCO;
            _chasingEndTimerRoutine.GetRoutineEvent += ChasingEndTimerCO;
            
            _chaseRoutine.Start();
            _distanceCheckRoutine.Start();
        }

        public virtual void Stop()
        {
            _chaseRoutine.GetRoutineEvent -= ChaseCO;
            _distanceCheckRoutine.GetRoutineEvent -= DistanceCheckCO;
            _chasingEndTimerRoutine.GetRoutineEvent -= ChasingEndTimerCO;
            
            _chaseRoutine.Stop();
            _distanceCheckRoutine.Stop();
        }

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected void ToggleCustomTargetPoint(bool customTargetPoint) =>
            _customTargetPoint = customTargetPoint;

        protected PlayerEntity GetTargetPlayer() =>
            GetTargetPlayerEvent.Invoke();
        
        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SetDestination()
        {
            if (!Agent.enabled)
                return;

            bool isTargetPointFound = TryGetDestinationPoint(out _targetPoint);

            if (!isTargetPointFound)
            {
                OnTargetNotFoundEvent.Invoke();
                return;
            }

            Agent.destination = _targetPoint;
        }

        private void CheckDistance()
        {
            PlayerEntity targetPlayer = GetTargetPlayer();
            bool isTargetValid = targetPlayer != null && !targetPlayer.IsDead();

            if (!isTargetValid)
            {
                OnTargetNotFoundEvent.Invoke();
                return;
            }

            Vector3 targetPosition = _targetPoint;
            Vector3 thisPosition = Transform.position;
            float distance = Vector3.Distance(a: thisPosition, b: targetPosition);
            bool isTooFar = distance > GetMaxChaseDistanceEvent.Invoke();
            bool isTargetReached = distance <= GetTargetReachDistanceEvent.Invoke();

            if (isTargetReached)
            {
                OnTargetReachedEvent.Invoke(targetPlayer);
                return;
            }

            if (isTooFar)
            {
                if (_isStopChasingTimerEnabled)
                    return;
                
                _isStopChasingTimerEnabled = true;
                _chasingEndTimerRoutine.Start();
            }
            else
            {
                if (!_isStopChasingTimerEnabled)
                    return;

                _isStopChasingTimerEnabled = false;
                _chasingEndTimerRoutine.Stop();
            }
        }

        private IEnumerator ChaseCO()
        {
            while (true)
            {
                float checkInterval = GetChasePositionCheckIntervalEvent.Invoke();
                yield return new WaitForSeconds(checkInterval);

                SetDestination();
            }
        }

        private IEnumerator DistanceCheckCO()
        {
            while (true)
            {
                float checkInterval = GetChaseDistanceCheckIntervalEvent.Invoke();
                yield return new WaitForSeconds(checkInterval);

                CheckDistance();
            }
        }

        private IEnumerator ChasingEndTimerCO()
        {
            float delay = GetChaseEndDelayEvent.Invoke();
            yield return new WaitForSeconds(delay);

            OnChaseEndedEvent.Invoke();
        }

        private bool TryGetDestinationPoint(out Vector3 result)
        {
            result = Vector3.zero;
            
            if (!Agent.isOnNavMesh)
                return false;
            
            PlayerEntity targetPlayer = GetTargetPlayer();
            bool isTargetValid = targetPlayer != null && !targetPlayer.IsDead();

            if (!isTargetValid)
                return false;
            
            if (_customTargetPoint)
            {
                bool isTargetPositionValid = GetTargetPositionEvent != null;

                if (!isTargetPositionValid)
                    return false;

                result = GetTargetPositionEvent.Invoke();
            }
            else
            {
                result = targetPlayer.transform.position;
            }
            
            return true;
        }
    }
}