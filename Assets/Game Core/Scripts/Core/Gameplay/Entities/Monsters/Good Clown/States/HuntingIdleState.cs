using System.Collections;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Systems.Utilities;
using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.Entities.Monsters.GoodClown.States
{
    public class HuntingIdleState : IEnterState, ITickableState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public HuntingIdleState(GoodClownEntity goodClownEntity)
        {
            _goodClownEntity = goodClownEntity;
            _clownUtilities = goodClownEntity.GetClownUtilities();
            _transform = goodClownEntity.transform;
            _agent = goodClownEntity.GetAgent();

            GoodClownAIConfigMeta goodClownAIConfig = goodClownEntity.GetGoodClownAIConfig();
            _huntingIdleConfig = goodClownAIConfig.HuntingIdleConfig;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly GoodClownEntity _goodClownEntity;
        private readonly GoodClownAIConfigMeta.HuntingIdleSettings _huntingIdleConfig;
        private readonly GoodClownUtilities _clownUtilities;
        private readonly NavMeshAgent _agent;
        private readonly Transform _transform;

        private Coroutine _distanceCheckCO;
        private float _cachedAgentSpeed;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            DisableAgent();
            ReleaseBalloon();
            StartDistanceCheck();
        }

        public void Tick()
        {
            _clownUtilities.UpdateAnimationMoveSpeed();
            RotateToTarget();
        }

        public void Exit()
        {
            ResetAgent();
            StopDistanceCheck();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void DisableAgent()
        {
            _cachedAgentSpeed = _agent.speed;
            _agent.speed = 0f;
        }

        private void ResetAgent() =>
            _agent.speed = _cachedAgentSpeed;

        private void ReleaseBalloon() =>
            _goodClownEntity.ReleaseBalloonServerRpc();

        private void RotateToTarget()
        {
            PlayerEntity targetPlayer = _goodClownEntity.GetTargetPlayer();
            bool isTargetFound = targetPlayer != null;

            if (!isTargetFound)
            {
                EnterSearchForTargetState();
                return;
            }

            Transform target = targetPlayer.transform;
            float rotationSpeed = _huntingIdleConfig.RotationSpeed;
            
            EntitiesUtilities.RotateToTarget(owner: _transform, target, rotationSpeed);
        }

        private void CheckDistance()
        {
            PlayerEntity targetPlayer = _goodClownEntity.GetTargetPlayer();
            bool isTargetFound = targetPlayer != null;

            if (!isTargetFound)
            {
                EnterSearchForTargetState();
                return;
            }

            Vector3 playerPosition = targetPlayer.transform.position;
            Vector3 thisPosition = _transform.position;
            float distance = Vector3.Distance(a: playerPosition, b: thisPosition);
            bool chaseTarget = distance >= _huntingIdleConfig.MinDistanceToChase;

            if (!chaseTarget)
                return;

            EnterHuntingChaseState();
        }

        private void StartDistanceCheck()
        {
            IEnumerator routine = DistanceCheckCO();
            _distanceCheckCO = _goodClownEntity.StartCoroutine(routine);
        }

        private void StopDistanceCheck()
        {
            if (_distanceCheckCO == null)
                return;

            _goodClownEntity.StopCoroutine(_distanceCheckCO);
        }

        private void EnterSearchForTargetState() =>
            _goodClownEntity.EnterSearchForTargetState();

        private void EnterHuntingChaseState() =>
            _goodClownEntity.EnterHuntingChaseState();

        private IEnumerator DistanceCheckCO()
        {
            while (true)
            {
                float checkInterval = _huntingIdleConfig.DistanceCheckInterval;
                yield return new WaitForSeconds(checkInterval);

                CheckDistance();
            }
        }
    }
}