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
            BlindCreatureAIConfigMeta blindCreatureAIConfig = blindCreatureEntity.GetAIConfig();
            BlindCreatureEntity.References references = blindCreatureEntity.GetReferences();
            
            _blindCreatureEntity = blindCreatureEntity;
            _suspicionStateConfig = blindCreatureAIConfig.GetSuspicionStateConfig();
            _suspicionSystem = blindCreatureEntity.GetSuspicionSystem();
            _agent = blindCreatureEntity.GetAgent();

            Transform transform = blindCreatureEntity.transform;
            _movementLogic = new FollowPositionMovementLogic(transform, _agent);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly BlindCreatureEntity _blindCreatureEntity;
        private readonly BlindCreatureAIConfigMeta.SuspicionStateConfig _suspicionStateConfig;
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
            _agent.speed = _suspicionStateConfig.SuspicionMoveSpeed;
        }

        private void EnterIdleState() =>
            _blindCreatureEntity.EnterIdleState();
        
        private void EnterLookAroundSuspicionPlaceState() =>
            _blindCreatureEntity.EnterLookAroundSuspicionPlaceState();

        private void EnterAttackSuspicionPlaceState() =>
            _blindCreatureEntity.EnterAttackSuspicionPlaceState();

        private Vector3 GetTargetPosition() =>
            _suspicionSystem.GetLastNoisePosition();

        private bool TryUpdateTargetPoint() =>
            _movementLogic.TryUpdateTargetPoint();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnNoiseDetected() => TryUpdateTargetPoint();

        private void OnArrived()
        {
            bool isAggressive = _suspicionSystem.IsAggressive();
            
            if (isAggressive)
                EnterAttackSuspicionPlaceState();
            else
                EnterLookAroundSuspicionPlaceState();
        }

        private void OnStuck()
        {
            bool isAggressive = _suspicionSystem.IsAggressive();
            
            if (isAggressive)
                EnterAttackSuspicionPlaceState();
            else
                EnterLookAroundSuspicionPlaceState();
        }
    }
}