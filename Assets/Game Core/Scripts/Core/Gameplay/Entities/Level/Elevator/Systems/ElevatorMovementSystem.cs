using Cysharp.Threading.Tasks;
using GameCore.Configs.Gameplay.Elevator;
using GameCore.Gameplay.Level.Elevator;
using GameCore.Utilities;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Level.Elevator
{
    public class ElevatorMovementSystem
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ElevatorMovementSystem(ElevatorEntity elevatorEntity)
        {
            _elevatorEntity = elevatorEntity;
            _elevatorConfig = elevatorEntity.GetElevatorConfig();
            _transform = elevatorEntity.transform;
            _startPositionY = _transform.localPosition.y;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly ElevatorEntity _elevatorEntity;
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

            if (_time >= 1.0f)
            {
                _time = 1f;
                // StopMovement();
            }

            float interpolatedValue = GetInterpolatedValue();
            Vector3 localPosition = _transform.localPosition;
            
            _transform.localPosition = new Vector3(x: localPosition.x, y: interpolatedValue, z: localPosition.z);
        }

        public async UniTaskVoid StartMovement()
        {
            float movementDelay = _elevatorConfig.MovementDelay;
            int delay = movementDelay.ConvertToMilliseconds();

            bool isCanceled = await UniTask
                .Delay(delay, cancellationToken: _elevatorEntity.GetCancellationTokenOnDestroy())
                .SuppressCancellationThrow();

            if (isCanceled)
                return;
            
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
            _elevatorConfig.MovementDurationPerFloor;
        
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