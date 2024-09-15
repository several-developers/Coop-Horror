using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Systems.Movement;
using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.Entities.Monsters.SpikySlime.States
{
    public class WanderingState : IEnterState, ITickableState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public WanderingState(SpikySlimeEntity spikySlimeEntity)
        {
            SpikySlimeAIConfigMeta spikySlimeAIConfig = spikySlimeEntity.GetAIConfig();
            
            _spikySlimeEntity = spikySlimeEntity;
            _wanderingConfig = spikySlimeAIConfig.GetWanderingConfig();
            _agent = spikySlimeEntity.GetAgent();
            _movementLogic = new WanderingMovementLogic(spikySlimeEntity.transform, _agent);
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly SpikySlimeEntity _spikySlimeEntity;
        private readonly SpikySlimeAIConfigMeta.WanderingConfig _wanderingConfig;
        private readonly NavMeshAgent _agent;
        private readonly WanderingMovementLogic _movementLogic;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            _movementLogic.OnStuckEvent += OnStuck;
            _movementLogic.OnArrivedEvent += OnArrived;
            _movementLogic.GetWanderingMinDistanceEvent += GetWanderingMinDistance;
            _movementLogic.GetWanderingMaxDistanceEvent += GetWanderingMaxDistance;

            _spikySlimeEntity.PlaySound(SpikySlimeEntity.SFXType.CalmMovement).Forget();
            _spikySlimeEntity.EnableAgent();
            UpdateMovementSpeed();
            SetDestinationPoint();
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

        private void SetDestinationPoint() =>
            _movementLogic.TrySetDestinationPoint();

        private void UpdateMovementSpeed() => 
            _agent.speed = GetWanderingSpeed();

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

        private void OnStuck()
        {
            UpdateMovementSpeed();
            SetDestinationPoint();
        }

        private void OnArrived()
        {
            UpdateMovementSpeed();
            SetDestinationPoint();
        }
    }
}