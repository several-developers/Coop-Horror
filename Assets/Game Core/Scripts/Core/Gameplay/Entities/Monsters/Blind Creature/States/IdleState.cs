using System.Collections;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.EntitiesSystems.Utilities;
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
            _suspicionSystem = blindCreatureEntity.GetSuspicionSystem();
            _wanderingTimerRoutine = new CoroutineHelper(blindCreatureEntity);
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly BlindCreatureEntity _blindCreatureEntity;
        private readonly BlindCreatureAIConfigMeta _blindCreatureAIConfig;
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

        private void EnterMoveToSuspicionPlaceState() =>
            _blindCreatureEntity.EnterMoveToSuspicionPlaceState();

        private IEnumerator WanderingTimerCO()
        {
            float minDelay = _blindCreatureAIConfig.WanderingMinDelay;
            float maxDelay = _blindCreatureAIConfig.WanderingMaxDelay;
            float timeBeforeWandering = Random.Range(minDelay, maxDelay);
            
            yield return new WaitForSeconds(timeBeforeWandering);
            
            _blindCreatureEntity.EnterWanderingState();
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnNoiseDetected() => EnterMoveToSuspicionPlaceState();
    }
}