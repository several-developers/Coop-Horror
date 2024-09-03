using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Systems.Movement;
using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.Entities.Monsters.EvilClown.States
{
    public class WanderingState : IEnterState, ITickableState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public WanderingState(EvilClownEntity evilClownEntity)
        {
            _evilClownEntity = evilClownEntity;
            _evilClownAIConfig = evilClownEntity.GetEvilClownAIConfig();
            _agent = evilClownEntity.GetAgent();
            _wanderingTimer = evilClownEntity.GetWanderingTimer();
            _wanderingMovementLogic = new WanderingMovementLogic(evilClownEntity.transform, _agent);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly EvilClownEntity _evilClownEntity;
        private readonly EvilClownAIConfigMeta _evilClownAIConfig;
        private readonly NavMeshAgent _agent;
        private readonly WanderingTimer _wanderingTimer;
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
            _wanderingTimer.TryStartTimer();

            if (!_wanderingMovementLogic.TrySetDestinationPoint())
                EnterWanderingState();
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

        private void EnterWanderingState() =>
            _evilClownEntity.EnterWanderingState();

        private float GetWanderingSpeed()
        {
            float minSpeed = _evilClownAIConfig.WanderingMinSpeed;
            float maxSpeed = _evilClownAIConfig.WanderingMaxSpeed;
            float speed = Random.Range(minSpeed, maxSpeed);
            return speed;
        }

        private float GetWanderingMinDistance() =>
            _evilClownAIConfig.WanderingMinDistance;

        private float GetWanderingMaxDistance() =>
            _evilClownAIConfig.WanderingMaxDistance;

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

#warning Stack overflow, добавить задержку между сменой стейтов
        private void OnStuck() => EnterWanderingState();

        private void OnArrived() => EnterWanderingState();
    }
}