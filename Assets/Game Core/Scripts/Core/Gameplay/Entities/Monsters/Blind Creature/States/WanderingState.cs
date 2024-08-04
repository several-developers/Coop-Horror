using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.EntitiesSystems.MovementLogics;
using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.Entities.Monsters.BlindCreature.States
{
    public class WanderingState : IEnterState, ITickableState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public WanderingState(BlindCreatureEntity blindCreatureEntity)
        {
            _blindCreatureEntity = blindCreatureEntity;
            _blindCreatureAIConfig = blindCreatureEntity.GetAIConfig();
            _agent = blindCreatureEntity.GetAgent();
            _wanderingMovementLogic = new WanderingMovementLogic(blindCreatureEntity.transform, _agent);
        }
        
        // FIELDS: --------------------------------------------------------------------------------

        private readonly BlindCreatureEntity _blindCreatureEntity;
        private readonly BlindCreatureAIConfigMeta _blindCreatureAIConfig;
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
            _blindCreatureEntity.EnterIdleState();
        
        private float GetWanderingSpeed()
        {
            float minSpeed = _blindCreatureAIConfig.WanderingMinSpeed;
            float maxSpeed = _blindCreatureAIConfig.WanderingMaxSpeed;
            float speed = Random.Range(minSpeed, maxSpeed);
            return speed;
        }

        private float GetWanderingMinDistance() =>
            _blindCreatureAIConfig.WanderingMinDistance;
        
        private float GetWanderingMaxDistance() =>
            _blindCreatureAIConfig.WanderingMaxDistance;

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnStuck() => EnterIdleState();

        private void OnArrived() => EnterIdleState();
    }
}