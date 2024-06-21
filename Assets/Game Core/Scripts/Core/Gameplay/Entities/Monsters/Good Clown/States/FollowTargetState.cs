using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.EntitiesSystems.CombatLogics;
using UnityEngine.AI;

namespace GameCore.Gameplay.Entities.Monsters.GoodClown.States
{
    public class FollowTargetState : IEnterState, ITickableState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public FollowTargetState(GoodClownEntity goodClownEntity)
        {
            _goodClownEntity = goodClownEntity;
            _agent = goodClownEntity.GetAgent();
            _clownUtilities = goodClownEntity.GetClownUtilities();
            _chaseLogic = new ChaseLogic(goodClownEntity, _agent);

            GoodClownAIConfigMeta goodClownAIConfig = goodClownEntity.GetGoodClownAIConfig();
            _followTargetConfig = goodClownAIConfig.FollowTargetConfig;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly GoodClownEntity _goodClownEntity;
        private readonly GoodClownAIConfigMeta.FollowTargetSettings _followTargetConfig;
        private readonly NavMeshAgent _agent;
        private readonly GoodClownUtilities _clownUtilities;
        private readonly ChaseLogic _chaseLogic;

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
            
            EnableAgent();
            SetWalkingAnimation();
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
            
            ResetAgent();
            _chaseLogic.Stop();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void EnableAgent()
        {
            _cachedAgentStoppingDistance = _agent.stoppingDistance;
            
            _agent.enabled = true;
            _agent.speed = _followTargetConfig.MoveSpeed;
            _agent.stoppingDistance = _followTargetConfig.StoppingDistance;
        }

        private void ResetAgent() =>
            _agent.stoppingDistance = _cachedAgentStoppingDistance;

        private void SetWalkingAnimation() =>
            _clownUtilities.SetWalkingAnimation();

        private void EnterSearchForTargetState() => 
            _goodClownEntity.EnterSearchForTargetState();

        private void EnterWanderingAroundTargetState() =>
            _goodClownEntity.EnterWanderingAroundTargetState();

        private PlayerEntity GetTargetPlayer() =>
            _goodClownEntity.GetTargetPlayer();

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

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnTargetNotFound() => EnterSearchForTargetState();

        private void OnTargetReached(PlayerEntity targetPlayer) => EnterWanderingAroundTargetState();

        private void OnChaseEnded() => EnterSearchForTargetState(); // Questionable
    }
}