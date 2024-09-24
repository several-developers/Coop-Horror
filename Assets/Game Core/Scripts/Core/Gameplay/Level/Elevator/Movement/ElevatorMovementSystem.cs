using GameCore.Configs.Gameplay.Elevator;
using GameCore.Gameplay.Network;
using UnityEngine;

namespace GameCore.Gameplay.Level.Elevator
{
    public class ElevatorMovementSystem
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ElevatorMovementSystem(ElevatorBase elevatorBase)
        {
            _elevatorBase = elevatorBase;
            _elevatorConfig = elevatorBase.GetElevatorConfig();
            _transform = elevatorBase.transform;
            _startPositionY = _transform.localPosition.y;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly ElevatorBase _elevatorBase;
        private readonly ElevatorConfigMeta _elevatorConfig;
        private readonly Transform _transform;
        private readonly float _startPositionY;
        
        private float _endValue;
        private float _time;
        
        private bool _isMoving;
        private bool _isMovingUp;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void Tick()
        {
            float duration = GetMovementDuration();
            _time += Time.deltaTime / duration;
            
            if (_time > 1f)
                _time = 1f;

            float interpolatedValue = GetInterpolatedValue();
            Vector3 localPosition = _transform.localPosition;
            
            _transform.localPosition = new Vector3(x: interpolatedValue, y: localPosition.y, z: localPosition.z);
        }

        public void StartMovement()
        {
            _time = 0f;
            _isMoving = true;
        }

        public void StopMovement() =>
            _isMoving = false;

        public void UpdateState(ElevatorStaticData data) =>
            _isMovingUp = data.IsMovingUp;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private AnimationCurve GetAnimationCurve() =>
            _isMoving ? _elevatorConfig.SpeedUpCurve : _elevatorConfig.SlowDownCurve;

        private float GetMovementDuration() =>
            _elevatorConfig.FloorMovementDuration;
        
        private float GetInterpolatedValue()
        {
            float endPositionY = GetEndPositionY();
            AnimationCurve animationCurve = GetAnimationCurve();
            return Mathf.Lerp(a: _startPositionY, b: endPositionY, t: animationCurve.Evaluate(_time));
        }

        private float GetEndPositionY()
        {
            if (!_isMoving)
                return _startPositionY;

            float movementOffsetY = _elevatorConfig.MovementOffsetY;

            if (_isMovingUp)
                return _startPositionY + movementOffsetY;
            
            return _startPositionY - movementOffsetY;
        }
    }
}