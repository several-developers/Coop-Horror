using System;
using System.Collections;
using GameCore.Configs.Gameplay.Balance;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Systems.Utilities;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Monsters.SpikySlime
{
    public class AggressionSystem
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        public AggressionSystem(SpikySlimeEntity spikySlimeEntity, BalanceConfigMeta balanceConfig)
        {
            SpikySlimeReferences references = spikySlimeEntity.GetReferences();
            SpikySlimeAIConfigMeta spikySlimeAIConfig = spikySlimeEntity.GetAIConfig();
            
            _spikySlimeEntity = spikySlimeEntity;
            _aggressionSystemConfig = spikySlimeAIConfig.GetAggressionSystemConfig();
            _balanceConfig = balanceConfig;
            _transform = spikySlimeEntity.transform;
            _shakeAnimation = references.ShakeAnimation;
            _decreaseTimerRoutine = new CoroutineHelper(spikySlimeEntity);

            _decreaseTimerRoutine.GetRoutineEvent += DecreaseTimerCO;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action<int> OnAggressionMeterChangedEvent = delegate { };
        public event Action OnStartAttackEvent = delegate { };
        
        private readonly SpikySlimeEntity _spikySlimeEntity;
        private readonly SpikySlimeAIConfigMeta.AggressionSystemConfig _aggressionSystemConfig;
        private readonly BalanceConfigMeta _balanceConfig;
        private readonly Transform _transform;
        private readonly ShakeAnimation _shakeAnimation;
        private readonly CoroutineHelper _decreaseTimerRoutine;
        
        private int _aggressionMeter;
        private bool _isAggressive;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void DetectNoise(Vector3 noisePosition, float noiseLoudness)
        {
            if (!IsLoudEnough(noiseLoudness))
                return;

            bool instantAggro = CheckForInstantAggro(noisePosition, noiseLoudness);
            int increaseValue = 1;

            if (instantAggro)
                increaseValue += 1;

            _shakeAnimation.PlayAnimation();
            IncreaseAggressionMeter(increaseValue);
            PlaySound(SpikySlimeEntity.SFXType.Angry);
        }

        public void StartDecreaseTimer()
        {
            _isAggressive = false;
            
            _decreaseTimerRoutine.Start();
            PlaySound(SpikySlimeEntity.SFXType.Calming);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void IncreaseAggressionMeter(int value) => ChangeAggressionMeter(increase: true, value);

        private void DecreaseAggressionMeter() => ChangeAggressionMeter(increase: false);

        private void ChangeAggressionMeter(bool increase, int value = 1)
        {
            int a = _aggressionMeter;
            int b = increase ? value : -value;
            int result = a + b;

            SetAggressionMeter(result);
        }
        
        private void SetAggressionMeter(int value)
        {
            int max = _aggressionSystemConfig.AggressionMeterToAggro;
            _aggressionMeter = Mathf.Clamp(value, min: 0, max);

            if (CheckForAggro())
            {
                _isAggressive = true;
                _decreaseTimerRoutine.Stop();
                OnStartAttackEvent.Invoke();
            }
            else if (!_isAggressive)
            {
                _decreaseTimerRoutine.Start();
            }

            OnAggressionMeterChangedEvent.Invoke(_aggressionMeter);
        }

        private void PlaySound(SpikySlimeEntity.SFXType sfxType) =>
            _spikySlimeEntity.PlaySound(sfxType).Forget();

        private IEnumerator DecreaseTimerCO()
        {
            while (_aggressionMeter > 0)
            {
                float delay = _aggressionSystemConfig.AggressionMeterDecreaseTime;
                yield return new WaitForSeconds(delay);

                DecreaseAggressionMeter();
            }
        }
        
        private bool IsLoudEnough(float noiseLoudness)
        {
            float minLoudnessToReact = _aggressionSystemConfig.MinLoudnessToReact;
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
            int suspicionMeterToAggro = _aggressionSystemConfig.AggressionMeterToAggro;
            bool canAggro = _aggressionMeter >= suspicionMeterToAggro;
            return canAggro;
        }
    }
}