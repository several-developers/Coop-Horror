using System;
using Cinemachine;
using GameCore.Configs.Gameplay.MobileHeadquarters;
using UnityEngine;

namespace GameCore.Gameplay.Entities.MobileHeadquarters
{
    public class PathMovement
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public PathMovement(MobileHeadquartersEntity mobileHeadquartersEntity,
            MobileHeadquartersConfigMeta mobileHeadquartersConfig)
        {
            _transform = mobileHeadquartersEntity.transform;
            _animator = mobileHeadquartersEntity.References.Animator;
            _mobileHeadquartersConfig = mobileHeadquartersConfig;
        }

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnDestinationReachedEvent;

        private readonly Transform _transform;
        private readonly Animator _animator;
        private readonly MobileHeadquartersConfigMeta _mobileHeadquartersConfig;

        private CinemachinePathBase _path;
        private float _animationBlend;
        private float _distance;
        private float _syncCurrentTime;
        private bool _isArrived;
        private bool _canMove = true;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Movement()
        {
            if (_path == null)
                return;

            float targetSpeed = _canMove ? _mobileHeadquartersConfig.MovementSpeed : 0f;
            float speedChangeRate = _mobileHeadquartersConfig.SpeedChangeRate;

            _distance += targetSpeed * Time.fixedDeltaTime;

            if (_distance > _path.PathLength)
            {
                if (!_isArrived)
                {
                    _isArrived = true;
                    OnDestinationReachedEvent?.Invoke();
                }
                else
                {
                    _distance = _path.PathLength;
                }
            }

            MoveRigidbody(_distance);

            float inputMagnitude = _mobileHeadquartersConfig.MotionSpeed; // 1f

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * speedChangeRate);

            // update animator if using character
            _animator.SetFloat(AnimatorHashes.Speed, _animationBlend);
            _animator.SetFloat(AnimatorHashes.MotionSpeed, inputMagnitude);
        }

        public void ToggleMovement(bool canMove) =>
            _canMove = canMove;

        public void ToggleMoveAnimation(bool canMove) =>
            _animator.SetBool(id: AnimatorHashes.CanMove, value: canMove);

        public void ChangePath(CinemachinePath path, float startDistancePercent = 0f, bool stayAtSamePosition = false)
        {
            _path = path;
            startDistancePercent = Mathf.Clamp01(startDistancePercent);
            
            float startDistance = 0;

            if (startDistancePercent > 0.0f)
                startDistance = _path.PathLength * startDistancePercent;

            if (stayAtSamePosition)
            {
                float newDistance = GetDistanceAtPosition(_transform.position);
                Debug.LogWarning("New Distance: " + newDistance);
                _distance = startDistance;
            }
            else
            {
                _distance = startDistance;
            }

            Vector3 position = EvaluatePositionAtUnit(distance: 0f);
            Quaternion rotation = EvaluateOrientationAtUnit(distance: 0f);

            _transform.position = position;
            _transform.rotation = rotation;
        }

        public void ToggleArrived(bool isArrived) =>
            _isArrived = isArrived;

        public void ResetDistance()
        {
            float difference = _distance - _path.PathLength;
            _distance = difference;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void MoveRigidbody(float distance)
        {
            Vector3 position = EvaluatePositionAtUnit(distance);
            Quaternion rotation = EvaluateOrientationAtUnit(distance);

            _transform.position = position;
            _transform.rotation = rotation;
        }

        private Vector3 EvaluatePositionAtUnit(float distance) =>
            _path.EvaluatePositionAtUnit(distance, CinemachinePathBase.PositionUnits.Distance);

        private Quaternion EvaluateOrientationAtUnit(float distance) =>
            _path.EvaluateOrientationAtUnit(distance, CinemachinePathBase.PositionUnits.Distance);

        private float GetDistanceAtPosition(Vector3 position) =>
            _path.FindClosestPoint(position, startSegment: 0, searchRadius: 1, stepsPerSegment: 1);
    }
}