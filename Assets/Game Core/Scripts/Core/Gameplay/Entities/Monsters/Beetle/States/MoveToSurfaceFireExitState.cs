using System.Collections;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Level;
using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.Entities.Monsters.Beetle.States
{
    public class MoveToSurfaceFireExitState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public MoveToSurfaceFireExitState(BeetleEntity beetleEntity, BeetleAIConfigMeta beetleAIConfig,
            ILevelProvider levelProvider)
        {
            _beetleEntity = beetleEntity;
            _beetleAIConfig = beetleAIConfig;
            _transform = beetleEntity.transform;
            _agent = beetleEntity.GetAgent();
            
            _levelProvider = levelProvider;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly BeetleEntity _beetleEntity;
        private readonly BeetleAIConfigMeta _beetleAIConfig;
        private readonly Transform _transform;
        private readonly NavMeshAgent _agent;

        private readonly ILevelProvider _levelProvider;

        private FireExit _fireExit;
        private Coroutine _distanceCheckCO;
        private Vector3 _destinationPoint;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            EnableAgent();
            StartDistanceCheck();
            SetDestinationPoint();

            _beetleEntity.OnEntityTeleportedEvent += OnEntityTeleported;
        }

        public void Exit()
        {
            StopDistanceCheck();
            
            _beetleEntity.OnEntityTeleportedEvent -= OnEntityTeleported;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void EnableAgent()
        {
            _agent.enabled = true;
            _agent.speed = _beetleAIConfig.MoveToDungeonSpeed;
        }

        private void SetDestinationPoint()
        {
            bool isFireExitFound = _levelProvider.TryGetOtherFireExit(Floor.Surface, out _fireExit);

            if (!isFireExitFound)
            {
                EnterIdleState();
                return;
            }

            Transform teleportPoint = _fireExit.GetTeleportPoint();
            _destinationPoint = teleportPoint.position;
            _agent.destination = _destinationPoint;
        }
        
        private void CheckDistanceToFireExit()
        {
            Vector3 beetlePosition = _transform.position;
            float distance = Vector3.Distance(a: beetlePosition, b: _destinationPoint);
            bool canInteract = distance <= _beetleAIConfig.FireExitInteractionDistance;

            if (!canInteract)
                return;
            
            _fireExit.Interact(_beetleEntity);
        }

        private void StartDistanceCheck()
        {
            IEnumerator routine = DistanceCheckCO();
            _distanceCheckCO = _beetleEntity.StartCoroutine(routine);
        }

        private void StopDistanceCheck()
        {
            if (_distanceCheckCO == null)
                return;
            
            _beetleEntity.StopCoroutine(_distanceCheckCO);
        }

        private IEnumerator DistanceCheckCO()
        {
            while (true)
            {
                float checkInterval = _beetleAIConfig.FireExitDistanceCheckInterval;
                yield return new WaitForSeconds(checkInterval);
                
                CheckDistanceToFireExit();
            }
        }

        private void EnterIdleState() =>
            _beetleEntity.EnterIdleState();

        private void EnterMoveToDungeonFireExitState() =>
            _beetleEntity.EnterMoveToDungeonFireExitState();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnEntityTeleported() => EnterMoveToDungeonFireExitState();
    }
}