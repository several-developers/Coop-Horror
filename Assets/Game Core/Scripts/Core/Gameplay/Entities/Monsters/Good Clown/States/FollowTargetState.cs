using GameCore.Configs.Gameplay.Enemies;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.EntitiesSystems.MovementLogics;
using GameCore.Gameplay.Level;
using UnityEngine.AI;

namespace GameCore.Gameplay.Entities.Monsters.GoodClown.States
{
    public class FollowTargetState : IEnterState, ITickableState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public FollowTargetState(GoodClownEntity goodClownEntity, ILevelProvider levelProvider)
        {
            _goodClownEntity = goodClownEntity;
            _agent = goodClownEntity.GetAgent();
            _clownUtilities = goodClownEntity.GetClownUtilities();
            _chaseLogic = new ClownChaseLogic(goodClownEntity, _agent, levelProvider);

            GoodClownAIConfigMeta goodClownAIConfig = goodClownEntity.GetGoodClownAIConfig();
            _followTargetConfig = goodClownAIConfig.FollowTargetConfig;
            _commonConfig = goodClownAIConfig.CommonConfig;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly GoodClownEntity _goodClownEntity;
        private readonly GoodClownAIConfigMeta.FollowTargetSettings _followTargetConfig;
        private readonly GoodClownAIConfigMeta.CommonSettings _commonConfig;
        private readonly NavMeshAgent _agent;
        private readonly GoodClownUtilities _clownUtilities;
        private readonly ClownChaseLogic _chaseLogic;

        private float _cachedAgentStoppingDistance;

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
            _cachedAgentStoppingDistance = _agent.stoppingDistance;

            _agent.enabled = true;
            _agent.speed = _followTargetConfig.MoveSpeed;
        }

        private void ResetAgent() =>
            _agent.stoppingDistance = _cachedAgentStoppingDistance;

        private void EnterSearchForTargetState() =>
            _goodClownEntity.EnterSearchForTargetState();

        private void EnterWanderingAroundTargetState() =>
            _goodClownEntity.EnterWanderingAroundTargetState();

        private PlayerEntity GetTargetPlayer() =>
            _goodClownEntity.GetTargetPlayer();

        private EntityLocation GetClownLocation() =>
            _goodClownEntity.EntityLocation;

        private Floor GetClownFloor() =>
            _goodClownEntity.CurrentFloor;
        
        private float GetChasePositionCheckInterval() =>
            _followTargetConfig.PositionCheckInterval;

        private float GetChaseDistanceCheckInterval() =>
            _followTargetConfig.DistanceCheckInterval;

        private float GetChaseEndDelay() =>
            _followTargetConfig.FollowEndDelay;

        private float GetMaxChaseDistance() =>
            _followTargetConfig.MaxFollowDistance;

        private float GetTargetReachDistance() =>
            _followTargetConfig.ReachDistance;

        private float GetFireExitInteractionDistance() =>
            _commonConfig.FireExitInteractionDistance;

        private float GetStoppingDistance() =>
            _followTargetConfig.StoppingDistance;

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnTargetNotFound() => EnterSearchForTargetState();

        private void OnTargetReached(PlayerEntity targetPlayer) => EnterWanderingAroundTargetState();

        private void OnChaseEnded() => EnterSearchForTargetState(); // Questionable
    }
}