using GameCore.Infrastructure.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Systems.Movement;
using GameCore.Gameplay.Systems.Utilities;
using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.Entities.Monsters.Mushroom.States
{
    public class RunawayState : IEnterState, ITickableState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public RunawayState(MushroomEntity mushroomEntity)
        {
            MushroomAIConfigMeta mushroomAIConfig = mushroomEntity.GetAIConfig();
            
            _mushroomEntity = mushroomEntity;
            _commonConfig = mushroomAIConfig.GetCommonConfig();
            _whisperingSystem = mushroomEntity.GetWhisperingSystem();
            _agent = mushroomEntity.GetAgent();
            _transform = mushroomEntity.transform;
            _movementLogic = new FollowPositionMovementLogic(_transform, _agent);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly MushroomEntity _mushroomEntity;
        private readonly MushroomAIConfigMeta.CommonConfig _commonConfig;
        private readonly WhisperingSystem _whisperingSystem;
        private readonly NavMeshAgent _agent;
        private readonly Transform _transform;
        private readonly FollowPositionMovementLogic _movementLogic;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            _movementLogic.OnArrivedEvent += OnArrived;
            _movementLogic.OnStuckEvent += OnStuck;
            _movementLogic.GetTargetPositionEvent += GetTargetPosition;

            _mushroomEntity.SetEmotion(MushroomEntity.Emotion.Scared);
            EnableAgent();
            SetSprintingState(isSprinting: true);
            PauseWhisperingSystem();
            TryUpdateTargetPoint();
        }

        public void Tick() =>
            _movementLogic.Tick();

        public void Exit()
        {
            _movementLogic.OnArrivedEvent -= OnArrived;
            _movementLogic.OnStuckEvent -= OnStuck;
            _movementLogic.GetTargetPositionEvent -= GetTargetPosition;
            
            SetSprintingState(isSprinting: false);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void EnableAgent()
        {
            _agent.enabled = true;
            _agent.speed = _commonConfig.RunawaySpeed;
        }

        private void SetSprintingState(bool isSprinting)
        {
            MushroomReferences references = _mushroomEntity.GetReferences();
            Animator animator = references.Animator;
            float value = isSprinting ? 1f : 0f;
            
            animator.SetFloat(id: AnimatorHashes.IsSprinting, value);
        }

        private void PauseWhisperingSystem() =>
            _whisperingSystem.Pause();
        
        private void EnterHidingState() =>
            _mushroomEntity.EnterHidingState();

        private Vector3 GetTargetPosition()
        {
            Vector3 currentPosition = _transform.position;

            bool isPlayerFound =
                MonstersUtilities.TryGetClosestAlivePlayer(currentPosition, out PlayerEntity playerEntity);

            if (!isPlayerFound)
            {
                Debug.LogWarning("Players not found");
                return currentPosition;
            }

            Vector3 playerPosition = playerEntity.transform.position;
            Vector3 runawayDirection = (currentPosition - playerPosition).normalized;
            Vector3 targetPosition = currentPosition + runawayDirection * 10f;
            return targetPosition;
        }

        private bool TryUpdateTargetPoint()
        {
            var suca = _movementLogic.TryUpdateTargetPoint();

            if (!suca)
                Debug.LogWarning("Ooopsie");

            return suca;
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnArrived() => TryUpdateTargetPoint();

        private void OnStuck() => TryUpdateTargetPoint();
    }
}