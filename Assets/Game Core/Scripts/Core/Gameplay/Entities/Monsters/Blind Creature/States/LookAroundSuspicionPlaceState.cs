using System.Collections;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Systems.Movement;
using GameCore.Gameplay.Systems.Utilities;
using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.Entities.Monsters.BlindCreature.States
{
    public class LookAroundSuspicionPlaceState : IEnterState, ITickableState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public LookAroundSuspicionPlaceState(BlindCreatureEntity blindCreatureEntity)
        {
            BlindCreatureAIConfigMeta blindCreatureAIConfig = blindCreatureEntity.GetAIConfig();

            _blindCreatureEntity = blindCreatureEntity;
            _suspicionStateConfig = blindCreatureAIConfig.GetSuspicionStateConfig();
            _lookAroundConfig = blindCreatureAIConfig.GetLookAroundConfig();
            _suspicionSystem = blindCreatureEntity.GetSuspicionSystem();
            _whispersTimerRoutine = new CoroutineHelper(blindCreatureEntity);
            _lookAroundTimerRoutine = new CoroutineHelper(blindCreatureEntity);
            _agent = blindCreatureEntity.GetAgent();
            _wanderingMovementLogic = new WanderingMovementLogic(blindCreatureEntity.transform, _agent);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly BlindCreatureEntity _blindCreatureEntity;
        private readonly BlindCreatureAIConfigMeta.SuspicionStateConfig _suspicionStateConfig;
        private readonly BlindCreatureAIConfigMeta.WanderingConfig _lookAroundConfig;
        private readonly SuspicionSystem _suspicionSystem;
        private readonly CoroutineHelper _whispersTimerRoutine;
        private readonly CoroutineHelper _lookAroundTimerRoutine;
        private readonly NavMeshAgent _agent;
        private readonly WanderingMovementLogic _wanderingMovementLogic;

        private bool _isWhispersOnCooldown;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            _suspicionSystem.OnSuspicionMeterChangedEvent += OnSuspicionMeterChanged;
            _suspicionSystem.OnNoiseDetectedEvent += OnNoiseDetected;

            _whispersTimerRoutine.GetRoutineEvent += WhispersTimerCO;

            _lookAroundTimerRoutine.GetRoutineEvent += LookAroundTimerCO;

            _wanderingMovementLogic.GetRandomPositionEvent += GetRandomPosition;
            _wanderingMovementLogic.GetWanderingMinDistanceEvent += GetLookAroundMinDistance;
            _wanderingMovementLogic.GetWanderingMaxDistanceEvent += GetLookAroundMaxDistance;

            EnableAgent();
            _lookAroundTimerRoutine.Start();
            TryPlayWhispersSound();
            TryLeaveState();
        }

        public void Tick() =>
            _wanderingMovementLogic.Tick();

        public void Exit()
        {
            _lookAroundTimerRoutine.Stop();

            _suspicionSystem.OnSuspicionMeterChangedEvent -= OnSuspicionMeterChanged;
            _suspicionSystem.OnNoiseDetectedEvent -= OnNoiseDetected;

            _whispersTimerRoutine.GetRoutineEvent -= WhispersTimerCO;

            _lookAroundTimerRoutine.GetRoutineEvent -= LookAroundTimerCO;

            _wanderingMovementLogic.GetRandomPositionEvent -= GetRandomPosition;
            _wanderingMovementLogic.GetWanderingMinDistanceEvent -= GetLookAroundMinDistance;
            _wanderingMovementLogic.GetWanderingMaxDistanceEvent -= GetLookAroundMaxDistance;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void EnableAgent()
        {
            _agent.enabled = true;
            _agent.speed = GetLookAroundSpeed();
            _agent.acceleration = _lookAroundConfig.Acceleration;
        }

        private void TryPlayWhispersSound()
        {
            if (_isWhispersOnCooldown)
                return;
            
            _blindCreatureEntity.PlaySound(BlindCreatureEntity.SFXType.Whispers);
            _whispersTimerRoutine.Start();
        }

        private void TryLeaveState()
        {
            int suspicionMeter = _suspicionSystem.GetSuspicionMeter();
            bool canLeave = suspicionMeter <= 0;

            if (!canLeave)
                return;

            EnterIdleState();
        }

        private void EnterIdleState() =>
            _blindCreatureEntity.EnterIdleState();

        private IEnumerator WhispersTimerCO()
        {
            _isWhispersOnCooldown = true;
            
            Vector2 whispersDelay = _suspicionStateConfig.WhispersDelay;
            float delay = Random.Range(whispersDelay.x, whispersDelay.y);

            yield return new WaitForSeconds(delay);

            _isWhispersOnCooldown = false;
        }

        private IEnumerator LookAroundTimerCO()
        {
            while (true)
            {
                float minDelay = _lookAroundConfig.MinDelay;
                float maxDelay = _lookAroundConfig.MaxDelay;
                float timeBeforeWandering = Random.Range(minDelay, maxDelay);

                yield return new WaitForSeconds(timeBeforeWandering);

                _wanderingMovementLogic.TrySetDestinationPoint();
                TryPlayWhispersSound();
            }
        }

        private Vector3 GetRandomPosition()
        {
            float minDistance = _lookAroundConfig.MinDistance;
            float maxDistance = _lookAroundConfig.MaxDistance;
            float distance = Random.Range(minDistance, maxDistance);

            Vector2 circle = Random.insideUnitCircle;
            circle *= distance;

            Vector3 circlePosition = new(x: circle.x, y: 0f, z: circle.y);
            Vector3 lastNoisePosition = _suspicionSystem.GetLastNoisePosition();
            Vector3 newPosition = circlePosition + lastNoisePosition;

            return newPosition;
        }

        private float GetLookAroundSpeed()
        {
            float minSpeed = _lookAroundConfig.MinSpeed;
            float maxSpeed = _lookAroundConfig.MaxSpeed;
            float speed = Random.Range(minSpeed, maxSpeed);
            return speed;
        }

        private float GetLookAroundMinDistance() =>
            _lookAroundConfig.MinDistance;

        private float GetLookAroundMaxDistance() =>
            _lookAroundConfig.MaxDistance;

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnSuspicionMeterChanged(int suspicionMeter) => TryLeaveState();

        private void OnNoiseDetected() =>
            _blindCreatureEntity.DecideStateAfterNoiseDetect();
    }
}