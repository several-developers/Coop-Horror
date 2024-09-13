using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Systems.Movement;
using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.Entities.Monsters.Mushroom.States
{
    public class MoveToInterestTargetState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        public MoveToInterestTargetState(MushroomEntity mushroomEntity)
        {
            MushroomAIConfigMeta mushroomAIConfig = mushroomEntity.GetAIConfig();
            
            _mushroomEntity = mushroomEntity;
            _moveToInterestTargetConfig = mushroomAIConfig.GetMoveToInterestTargetConfig();
            _whisperingSystem = mushroomEntity.GetWhisperingSystem();
            _agent = mushroomEntity.GetAgent();
            _chaseLogic = new ChaseLogic(mushroomEntity, _agent);
        }
        
        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly MushroomEntity _mushroomEntity;
        private readonly MushroomAIConfigMeta.MoveToInterestTargetConfig _moveToInterestTargetConfig;
        private readonly WhisperingSystem _whisperingSystem;
        private readonly NavMeshAgent _agent;
        private readonly ChaseLogic _chaseLogic;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            _chaseLogic.OnTargetNotFoundEvent += OnTargetNotFound;
            _chaseLogic.OnTargetReachedEvent += OnTargetReached;
            _chaseLogic.OnChaseEndedEvent += OnChaseEnded;
            _chaseLogic.GetTargetPlayerEvent += GetTargetPlayer;
            _chaseLogic.GetChasePositionCheckIntervalEvent += GetChasePositionCheckInterval;
            _chaseLogic.GetChaseDistanceCheckIntervalEvent += GetChaseDistanceCheckInterval;
            _chaseLogic.GetTargetReachDistanceEvent += GetTargetReachDistance;
            
            EnableAgent();
            SetSneakingState(isSneaking: true);
            PauseWhisperingSystem();
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
            _chaseLogic.GetTargetReachDistanceEvent -= GetTargetReachDistance;
            
            _chaseLogic.Stop();
            SetSneakingState(isSneaking: false);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void EnableAgent()
        {
            Vector2 moveSpeed = _moveToInterestTargetConfig.MoveSpeed;
            float speed = Random.Range(moveSpeed.x, moveSpeed.y);
            
            _agent.enabled = true;
            _agent.speed = speed;
        }

        private void SetSneakingState(bool isSneaking) =>
            _mushroomEntity.SetSneakingState(isSneaking);
        
        private void PauseWhisperingSystem() =>
            _whisperingSystem.Pause();

        private void EnterIdleState() =>
            _mushroomEntity.EnterIdleState();

        private void EnterLookAtInterestTargetState() =>
            _mushroomEntity.EnterLookAtInterestTargetState();

        private PlayerEntity GetTargetPlayer() =>
            _mushroomEntity.GetInterestTarget();

        private float GetChasePositionCheckInterval() =>
            _moveToInterestTargetConfig.PositionCheckInterval;

        private float GetChaseDistanceCheckInterval() =>
            _moveToInterestTargetConfig.DistanceCheckInterval;

        private float GetTargetReachDistance() =>
            _moveToInterestTargetConfig.TargetReachDistance;

        // EVENTS RECEIVERS: ----------------------------------------------------------------------
        
        private void OnTargetNotFound() => EnterIdleState();

        private void OnTargetReached(PlayerEntity targetPlayer) => EnterLookAtInterestTargetState();

        private void OnChaseEnded() => EnterLookAtInterestTargetState();
    }
}