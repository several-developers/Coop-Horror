using System;
using GameCore.Configs.Gameplay.Train;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Train
{
    public class MoveSpeedController : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title("Debug Info")]
        [SerializeField, ReadOnly]
        private float _currentSpeedDebug;

        [SerializeField, ReadOnly]
        private float _targetSpeedDebug;

        [Title("Debug Settings")]
        [SerializeField, Range(0f, 1f), SuffixLabel("%", overlay: true)]
        [OnValueChanged(nameof(OnDistanceChanged))]
        private float _distance;

        [SerializeField]
        private bool _changeTargetSpeed;

        [SerializeField, Min(0)]
        [ShowIf(nameof(_changeTargetSpeed))]
        private float _targetSpeed;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action<float> OnDistanceChangedEvent = delegate { };

        private TrainConfigMeta _trainConfig;
        private float _speedMultiplier;
        private float _leavingMainRoadSpeedMultiplier;
        private float _currentSpeed;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Tick()
        {
            float targetSpeed = _changeTargetSpeed ? _targetSpeed : _trainConfig.MovementSpeed;
            float finalTargetSpeed = targetSpeed + targetSpeed * (_speedMultiplier + _leavingMainRoadSpeedMultiplier);
            float speedChangeRate = _trainConfig.SpeedChangeRate;
            float newSpeed = Mathf.Lerp(a: _currentSpeed, b: finalTargetSpeed, t: speedChangeRate * Time.deltaTime);

            _currentSpeed = Mathf.Max(a: newSpeed, b: 0f);
            _currentSpeedDebug = _currentSpeed;
            _targetSpeedDebug = finalTargetSpeed;
        }

        public void Init(TrainConfigMeta trainConfig)
        {
            _trainConfig = trainConfig;
            _targetSpeed = trainConfig.MovementSpeed;
            _currentSpeed = _targetSpeed;
        }

        public void IncreaseSpeedMultiplier(float amount) =>
            _speedMultiplier = Mathf.Clamp(_speedMultiplier + amount, min: -1f, max: 1f);

        public void DecreaseSpeedMultiplier(float amount) =>
            _speedMultiplier = Mathf.Clamp(_speedMultiplier - amount, min: -1f, max: 1f);

        public float GetCurrentSpeed() => _currentSpeed;

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnDistanceChanged(float distance) =>
            OnDistanceChangedEvent.Invoke(distance);
    }
}