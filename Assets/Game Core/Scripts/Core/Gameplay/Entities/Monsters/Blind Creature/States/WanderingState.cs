using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Systems.MovementLogics;
using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.Entities.Monsters.BlindCreature.States
{
    public class WanderingState : IEnterState, ITickableState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public WanderingState(BlindCreatureEntity blindCreatureEntity)
        {
            BlindCreatureAIConfigMeta blindCreatureAIConfig = blindCreatureEntity.GetAIConfig();
            
            _blindCreatureEntity = blindCreatureEntity;
            _wanderingConfig = blindCreatureAIConfig.GetWanderingConfig();
            _suspicionSystem = blindCreatureEntity.GetSuspicionSystem();
            _agent = blindCreatureEntity.GetAgent();
            _wanderingMovementLogic = new WanderingMovementLogic(blindCreatureEntity.transform, _agent);
        }
        
        // FIELDS: --------------------------------------------------------------------------------

        private readonly BlindCreatureEntity _blindCreatureEntity;
        private readonly BlindCreatureAIConfigMeta.WanderingConfig _wanderingConfig;
        private readonly SuspicionSystem _suspicionSystem;
        private readonly NavMeshAgent _agent;
        private readonly WanderingMovementLogic _wanderingMovementLogic;

        private Vector3 _targetPoint;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            _suspicionSystem.OnNoiseDetectedEvent += OnNoiseDetected;
            
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
            _suspicionSystem.OnNoiseDetectedEvent -= OnNoiseDetected;
            
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
            _blindCreatureEntity.EnterIdleState();
        
        private void EnterMoveToSuspicionPlaceState() =>
            _blindCreatureEntity.EnterMoveToSuspicionPlaceState();
        
        private float GetWanderingSpeed()
        {
            float minSpeed = _wanderingConfig.MinSpeed;
            float maxSpeed = _wanderingConfig.MaxSpeed;
            float speed = Random.Range(minSpeed, maxSpeed);
            return speed;
        }

        private float GetWanderingMinDistance() =>
            _wanderingConfig.MinDistance;
        
        private float GetWanderingMaxDistance() =>
            _wanderingConfig.MaxDistance;

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnNoiseDetected() => EnterMoveToSuspicionPlaceState();

        private void OnStuck() => EnterIdleState();

        private void OnArrived() => EnterIdleState();
    }
}