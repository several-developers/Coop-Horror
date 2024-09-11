using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Systems.Movement;
using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.Entities.Monsters.Mushroom.States
{
    public class WanderingState : IEnterState, ITickableState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public WanderingState(MushroomEntity mushroomEntity)
        {
            MushroomAIConfigMeta mushroomAIConfig = mushroomEntity.GetAIConfig(); 
            
            _mushroomEntity = mushroomEntity;
            _wanderingConfig = mushroomAIConfig.GetWanderingConfig();
            _agent = mushroomEntity.GetAgent();
            _wanderingMovementLogic = new WanderingMovementLogic(mushroomEntity.transform, _agent);
        }
        
        // FIELDS: --------------------------------------------------------------------------------

        private readonly MushroomEntity _mushroomEntity;
        private readonly MushroomAIConfigMeta.WanderingConfig _wanderingConfig;
        private readonly NavMeshAgent _agent;
        private readonly WanderingMovementLogic _wanderingMovementLogic;

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
            _agent.acceleration = _wanderingConfig.Acceleration;
        }

        private void EnterIdleState() =>
            _mushroomEntity.EnterIdleState();

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

        private void OnStuck() => EnterIdleState();

        private void OnArrived() => EnterIdleState();
    }
}