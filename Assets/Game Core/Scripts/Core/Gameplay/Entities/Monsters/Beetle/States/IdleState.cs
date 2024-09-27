using System.Collections;
using GameCore.Infrastructure.Configs.Gameplay.Enemies;
using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.Entities.Monsters.Beetle.States
{
    public class IdleState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public IdleState(BeetleEntity beetleEntity)
        {
            _beetleEntity = beetleEntity;
            _beetleAIConfig = beetleEntity.GetAIConfig();
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly BeetleEntity _beetleEntity;
        private readonly BeetleAIConfigMeta _beetleAIConfig;

        private Coroutine _wanderingTimerCO;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void Enter()
        {
            AggressionSystem aggressionSystem = _beetleEntity.GetAggressionSystem();
            
            EnableAggressionSystemTriggerCheck(aggressionSystem);
            DisableAgent();
            StopWanderingTimer();
            StartWanderingTimer();
        }

        public void Exit() => StopWanderingTimer();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private static void EnableAggressionSystemTriggerCheck(AggressionSystem aggressionSystem) =>
            aggressionSystem.ToggleTriggerCheckState(isEnabled: true);

        private void DisableAgent()
        {
            NavMeshAgent agent = _beetleEntity.GetAgent();
            agent.enabled = false;
        }

        private void StartWanderingTimer()
        {
            float minDelay = _beetleAIConfig.WanderingMinDelay;
            float maxDelay = _beetleAIConfig.WanderingMaxDelay;
            float timeBeforeWandering = Random.Range(minDelay, maxDelay);

            IEnumerator routine = WanderingTimerCO(timeBeforeWandering);
            _wanderingTimerCO = _beetleEntity.StartCoroutine(routine);
        }

        private void StopWanderingTimer()
        {
            if (_wanderingTimerCO == null)
                return;
            
            _beetleEntity.StopCoroutine(_wanderingTimerCO);
        }
        
        private IEnumerator WanderingTimerCO(float timeBeforeWandering)
        {
            yield return new WaitForSeconds(timeBeforeWandering);
            
            _beetleEntity.EnterWanderingState();
        }
    }
}