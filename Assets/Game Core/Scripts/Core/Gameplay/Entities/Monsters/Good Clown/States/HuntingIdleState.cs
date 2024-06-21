using System.Collections;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Entities.Player;
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

            GoodClownAIConfigMeta goodClownAIConfig = goodClownEntity.GetGoodClownAIConfig();
            _huntingIdleConfig = goodClownAIConfig.HuntingIdleConfig;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly GoodClownEntity _goodClownEntity;
        private readonly GoodClownAIConfigMeta.HuntingIdleSettings _huntingIdleConfig;
        private readonly GoodClownUtilities _clownUtilities;
        private readonly Transform _transform;

        private Coroutine _distanceCheckCO;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void Enter()
        {
            DisableAgent();
            SetIdleAnimation();
            ReleaseBalloon();
            StartDistanceCheck();
        }

        public void Tick()
        {
            _clownUtilities.UpdateAnimationMoveSpeed();
            RotateToTarget();
        }

        public void Exit() => StopDistanceCheck();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void DisableAgent()
        {
            NavMeshAgent agent = _goodClownEntity.GetAgent();
            agent.enabled = false;
        }
        
        private void SetIdleAnimation() =>
            _clownUtilities.SetIdleAnimation();

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
            
            _transform.LookAt(targetPlayer.transform);
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