using System.Collections;
using GameCore.Configs.Gameplay.Enemies;
using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.Entities.Monsters.BlindCreature.States
{
    public class IdleState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public IdleState(BlindCreatureEntity blindCreatureEntity)
        {
            _blindCreatureEntity = blindCreatureEntity;
            _blindCreatureAIConfig = blindCreatureEntity.GetAIConfig();
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly BlindCreatureEntity _blindCreatureEntity;
        private readonly BlindCreatureAIConfigMeta _blindCreatureAIConfig;
        
        private Coroutine _wanderingTimerCO;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void Enter()
        {
            DisableAgent();
            StopWanderingTimer();
            StartWanderingTimer();
        }

        public void Exit() => StopWanderingTimer();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void DisableAgent()
        {
            NavMeshAgent agent = _blindCreatureEntity.GetAgent();
            agent.enabled = false;
        }

        private void StartWanderingTimer()
        {
            float minDelay = _blindCreatureAIConfig.WanderingMinDelay;
            float maxDelay = _blindCreatureAIConfig.WanderingMaxDelay;
            float timeBeforeWandering = Random.Range(minDelay, maxDelay);

            IEnumerator routine = WanderingTimerCO(timeBeforeWandering);
            _wanderingTimerCO = _blindCreatureEntity.StartCoroutine(routine);
        }

        private void StopWanderingTimer()
        {
            if (_wanderingTimerCO == null)
                return;
            
            _blindCreatureEntity.StopCoroutine(_wanderingTimerCO);
        }
        
        private IEnumerator WanderingTimerCO(float timeBeforeWandering)
        {
            yield return new WaitForSeconds(timeBeforeWandering);
            
            _blindCreatureEntity.EnterWanderingState();
        }
    }
}