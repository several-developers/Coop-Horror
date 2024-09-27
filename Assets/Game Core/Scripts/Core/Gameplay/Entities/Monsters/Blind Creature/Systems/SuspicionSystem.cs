using System;
using System.Collections;
using GameCore.Infrastructure.Configs.Gameplay.Balance;
using GameCore.Infrastructure.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Systems.Utilities;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Monsters.BlindCreature
{
    public class SuspicionSystem
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public SuspicionSystem(BlindCreatureEntity blindCreatureEntity, BalanceConfigMeta balanceConfig)
        {
            BlindCreatureAIConfigMeta blindCreatureAIConfig = blindCreatureEntity.GetAIConfig();

            _blindCreatureEntity = blindCreatureEntity;
            _references = blindCreatureEntity.GetReferences();
            _suspicionSystemConfig = blindCreatureAIConfig.GetSuspicionSystemConfig();
            _balanceConfig = balanceConfig;
            _transform = blindCreatureEntity.transform;
        }

        // FIELDS: --------------------------------------------------------------------------------

        public event Action<int> OnSuspicionMeterChangedEvent = delegate { };
        public event Action OnNoiseDetectedEvent = delegate { };

        private readonly BlindCreatureEntity _blindCreatureEntity;
        private readonly BlindCreatureEntity.References _references;
        private readonly BlindCreatureAIConfigMeta.SuspicionSystemConfig _suspicionSystemConfig;
        private readonly BalanceConfigMeta _balanceConfig;
        private readonly Transform _transform;

        private Coroutine _decreaseTimerCO;
        private Vector3 _lastNoisePosition;
        private int _suspicionMeter;
        private bool _isAggressive;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void DetectNoise(Vector3 noisePosition, float noiseLoudness)
        {
            if (!IsLoudEnough(noiseLoudness))
                return;

            _lastNoisePosition = noisePosition;
            bool instantAggro = CheckForInstantAggro(noisePosition, noiseLoudness);

            if (instantAggro)
            {
                InstantAggro(noisePosition);
                return;
            }

            IncreaseSuspicionMeter();

            OnNoiseDetectedEvent.Invoke();
            StartDecreaseTimer();
            PlaySound(BlindCreatureEntity.SFXType.BirdScream);
        }

        public void InstantAggro(Vector3 noisePosition)
        {
            _lastNoisePosition = noisePosition;
            
            int suspicionMeterAmount = _suspicionSystemConfig.SuspicionMeterAfterInstantAggro;
            bool isValid = suspicionMeterAmount > _suspicionMeter;

            if (isValid)
                SetSuspicionMeter(suspicionMeterAmount);
            
            OnNoiseDetectedEvent.Invoke();
            StartDecreaseTimer();
            PlaySound(BlindCreatureEntity.SFXType.BirdScream);
        }

        private void PlaySound(BlindCreatureEntity.SFXType sfxType) =>
            _blindCreatureEntity.PlaySound(sfxType).Forget();

        public Vector3 GetLastNoisePosition() =>
            _lastNoisePosition;

        public int GetSuspicionMeter() => _suspicionMeter;

        public bool IsAggressive() => _isAggressive;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void StartDecreaseTimer()
        {
            if (_decreaseTimerCO != null)
                StopDecreaseTimer();

            IEnumerator routine = DecreaseTimerCO();
            _decreaseTimerCO = _blindCreatureEntity.StartCoroutine(routine);
        }

        private void StopDecreaseTimer()
        {
            if (_decreaseTimerCO == null)
                return;

            _blindCreatureEntity.StopCoroutine(_decreaseTimerCO);
        }
        
        private void IncreaseSuspicionMeter() => ChangeSuspicionMeter(increase: true);

        private void DecreaseSuspicionMeter() => ChangeSuspicionMeter(increase: false);

        private void ChangeSuspicionMeter(bool increase)
        {
            int a = _suspicionMeter;
            int b = increase ? 1 : -1;
            int value = a + b;

            if (increase && CheckForAggro())
                value = _suspicionSystemConfig.SuspicionMeterAfterInstantAggro;

            SetSuspicionMeter(value);
        }

        private void SetSuspicionMeter(int value)
        {
            int max = _suspicionSystemConfig.SuspicionMeterMaxAmount;
            _suspicionMeter = Mathf.Clamp(value, min: 0, max);

            if (CheckForAggro())
                EnterAggressiveMode();
            else
                ExitAggressiveMode();

            OnSuspicionMeterChangedEvent.Invoke(_suspicionMeter);
        }

        private void EnterAggressiveMode()
        {
            if (_isAggressive)
                return;

            _isAggressive = true;
            ChangeCreatureVisual();
        }

        private void ExitAggressiveMode()
        {
            if (!_isAggressive)
                return;

            _isAggressive = false;
            ChangeCreatureVisual();
        }

        private void ChangeCreatureVisual()
        {
            _references.CalmFace.SetActive(!_isAggressive);
            _references.AngryFace.SetActive(_isAggressive); 
            // _references.CalmCape.SetActive(!_isAggressive);
            // _references.AngryCape.SetActive(_isAggressive);
        }

        private IEnumerator DecreaseTimerCO()
        {
            while (_suspicionMeter > 0)
            {
                float delay = _suspicionSystemConfig.SuspicionMeterDecreaseTime;
                yield return new WaitForSeconds(delay);

                DecreaseSuspicionMeter();
            }
        }

        private bool IsLoudEnough(float noiseLoudness)
        {
            float minLoudnessToReact = _suspicionSystemConfig.MinLoudnessToReact;
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
            int suspicionMeterToAggro = _suspicionSystemConfig.SuspicionMeterToAggro;
            bool canAggro = _suspicionMeter >= suspicionMeterToAggro;
            return canAggro;
        }
    }
}