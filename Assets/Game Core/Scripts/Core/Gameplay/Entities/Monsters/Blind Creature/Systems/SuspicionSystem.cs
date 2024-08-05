using System;
using System.Collections;
using GameCore.Configs.Gameplay.Balance;
using GameCore.Configs.Gameplay.Enemies;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Monsters.BlindCreature
{
    public class SuspicionSystem
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public SuspicionSystem(BlindCreatureEntity blindCreatureEntity, BalanceConfigMeta balanceConfig)
        {
            _blindCreatureEntity = blindCreatureEntity;
            _blindCreatureAIConfig = blindCreatureEntity.GetAIConfig();
            _balanceConfig = balanceConfig;
            _transform = blindCreatureEntity.transform;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action<int> OnSuspicionMeterChangedEvent = delegate { };
        public event Action OnNoiseDetectedEvent = delegate { };

        private readonly BlindCreatureEntity _blindCreatureEntity;
        private readonly BlindCreatureAIConfigMeta _blindCreatureAIConfig;
        private readonly BalanceConfigMeta _balanceConfig;
        private readonly Transform _transform;

        private Coroutine _decreaseTimerCO;
        private Vector3 _lastNoisePosition;
        private int _suspicionMeter;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void DetectNoise(Vector3 noisePosition, float noiseLoudness)
        {
            if (!IsLoudnessValid(noiseLoudness))
                return;

            _lastNoisePosition = noisePosition;
            bool instantAggro = CheckForInstantAggro(noisePosition, noiseLoudness);

            if (instantAggro)
            {
                int suspicionMeterAmount = _blindCreatureAIConfig.InstantAggroSuspicionMeterAmount;
                bool isValid = suspicionMeterAmount > _suspicionMeter;
                
                if (isValid)
                    SetSuspicionMeter(suspicionMeterAmount);
            }
            else
            {
                IncreaseSuspicionMeter();
            }
            
            OnNoiseDetectedEvent.Invoke();
            StartDecreaseTimer();
        }
        
        public void StartDecreaseTimer()
        {
            if (_decreaseTimerCO != null)
                StopDecreaseTimer();
            
            IEnumerator routine = DecreaseTimerCO();
            _decreaseTimerCO = _blindCreatureEntity.StartCoroutine(routine);
        }

        public void StopDecreaseTimer()
        {
            if (_decreaseTimerCO == null)
                return;
            
            _blindCreatureEntity.StopCoroutine(_decreaseTimerCO);
        }

        public Vector3 GetLastNoisePosition() =>
            _lastNoisePosition;

        public int GetSuspicionMeter() => _suspicionMeter;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void IncreaseSuspicionMeter() => ChangeSuspicionMeter(increase: true);

        private void DecreaseSuspicionMeter() => ChangeSuspicionMeter(increase: false);

        private void ChangeSuspicionMeter(bool increase)
        {
            int a = _suspicionMeter;
            int b = increase ? 1 : -1;
            int value = a + b;

            if (increase && CheckForAggro())
                value = _blindCreatureAIConfig.InstantAggroSuspicionMeterAmount;
            
            SetSuspicionMeter(value);
        }

        private void SetSuspicionMeter(int value)
        {
            //int previousSuspicionMeter = _suspicionMeter;
            int max = _blindCreatureAIConfig.SuspicionMeterMaxAmount;
            _suspicionMeter = Mathf.Clamp(value, min: 0, max);

            // bool sendEvent = _suspicionMeter != previousSuspicionMeter;
            //
            // if (!sendEvent)
            //     return;
            
            OnSuspicionMeterChangedEvent.Invoke(_suspicionMeter);
        }

        private IEnumerator DecreaseTimerCO()
        {
            while (_suspicionMeter > 0)
            {
                float delay = _blindCreatureAIConfig.SuspicionMeterDecreaseTime;
                yield return new WaitForSeconds(delay);

                DecreaseSuspicionMeter();
            }
        }

        private bool IsLoudnessValid(float noiseLoudness)
        {
            float minLoudnessToReact = _blindCreatureAIConfig.MinLoudnessToReact;
            bool isValid = noiseLoudness >= minLoudnessToReact;
            return isValid;
        }
        
        private bool CheckForInstantAggro(Vector3 noisePosition, float noiseLoudness)
        {
            float noiseLoudnessMultiplier = _balanceConfig.NoiseLoudnessMultiplier;
            float loudness = noiseLoudness * noiseLoudnessMultiplier;
            float distance = Vector3.Distance(a: _transform.position, b: noisePosition);
            bool canAggro = distance <= loudness;
            return canAggro;
        }

        private bool CheckForAggro()
        {
            int suspicionMeterToAggro = _blindCreatureAIConfig.SuspicionMeterToAggro;
            bool canAggro = _suspicionMeter >= suspicionMeterToAggro;
            return canAggro;
        }
    }
}