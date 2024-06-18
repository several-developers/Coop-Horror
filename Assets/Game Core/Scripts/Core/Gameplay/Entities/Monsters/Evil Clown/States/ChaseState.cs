using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.EntitiesSystems.CombatLogics;
using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.Entities.Monsters.EvilClown.States
{
    public class ChaseState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ChaseState(EvilClownEntity evilClownEntity)
        {
            _evilClownEntity = evilClownEntity;
            _evilClownAIConfig = evilClownEntity.GetEvilClownAIConfig();
            _agent = evilClownEntity.GetAgent();
            _wanderingTimer = evilClownEntity.GetWanderingTimer();
            _chaseLogic = new ChaseLogic(_evilClownEntity, _agent);
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly EvilClownEntity _evilClownEntity;
        private readonly EvilClownAIConfigMeta _evilClownAIConfig;
        private readonly NavMeshAgent _agent;
        private readonly WanderingTimer _wanderingTimer;
        private readonly ChaseLogic _chaseLogic;

        private Coroutine _chaseCO;
        private Coroutine _distanceCheckCO;
        private Coroutine _chasingEndTimerCO;
        private float _cachedAgentStoppingDistance;
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
            
            EnableAgent();
            _wanderingTimer.StopTimer();
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
            
            ResetAgent();
            _chaseLogic.Stop();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void EnableAgent()
        {
            _cachedAgentStoppingDistance = _agent.stoppingDistance;
            
            _agent.enabled = true;
            _agent.speed = _evilClownAIConfig.ChaseSpeed;
            _agent.stoppingDistance = _evilClownAIConfig.ChaseStoppingDistance;
        }

        private void ResetAgent() =>
            _agent.stoppingDistance = _cachedAgentStoppingDistance;

        private void EnterAttackState() =>
            _evilClownEntity.EnterAttackState();
        
        private void EnterWanderingState() =>
            _evilClownEntity.EnterWanderingState();

        private PlayerEntity GetTargetPlayer() =>
            _evilClownEntity.GetTargetPlayer();

        private float GetChasePositionCheckInterval() =>
            _evilClownAIConfig.ChasePositionCheckInterval;

        private float GetChaseDistanceCheckInterval() =>
            _evilClownAIConfig.ChaseDistanceCheckInterval;

        private float GetChaseEndDelay() =>
            _evilClownAIConfig.ChaseEndDelay;

        private float GetMaxChaseDistance() =>
            _evilClownAIConfig.MaxChaseDistance;

        private float GetTargetReachDistance() =>
            _evilClownAIConfig.AttackDistance;

        // EVENTS RECEIVERS: ----------------------------------------------------------------------
        
        private void OnTargetNotFound() => EnterWanderingState();

        private void OnTargetReached(PlayerEntity targetPlayer) => EnterAttackState();

        private void OnChaseEnded() => EnterWanderingState();
    }
}