using System;
using System.Collections;
using GameCore.Gameplay.Entities.Player;
using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.EntitiesSystems.CombatLogics
{
    public class ChaseLogic
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ChaseLogic(MonoBehaviour coroutineRunner, NavMeshAgent agent)
        {
            _coroutineRunner = coroutineRunner;
            _transform = coroutineRunner.transform;
            _agent = agent;
        }

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnTargetNotFoundEvent = delegate { };
        public event Action<PlayerEntity> OnTargetReachedEvent = delegate { };
        public event Action OnChaseEndedEvent = delegate { };
        public event Func<PlayerEntity> GetTargetPlayerEvent = () => null; 
        public event Func<float> GetChasePositionCheckIntervalEvent = () => 0.25f;
        public event Func<float> GetChaseDistanceCheckIntervalEvent = () => 0.25f;
        public event Func<float> GetChaseEndDelayEvent = () => 3f;
        public event Func<float> GetMaxChaseDistanceEvent = () => 1f; 
        public event Func<float> GetTargetReachDistanceEvent = () => 0.5f; 

        private readonly MonoBehaviour _coroutineRunner;
        private readonly Transform _transform;
        private readonly NavMeshAgent _agent;
        
        private Coroutine _chaseCO;
        private Coroutine _distanceCheckCO;
        private Coroutine _chasingEndTimerCO;
        private bool _isStopChasingTimerEnabled;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Start()
        {
            StartChasing();
            StartDistanceCheck();
        }

        public void Stop()
        {
            StopChasing();
            StopDistanceCheck();
        }
        
        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void SetDestination()
        {
            if (!_agent.enabled)
                return;
            
            PlayerEntity targetPlayer = GetTargetPlayer();
            bool isTargetExists = targetPlayer != null;

            if (!isTargetExists)
            {
                OnTargetNotFoundEvent.Invoke();
                return;
            }

            Vector3 targetPosition = targetPlayer.transform.position;
            _agent.destination = targetPosition;
        }
        
        private void CheckDistance()
        {
            PlayerEntity targetPlayer = GetTargetPlayer();
            bool isTargetExists = targetPlayer != null;

            if (!isTargetExists)
            {
                OnTargetNotFoundEvent.Invoke();
                return;
            }

            Vector3 targetPosition = targetPlayer.transform.position;
            Vector3 thisPosition = _transform.position;
            float distance = Vector3.Distance(a: thisPosition, b: targetPosition);
            bool isTooFar = distance > GetMaxChaseDistanceEvent.Invoke();
            bool isTargetReached = distance <= GetTargetReachDistanceEvent.Invoke();

            if (isTargetReached)
            {
                OnTargetReachedEvent.Invoke(targetPlayer);
                return;
            }
            
            if (isTooFar)
                StartChasingEndTimer();
            else
                StopChasingEndTimer();
        }
        
        private void StartChasing()
        {
            IEnumerator routine = ChaseCO();
            _chaseCO = _coroutineRunner.StartCoroutine(routine);
        }

        private void StopChasing()
        {
            if (_chaseCO == null)
                return;
            
            _coroutineRunner.StopCoroutine(_chaseCO);
        }

        private void StartDistanceCheck()
        {
            IEnumerator routine = DistanceCheckCO();
            _distanceCheckCO = _coroutineRunner.StartCoroutine(routine);
        }

        private void StopDistanceCheck()
        {
            if (_distanceCheckCO == null)
                return;
            
            _coroutineRunner.StopCoroutine(_distanceCheckCO);
        }

        private void StartChasingEndTimer()
        {
            if (_isStopChasingTimerEnabled)
                return;

            IEnumerator routine = ChasingEndTimerCO();
            _chasingEndTimerCO = _coroutineRunner.StartCoroutine(routine);
            _isStopChasingTimerEnabled = true;
        }

        private void StopChasingEndTimer()
        {
            if (!_isStopChasingTimerEnabled)
                return;

            if (_chasingEndTimerCO == null)
                return;

            _coroutineRunner.StopCoroutine(_chasingEndTimerCO);
            _isStopChasingTimerEnabled = false;
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

        private PlayerEntity GetTargetPlayer() =>
            GetTargetPlayerEvent.Invoke();
    }
}