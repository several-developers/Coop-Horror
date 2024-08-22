using System.Collections;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Systems.Utilities;
using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.Entities.Monsters.BlindCreature.States
{
    public class IdleState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public IdleState(BlindCreatureEntity blindCreatureEntity)
        {
            BlindCreatureAIConfigMeta blindCreatureAIConfig = blindCreatureEntity.GetAIConfig();
            
            _blindCreatureEntity = blindCreatureEntity;
            _wanderingConfig = blindCreatureAIConfig.GetWanderingConfig();
            _suspicionSystem = blindCreatureEntity.GetSuspicionSystem();
            _wanderingTimerRoutine = new CoroutineHelper(blindCreatureEntity);
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly BlindCreatureEntity _blindCreatureEntity;
        private readonly BlindCreatureAIConfigMeta.WanderingConfig _wanderingConfig;
        private readonly SuspicionSystem _suspicionSystem;
        private readonly CoroutineHelper _wanderingTimerRoutine;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void Enter()
        {
            _suspicionSystem.OnNoiseDetectedEvent += OnNoiseDetected;
            
            _wanderingTimerRoutine.GetRoutineEvent += WanderingTimerCO;
            
            DisableAgent();
            _wanderingTimerRoutine.Start();
        }

        public void Exit()
        {
            _suspicionSystem.OnNoiseDetectedEvent -= OnNoiseDetected;
            
            _wanderingTimerRoutine.GetRoutineEvent -= WanderingTimerCO;
            
            _wanderingTimerRoutine.Stop();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void DisableAgent()
        {
            NavMeshAgent agent = _blindCreatureEntity.GetAgent();
            agent.enabled = false;
        }

        private IEnumerator WanderingTimerCO()
        {
            float minDelay = _wanderingConfig.MinDelay;
            float maxDelay = _wanderingConfig.MaxDelay;
            float timeBeforeWandering = Random.Range(minDelay, maxDelay);
            
            yield return new WaitForSeconds(timeBeforeWandering);
            
            _blindCreatureEntity.EnterWanderingState();
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnNoiseDetected() =>
            _blindCreatureEntity.DecideStateAfterNoiseDetect();
    }
}