using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Systems.Movement;
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

            _blindCreatureEntity = blindCreatureEntity;
            _suspicionStateConfig = blindCreatureAIConfig.GetSuspicionStateConfig();
            _suspicionSystem = blindCreatureEntity.GetSuspicionSystem();
            _combatSystem = blindCreatureEntity.GetCombatSystem();
            _agent = blindCreatureEntity.GetAgent();

            Transform transform = blindCreatureEntity.transform;
            _movementLogic = new FollowPositionMovementLogic(transform, _agent);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly BlindCreatureEntity _blindCreatureEntity;
        private readonly BlindCreatureAIConfigMeta.SuspicionStateConfig _suspicionStateConfig;
        private readonly SuspicionSystem _suspicionSystem;
        private readonly CombatSystem _combatSystem;
        private readonly NavMeshAgent _agent;
        private readonly FollowPositionMovementLogic _movementLogic;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            _suspicionSystem.OnNoiseDetectedEvent += OnNoiseDetected;

            _combatSystem.OnAttackPerformedEvent += OnAttackPerformed;

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
            
            _combatSystem.OnAttackPerformedEvent -= OnAttackPerformed;

            _movementLogic.OnArrivedEvent -= OnArrived;
            _movementLogic.OnStuckEvent -= OnStuck;
            _movementLogic.GetTargetPositionEvent -= GetTargetPosition;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void EnableAgent()
        {
            _agent.enabled = true;
            _agent.speed = _suspicionStateConfig.SuspicionMoveSpeed;
            _agent.acceleration = _suspicionStateConfig.SuspicionAcceleration;
        }

        private void DecideStateAfterArriving()
        {
            bool isAggressive = _suspicionSystem.IsAggressive();
            bool isAttackOnCooldown = _combatSystem.IsAttackOnCooldown();
            bool canAttack = isAggressive && !isAttackOnCooldown;

            if (canAttack)
                EnterAttackSuspicionPlaceState();
            else
                EnterLookAroundSuspicionPlaceState();
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

        private void OnAttackPerformed() => EnterLookAroundSuspicionPlaceState();

        private void OnArrived() => DecideStateAfterArriving();

        private void OnStuck() => DecideStateAfterArriving();
    }
}