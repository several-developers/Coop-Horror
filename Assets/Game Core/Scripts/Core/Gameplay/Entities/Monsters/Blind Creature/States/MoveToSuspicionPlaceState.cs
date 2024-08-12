using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Systems.MovementLogics;
using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.Entities.Monsters.BlindCreature.States
{
    public class MoveToSuspicionPlaceState : IEnterState, ITickableState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public MoveToSuspicionPlaceState(BlindCreatureEntity blindCreatureEntity)
        {
            _blindCreatureEntity = blindCreatureEntity;
            _blindCreatureAIConfig = blindCreatureEntity.GetAIConfig();
            _suspicionSystem = blindCreatureEntity.GetSuspicionSystem();
            _agent = blindCreatureEntity.GetAgent();

            Transform transform = blindCreatureEntity.transform;
            _movementLogic = new FollowPositionMovementLogic(transform, _agent);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly BlindCreatureEntity _blindCreatureEntity;
        private readonly BlindCreatureAIConfigMeta _blindCreatureAIConfig;
        private readonly SuspicionSystem _suspicionSystem;
        private readonly NavMeshAgent _agent;
        private readonly FollowPositionMovementLogic _movementLogic;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            _suspicionSystem.OnNoiseDetectedEvent += OnNoiseDetected;

            _movementLogic.OnArrivedEvent += OnArrived;
            _movementLogic.OnStuckEvent += OnStuck;
            _movementLogic.GetTargetPositionEvent += GetTargetPosition;

            EnableAgent();

            if (!TryUpdateTargetPoint())
                EnterIdleState();
        }

        public void Tick() =>
            _movementLogic.Tick();

        public void Exit()
        {
            _suspicionSystem.OnNoiseDetectedEvent -= OnNoiseDetected;
            
            _movementLogic.OnArrivedEvent -= OnArrived;
            _movementLogic.OnStuckEvent -= OnStuck;
            _movementLogic.GetTargetPositionEvent -= GetTargetPosition;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void EnableAgent()
        {
            _agent.enabled = true;
            _agent.speed = _blindCreatureAIConfig.SuspicionMoveSpeed;
        }

        private void EnterIdleState() =>
            _blindCreatureEntity.EnterIdleState();

        private void EnterLookAroundSuspicionPlaceState() =>
            _blindCreatureEntity.EnterLookAroundSuspicionPlaceState();

        private Vector3 GetTargetPosition() =>
            _suspicionSystem.GetLastNoisePosition();

        private bool TryUpdateTargetPoint() =>
            _movementLogic.TryUpdateTargetPoint();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnNoiseDetected() => TryUpdateTargetPoint();

        private void OnArrived() => EnterLookAroundSuspicionPlaceState();

        private void OnStuck() => EnterLookAroundSuspicionPlaceState();
    }
}