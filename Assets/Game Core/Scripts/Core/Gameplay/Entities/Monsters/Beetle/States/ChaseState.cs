using System.Collections;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.EntitiesSystems.CombatLogics;
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
            _chaseLogic = new ChaseLogic(beetleEntity, _agent);
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly BeetleEntity _beetleEntity;
        private readonly BeetleAIConfigMeta _beetleAIConfig;
        private readonly Transform _transform;
        private readonly NavMeshAgent _agent;
        private readonly ChaseLogic _chaseLogic;

        private Coroutine _chaseCO;
        private Coroutine _distanceCheckCO;
        private Coroutine _chasingEndTimerCO;
        private float _cachedStoppingDistance;
        private bool _isStopChasingTimerEnabled;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            _chaseLogic.OnTargetNotFoundEvent += OnTargetNotFound;
            _chaseLogic.OnTargetReachedEvent += OnTargetReached;
            _chaseLogic.OnChaseEndedEvent += OnChaseEnded;
            _chaseLogic.GetTargetPlayerEvent += GetTargetPlayer;
            _chaseLogic.GetChasePositionCheckIntervalEvent += GetChasePositionCheckInterval;
            _chaseLogic.GetChaseDistanceCheckIntervalEvent += GetChaseDistanceCheckInterval;
            _chaseLogic.GetChaseEndDelayEvent += GetChaseEndDelay;
            _chaseLogic.GetMaxChaseDistanceEvent += GetMaxChaseDistance;
            _chaseLogic.GetTargetReachDistanceEvent += GetTargetReachDistance;
            
            ToggleTriggerCheckState(isEnabled: false);
            EnableAgent();
            _chaseLogic.Start();
        }

        public void Exit()
        {
            _chaseLogic.OnTargetNotFoundEvent -= OnTargetNotFound;
            _chaseLogic.OnTargetReachedEvent -= OnTargetReached;
            _chaseLogic.OnChaseEndedEvent -= OnChaseEnded;
            _chaseLogic.GetTargetPlayerEvent -= GetTargetPlayer;
            _chaseLogic.GetChasePositionCheckIntervalEvent -= GetChasePositionCheckInterval;
            _chaseLogic.GetChaseDistanceCheckIntervalEvent -= GetChaseDistanceCheckInterval;
            _chaseLogic.GetChaseEndDelayEvent -= GetChaseEndDelay;
            _chaseLogic.GetMaxChaseDistanceEvent -= GetMaxChaseDistance;
            _chaseLogic.GetTargetReachDistanceEvent -= GetTargetReachDistance;
            
            ToggleTriggerCheckState(isEnabled: true);
            ResetAgent();
            _chaseLogic.Stop();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ToggleTriggerCheckState(bool isEnabled)
        {
            AggressionSystem aggressionSystem = _beetleEntity.GetAggressionSystem();
            aggressionSystem.ToggleTriggerCheckState(isEnabled);
        }

        private void EnableAgent()
        {
            NavMeshAgent agent = _beetleEntity.GetAgent();
            _cachedStoppingDistance = agent.stoppingDistance;
            
            agent.enabled = true;
            agent.speed = _beetleAIConfig.ChaseSpeed;
            agent.stoppingDistance = _beetleAIConfig.ChaseStoppingDistance;
        }

        private void ResetAgent()
        {
            NavMeshAgent agent = _beetleEntity.GetAgent();
            agent.stoppingDistance = _cachedStoppingDistance;
        }

        private void EnterTriggerState() =>
            _beetleEntity.EnterTriggerState();

        private void EnterAttackState() =>
            _beetleEntity.EnterAttackState();

        private PlayerEntity GetTargetPlayer() =>
            _beetleEntity.GetTargetPlayer();

        private float GetChasePositionCheckInterval() =>
            _beetleAIConfig.ChasePositionCheckInterval;

        private float GetChaseDistanceCheckInterval() =>
            _beetleAIConfig.ChaseDistanceCheckInterval;

        private float GetChaseEndDelay() =>
            _beetleAIConfig.ChaseEndDelay;

        private float GetMaxChaseDistance() =>
            _beetleAIConfig.MaxChaseDistance;

        private float GetTargetReachDistance() =>
            _beetleAIConfig.AttackDistance;

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnTargetNotFound() => EnterTriggerState();

        private void OnTargetReached(PlayerEntity targetPlayer) => EnterAttackState();

        private void OnChaseEnded() => EnterTriggerState();
    }
}