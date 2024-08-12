using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Systems.MovementLogics;
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
            _wanderingMovementLogic = new WanderingMovementLogic(beetleEntity.transform, _agent);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly BeetleEntity _beetleEntity;
        private readonly BeetleAIConfigMeta _beetleAIConfig;
        private readonly NavMeshAgent _agent;
        private readonly WanderingMovementLogic _wanderingMovementLogic;

        private Vector3 _targetPoint;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            _wanderingMovementLogic.OnStuckEvent += OnStuck;
            _wanderingMovementLogic.OnArrivedEvent += OnArrived;
            _wanderingMovementLogic.GetWanderingMinDistanceEvent += GetWanderingMinDistance;
            _wanderingMovementLogic.GetWanderingMaxDistanceEvent += GetWanderingMaxDistance;
            
            EnableAgent();

            if (!_wanderingMovementLogic.TrySetDestinationPoint())
                EnterIdleState();
        }

        public void Tick() =>
            _wanderingMovementLogic.Tick();

        public void Exit()
        {
            _wanderingMovementLogic.OnStuckEvent -= OnStuck;
            _wanderingMovementLogic.OnArrivedEvent -= OnArrived;
            _wanderingMovementLogic.GetWanderingMinDistanceEvent -= GetWanderingMinDistance;
            _wanderingMovementLogic.GetWanderingMaxDistanceEvent -= GetWanderingMaxDistance;
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