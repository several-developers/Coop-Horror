using System.Collections;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Entities.Player;
using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.Entities.Monsters.Beetle.States
{
    public class ChaseState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        public ChaseState(BeetleEntity beetleEntity, BeetleAIConfigMeta beetleAIConfig)
        {
            _beetleEntity = beetleEntity;
            _beetleAIConfig = beetleAIConfig;
            _transform = beetleEntity.transform;
            _agent = beetleEntity.GetAgent();
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly BeetleEntity _beetleEntity;
        private readonly BeetleAIConfigMeta _beetleAIConfig;
        private readonly Transform _transform;
        private readonly NavMeshAgent _agent;

        private Coroutine _chaseCO;
        private Coroutine _distanceCheckCO;
        private Coroutine _chasingEndTimerCO;
        private float _startStoppingDistance;
        private bool _isStopChasingTimerEnabled;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            EnableAgent();
            StartChasing();
            StartDistanceCheck();
        }

        public void Exit()
        {
            ResetAgent();
            StopChasing();
            StopDistanceCheck();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void EnableAgent()
        {
            NavMeshAgent agent = _beetleEntity.GetAgent();
            _startStoppingDistance = agent.stoppingDistance;
            
            agent.enabled = true;
            agent.speed = _beetleAIConfig.ChaseSpeed;
            agent.stoppingDistance = _beetleAIConfig.ChaseStoppingDistance;
        }

        private void ResetAgent()
        {
            NavMeshAgent agent = _beetleEntity.GetAgent();
            agent.stoppingDistance = _startStoppingDistance;
        }
        
        private void SetDestination()
        {
            PlayerEntity targetPlayer = _beetleEntity.GetTargetPlayer();
            bool isTargetExists = targetPlayer != null;

            if (!isTargetExists)
            {
                EnterTriggerState();
                return;
            }

            Vector3 targetPosition = targetPlayer.transform.position;
            _agent.destination = targetPosition;
        }

        private void CheckDistance()
        {
            PlayerEntity targetPlayer = _beetleEntity.GetTargetPlayer();
            bool isTargetExists = targetPlayer != null;

            if (!isTargetExists)
            {
                EnterTriggerState();
                return;
            }

            Vector3 targetPosition = targetPlayer.transform.position;
            Vector3 beetlePosition = _transform.position;
            float distance = Vector3.Distance(a: beetlePosition, b: targetPosition);
            bool isTooFar = distance > _beetleAIConfig.MaxChaseDistance;

            if (isTooFar)
                StartChasingEndTimer();
            else
                StopChasingEndTimer();
        }
        
        private void StartChasing()
        {
            IEnumerator routine = ChaseCO();
            _chaseCO = _beetleEntity.StartCoroutine(routine);
        }

        private void StopChasing()
        {
            if (_chaseCO == null)
                return;
            
            _beetleEntity.StopCoroutine(_chaseCO);
        }

        private void StartDistanceCheck()
        {
            IEnumerator routine = DistanceCheckCO();
            _distanceCheckCO = _beetleEntity.StartCoroutine(routine);
        }

        private void StopDistanceCheck()
        {
            if (_distanceCheckCO == null)
                return;
            
            _beetleEntity.StopCoroutine(_distanceCheckCO);
        }

        private void StartChasingEndTimer()
        {
            if (_isStopChasingTimerEnabled)
                return;

            IEnumerator routine = ChasingEndTimerCO();
            _chasingEndTimerCO = _beetleEntity.StartCoroutine(routine);
            _isStopChasingTimerEnabled = true;
        }

        private void StopChasingEndTimer()
        {
            if (!_isStopChasingTimerEnabled)
                return;

            if (_chasingEndTimerCO == null)
                return;

            _beetleEntity.StopCoroutine(_chasingEndTimerCO);
            _isStopChasingTimerEnabled = false;
        }
        
        private IEnumerator ChaseCO()
        {
            while (true)
            {
                float checkInterval = _beetleAIConfig.ChasePositionCheckInterval;
                yield return new WaitForSeconds(checkInterval);
                
                SetDestination();
            }
        }

        private IEnumerator DistanceCheckCO()
        {
            while (true)
            {
                float checkInterval = _beetleAIConfig.ChaseDistanceCheckInterval;
                yield return new WaitForSeconds(checkInterval);
                
                CheckDistance();
            }
        }
        
        private IEnumerator ChasingEndTimerCO()
        {
            float delay = _beetleAIConfig.ChaseEndDelay;
            yield return new WaitForSeconds(delay);
                
            EnterTriggerState();
        }
        
        private void EnterTriggerState() =>
            _beetleEntity.EnterTriggerState();
    }
}