using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Systems.Movement;
using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.Entities.Monsters.Beetle.States
{
    public class WanderingState : IEnterState, ITickableState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public WanderingState(BeetleEntity beetleEntity)
        {
            _beetleEntity = beetleEntity;
            _beetleAIConfig = beetleEntity.GetAIConfig();
            _agent = beetleEntity.GetAgent();
            _movementLogic = new WanderingMovementLogic(beetleEntity.transform, _agent);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly BeetleEntity _beetleEntity;
        private readonly BeetleAIConfigMeta _beetleAIConfig;
        private readonly NavMeshAgent _agent;
        private readonly WanderingMovementLogic _movementLogic;

        private Vector3 _targetPoint;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            _movementLogic.OnStuckEvent += OnStuck;
            _movementLogic.OnArrivedEvent += OnArrived;
            _movementLogic.GetWanderingMinDistanceEvent += GetWanderingMinDistance;
            _movementLogic.GetWanderingMaxDistanceEvent += GetWanderingMaxDistance;
            
            EnableAgent();

            if (!_movementLogic.TrySetDestinationPoint())
                EnterIdleState();
        }

        public void Tick() =>
            _movementLogic.Tick();

        public void Exit()
        {
            _movementLogic.OnStuckEvent -= OnStuck;
            _movementLogic.OnArrivedEvent -= OnArrived;
            _movementLogic.GetWanderingMinDistanceEvent -= GetWanderingMinDistance;
            _movementLogic.GetWanderingMaxDistanceEvent -= GetWanderingMaxDistance;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void EnableAgent()
        {
            _agent.enabled = true;
            _agent.speed = GetWanderingSpeed();
        }

        private void EnterIdleState() =>
            _beetleEntity.EnterIdleState();
        
        private float GetWanderingSpeed()
        {
            float minSpeed = _beetleAIConfig.WanderingMinSpeed;
            float maxSpeed = _beetleAIConfig.WanderingMaxSpeed;
            float speed = Random.Range(minSpeed, maxSpeed);
            return speed;
        }

        private float GetWanderingMinDistance() =>
            _beetleAIConfig.WanderingMinDistance;
        
        private float GetWanderingMaxDistance() =>
            _beetleAIConfig.WanderingMaxDistance;

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnStuck() => EnterIdleState();

        private void OnArrived() => EnterIdleState();
    }
}