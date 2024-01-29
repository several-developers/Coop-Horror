using System;
using Cinemachine;
using GameCore.Configs.Gameplay.MobileHeadquarters;
using UnityEngine;

namespace GameCore.Gameplay.Entities.MobileHeadquarters
{
    public class RigidbodyPathMovement
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public RigidbodyPathMovement(MobileHeadquartersEntity mobileHeadquartersEntity, Animator animator,
            MobileHeadquartersConfigMeta mobileHeadquartersConfig)
        {
            _transform = mobileHeadquartersEntity.transform;
            _rigidbody = mobileHeadquartersEntity.GetComponent<Rigidbody>();
            _animator = animator;
            _mobileHeadquartersConfig = mobileHeadquartersConfig;

            //_dollyCart.enabled = true;
        }

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnDestinationReachedEvent;

        private readonly Transform _transform;
        private readonly Rigidbody _rigidbody;
        private readonly Animator _animator;
        private readonly MobileHeadquartersConfigMeta _mobileHeadquartersConfig;

        private CinemachinePathBase _path;
        private float _animationBlend;
        private float _distance;
        private float _syncCurrentTime;
        private bool _isArrived;
        private bool _canMove;

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
                
                _distance = _path.PathLength;
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

        public void ChangePath(CinemachinePath path)
        {
            _path = path;
            _distance = 0;

            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            
            Vector3 position = EvaluatePositionAtUnit(distance: 0f);
            Quaternion rotation = EvaluateOrientationAtUnit(distance: 0f);

            _transform.position = position;
            _transform.rotation = rotation;
        }

        public void ToggleArrived(bool isArrived) =>
            _isArrived = isArrived;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void MoveRigidbody(float distance)
        {
            Vector3 position = EvaluatePositionAtUnit(distance);
            Quaternion rotation = EvaluateOrientationAtUnit(distance);

            _rigidbody.MovePosition(position);
            _rigidbody.MoveRotation(rotation);
        }

        public Vector3 EvaluatePositionAtUnit(float distance) =>
            _path.EvaluatePositionAtUnit(distance, CinemachinePathBase.PositionUnits.Distance);

        public Quaternion EvaluateOrientationAtUnit(float distance) =>
            _path.EvaluateOrientationAtUnit(distance, CinemachinePathBase.PositionUnits.Distance);
    }
}