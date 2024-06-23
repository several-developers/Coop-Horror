using GameCore.Configs.Gameplay.Enemies;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.EntitiesSystems.MovementLogics;
using GameCore.Gameplay.Level;
using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.Entities.Monsters.EvilClown.States
{
    public class ChaseState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ChaseState(EvilClownEntity evilClownEntity, ILevelProvider levelProvider)
        {
            _evilClownEntity = evilClownEntity;
            _evilClownAIConfig = evilClownEntity.GetEvilClownAIConfig();
            _agent = evilClownEntity.GetAgent();
            _wanderingTimer = evilClownEntity.GetWanderingTimer();
            _chaseLogic = new ClownChaseLogic(_evilClownEntity, _agent, levelProvider);
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly EvilClownEntity _evilClownEntity;
        private readonly EvilClownAIConfigMeta _evilClownAIConfig;
        private readonly NavMeshAgent _agent;
        private readonly WanderingTimer _wanderingTimer;
        private readonly ClownChaseLogic _chaseLogic;

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
            
            _chaseLogic.GetClownLocationEvent += GetClownLocation;
            _chaseLogic.GetClownFloorEvent += GetClownFloor;
            _chaseLogic.GetFireExitInteractionDistanceEvent += GetFireExitInteractionDistance;
            _chaseLogic.GetStoppingDistanceEvent += GetStoppingDistance;
            
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
            
            _chaseLogic.GetClownLocationEvent -= GetClownLocation;
            _chaseLogic.GetClownFloorEvent -= GetClownFloor;
            _chaseLogic.GetFireExitInteractionDistanceEvent -= GetFireExitInteractionDistance;
            _chaseLogic.GetStoppingDistanceEvent -= GetStoppingDistance;
            
            _chaseLogic.Stop();
            ResetAgent();
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
        
        private EntityLocation GetClownLocation() =>
            _evilClownEntity.EntityLocation;

        private Floor GetClownFloor() =>
            _evilClownEntity.CurrentFloor;

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
        
        private float GetFireExitInteractionDistance() =>
            _evilClownAIConfig.FireExitInteractionDistance;

        private float GetStoppingDistance() =>
            _evilClownAIConfig.ChaseStoppingDistance;

        // EVENTS RECEIVERS: ----------------------------------------------------------------------
        
        private void OnTargetNotFound() => EnterWanderingState();

        private void OnTargetReached(PlayerEntity targetPlayer) => EnterAttackState();

        private void OnChaseEnded() => EnterWanderingState();
    }
}