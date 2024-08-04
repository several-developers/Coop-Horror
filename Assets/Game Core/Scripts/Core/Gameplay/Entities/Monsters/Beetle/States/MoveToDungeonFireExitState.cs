using GameCore.Configs.Gameplay.Enemies;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.EntitiesSystems.MovementLogics;
using GameCore.Gameplay.Level;
using UnityEngine.AI;

namespace GameCore.Gameplay.Entities.Monsters.Beetle.States
{
    public class MoveToDungeonFireExitState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public MoveToDungeonFireExitState(BeetleEntity beetleEntity, ILevelProvider levelProvider)
        {
            _beetleEntity = beetleEntity;
            _beetleAIConfig = beetleEntity.GetAIConfig();
            _agent = beetleEntity.GetAgent();
            _movementLogic = new MoveFromStairsToDungeonLogic(beetleEntity, _agent, levelProvider);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly BeetleEntity _beetleEntity;
        private readonly BeetleAIConfigMeta _beetleAIConfig;
        private readonly NavMeshAgent _agent;
        private readonly MoveFromStairsToDungeonLogic _movementLogic;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            _movementLogic.OnFireExitNotFoundEvent += OnFireExitNotFound;
            _movementLogic.OnInteractWithFireExitEvent += OnInteractWithFireExit;
            _movementLogic.GetDungeonFloorEvent += GetDungeonFloor;
            _movementLogic.GetFireExitDistanceCheckIntervalEvent += GetFireExitDistanceCheckInterval;
            _movementLogic.GetFireExitInteractionDistanceEvent += GetFireExitInteractionDistance;
            
            EnableAgent();
            _movementLogic.Start();
            
            _beetleEntity.OnEntityTeleportedEvent += OnEntityTeleported;
        }

        public void Exit()
        {
            _movementLogic.OnFireExitNotFoundEvent -= OnFireExitNotFound;
            _movementLogic.OnInteractWithFireExitEvent -= OnInteractWithFireExit;
            _movementLogic.GetDungeonFloorEvent -= GetDungeonFloor;
            _movementLogic.GetFireExitDistanceCheckIntervalEvent -= GetFireExitDistanceCheckInterval;
            _movementLogic.GetFireExitInteractionDistanceEvent -= GetFireExitInteractionDistance;
            
            _movementLogic.Stop();
            
            _beetleEntity.OnEntityTeleportedEvent -= OnEntityTeleported;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void EnableAgent()
        {
            _agent.enabled = true;
            _agent.speed = _beetleAIConfig.MoveToDungeonSpeed;
        }

        private void DecideStateByLocation() =>
            _beetleEntity.DecideStateByLocation();

        private void EnterIdleState() =>
            _beetleEntity.EnterIdleState();

        private Floor GetDungeonFloor() =>
            _beetleEntity.CurrentFloor;

        private float GetFireExitDistanceCheckInterval() =>
            _beetleAIConfig.FireExitDistanceCheckInterval;

        private float GetFireExitInteractionDistance() =>
            _beetleAIConfig.FireExitInteractionDistance;

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnFireExitNotFound() => EnterIdleState();

        private void OnInteractWithFireExit(FireExit fireExit) =>
            fireExit.Interact(_beetleEntity);

        private void OnEntityTeleported()
        {
            _beetleEntity.SetEntityLocation(EntityLocation.Dungeon);
            DecideStateByLocation();
        }
    }
}