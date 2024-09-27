using GameCore.Infrastructure.Configs.Gameplay.Enemies;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Systems.Movement;
using GameCore.Gameplay.Level;
using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.Entities.Monsters.GoodClown.States
{
    public class HuntingChaseState : IEnterState, ITickableState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        public HuntingChaseState(GoodClownEntity goodClownEntity, ILevelProvider levelProvider)
        {
            _goodClownEntity = goodClownEntity;
            _clownUtilities = goodClownEntity.GetClownUtilities();
            _agent = goodClownEntity.GetAgent();
            _chaseLogic = new ClownChaseLogic(goodClownEntity, _agent, levelProvider);

            GoodClownAIConfigMeta goodClownAIConfig = goodClownEntity.GetGoodClownAIConfig();
            _huntingChaseConfig = goodClownAIConfig.HuntingChaseConfig;
            _commonConfig = goodClownAIConfig.CommonConfig;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly GoodClownEntity _goodClownEntity;
        private readonly GoodClownAIConfigMeta.HuntingChaseSettings _huntingChaseConfig;
        private readonly GoodClownAIConfigMeta.CommonSettings _commonConfig;
        private readonly GoodClownUtilities _clownUtilities;
        private readonly NavMeshAgent _agent;
        private readonly ClownChaseLogic _chaseLogic;
        
        private float _cachedStoppingDistance;

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
            _chaseLogic.Start();
        }

        public void Tick() =>
            _clownUtilities.UpdateAnimationMoveSpeed();

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
            _cachedStoppingDistance = _agent.stoppingDistance;
            
            _agent.enabled = true;
            _agent.speed = _huntingChaseConfig.MoveSpeed;
            _agent.stoppingDistance = _huntingChaseConfig.StoppingDistance;
        }

        private void ResetAgent() =>
            _agent.stoppingDistance = _cachedStoppingDistance;

        private void EnterSearchForTargetState() =>
            _goodClownEntity.EnterSearchForTargetState();

        private void EnterHuntingIdleState() =>
            _goodClownEntity.EnterHuntingIdleState();

        private PlayerEntity GetTargetPlayer() =>
            _goodClownEntity.GetTargetPlayer();
        
        private EntityLocation GetClownLocation() =>
            _goodClownEntity.GetCurrentLocation();

        private Floor GetClownFloor() =>
            _goodClownEntity.GetCurrentFloor();

        private float GetChasePositionCheckInterval() =>
            _huntingChaseConfig.PositionCheckInterval;

        private float GetChaseDistanceCheckInterval() =>
            _huntingChaseConfig.DistanceCheckInterval;

        private float GetChaseEndDelay() =>
            _huntingChaseConfig.ChaseEndDelay;

        private float GetMaxChaseDistance() =>
            _huntingChaseConfig.MaxChaseDistance;

        private float GetTargetReachDistance() =>
            _huntingChaseConfig.ReachDistance;
        
        private float GetFireExitInteractionDistance() =>
            _commonConfig.FireExitInteractionDistance;

        private float GetStoppingDistance() =>
            _huntingChaseConfig.StoppingDistance;

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnTargetNotFound() => EnterSearchForTargetState();

        private void OnTargetReached(PlayerEntity targetPlayer) => EnterHuntingIdleState();

        private void OnChaseEnded() => EnterSearchForTargetState();
    }
}