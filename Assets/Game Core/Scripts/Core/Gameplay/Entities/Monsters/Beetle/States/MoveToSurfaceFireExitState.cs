﻿using GameCore.Infrastructure.Configs.Gameplay.Enemies;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Systems.Movement;
using GameCore.Gameplay.Level;
using UnityEngine.AI;

namespace GameCore.Gameplay.Entities.Monsters.Beetle.States
{
    public class MoveToSurfaceFireExitState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public MoveToSurfaceFireExitState(BeetleEntity beetleEntity, ILevelProvider levelProvider)
        {
            _beetleEntity = beetleEntity;
            _beetleAIConfig = beetleEntity.GetAIConfig();
            _agent = beetleEntity.GetAgent();
            _movementLogic = new MoveFromSurfaceToStairsLogic(beetleEntity, _agent, levelProvider);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly BeetleEntity _beetleEntity;
        private readonly BeetleAIConfigMeta _beetleAIConfig;
        private readonly NavMeshAgent _agent;
        private readonly MoveFromSurfaceToStairsLogic _movementLogic;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            _movementLogic.OnFireExitNotFoundEvent += OnFireExitNotFound;
            _movementLogic.OnInteractWithFireExitEvent += OnInteractWithFireExit;
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
            _beetleEntity.SetEntityLocation(EntityLocation.Stairs);
            DecideStateByLocation();
        }
    }
}