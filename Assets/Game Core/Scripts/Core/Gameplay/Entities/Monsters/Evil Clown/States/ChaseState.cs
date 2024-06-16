using System.Collections;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Entities.Player;
using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.Entities.Monsters.EvilClown.States
{
    public class ChaseState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ChaseState(EvilClownEntity evilClownEntity, EvilClownAIConfigMeta evilClownAIConfig)
        {
            _evilClownEntity = evilClownEntity;
            _evilClownAIConfig = evilClownAIConfig;
            _transform = evilClownEntity.transform;
            _agent = evilClownEntity.GetAgent();
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly EvilClownEntity _evilClownEntity;
        private readonly EvilClownAIConfigMeta _evilClownAIConfig;
        private readonly Transform _transform;
        private readonly NavMeshAgent _agent;

        private Coroutine _chaseCO;
        private Coroutine _distanceCheckCO;
        private Coroutine _chasingEndTimerCO;
        private float _cachedStoppingDistance;
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
            NavMeshAgent agent = _evilClownEntity.GetAgent();
            _cachedStoppingDistance = agent.stoppingDistance;
            
            agent.enabled = true;
            agent.speed = _evilClownAIConfig.ChaseSpeed;
            agent.stoppingDistance = _evilClownAIConfig.ChaseStoppingDistance;
        }

        private void ResetAgent()
        {
            NavMeshAgent agent = _evilClownEntity.GetAgent();
            agent.stoppingDistance = _cachedStoppingDistance;
        }
        
        private void SetDestination()
        {
            PlayerEntity targetPlayer = _evilClownEntity.GetTargetPlayer();
            bool isTargetExists = targetPlayer != null;

            if (!isTargetExists)
            {
                EnterWanderingState();
                return;
            }

            Vector3 targetPosition = targetPlayer.transform.position;
            _agent.destination = targetPosition;
        }

        private void CheckDistance()
        {
            PlayerEntity targetPlayer = _evilClownEntity.GetTargetPlayer();
            bool isTargetExists = targetPlayer != null;

            if (!isTargetExists)
            {
                EnterWanderingState();
                return;
            }

            Vector3 targetPosition = targetPlayer.transform.position;
            Vector3 beetlePosition = _transform.position;
            float distance = Vector3.Distance(a: beetlePosition, b: targetPosition);
            bool isTooFar = distance > _evilClownAIConfig.MaxChaseDistance;
            bool canAttack = distance <= _evilClownAIConfig.AttackDistance;

            if (canAttack)
            {
                EnterAttackState();
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
            _chaseCO = _evilClownEntity.StartCoroutine(routine);
        }

        private void StopChasing()
        {
            if (_chaseCO == null)
                return;
            
            _evilClownEntity.StopCoroutine(_chaseCO);
        }

        private void StartDistanceCheck()
        {
            IEnumerator routine = DistanceCheckCO();
            _distanceCheckCO = _evilClownEntity.StartCoroutine(routine);
        }

        private void StopDistanceCheck()
        {
            if (_distanceCheckCO == null)
                return;
            
            _evilClownEntity.StopCoroutine(_distanceCheckCO);
        }

        private void StartChasingEndTimer()
        {
            if (_isStopChasingTimerEnabled)
                return;

            IEnumerator routine = ChasingEndTimerCO();
            _chasingEndTimerCO = _evilClownEntity.StartCoroutine(routine);
            _isStopChasingTimerEnabled = true;
        }

        private void StopChasingEndTimer()
        {
            if (!_isStopChasingTimerEnabled)
                return;

            if (_chasingEndTimerCO == null)
                return;

            _evilClownEntity.StopCoroutine(_chasingEndTimerCO);
            _isStopChasingTimerEnabled = false;
        }
        
        
        private IEnumerator ChaseCO()
        {
            while (true)
            {
                float checkInterval = _evilClownAIConfig.ChasePositionCheckInterval;
                yield return new WaitForSeconds(checkInterval);
                
                SetDestination();
            }
        }

        private IEnumerator DistanceCheckCO()
        {
            while (true)
            {
                float checkInterval = _evilClownAIConfig.ChaseDistanceCheckInterval;
                yield return new WaitForSeconds(checkInterval);
                
                CheckDistance();
            }
        }
        
        private IEnumerator ChasingEndTimerCO()
        {
            float delay = _evilClownAIConfig.ChaseEndDelay;
            yield return new WaitForSeconds(delay);
                
            EnterWanderingState();
        }

        private void EnterAttackState() =>
            _evilClownEntity.EnterAttackState();

        private void EnterWanderingState() =>
            _evilClownEntity.EnterWanderingState();
    }
}