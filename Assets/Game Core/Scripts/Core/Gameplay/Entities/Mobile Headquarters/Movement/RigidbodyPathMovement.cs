using System;
using Cinemachine;
using GameCore.Configs.Gameplay.MobileHeadquarters;
using UnityEngine;

namespace GameCore.Gameplay.Entities.MobileHeadquarters
{
    public class RigidbodyPathMovement
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public RigidbodyPathMovement(MobileHeadquartersEntity mobileHeadquartersEntity, Animator animator, CinemachineDollyCart dollyCart,
            MobileHeadquartersConfigMeta mobileHeadquartersConfig)
        {
            _mobileHeadquartersEntity = mobileHeadquartersEntity;
            _rigidbody = mobileHeadquartersEntity.GetComponent<Rigidbody>();
            _animator = animator;
            _path = dollyCart.m_Path;
            _mobileHeadquartersConfig = mobileHeadquartersConfig;

            //_dollyCart.enabled = true;
        }

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnDestinationReachedEvent;

        private readonly MobileHeadquartersEntity _mobileHeadquartersEntity;
        private readonly Rigidbody _rigidbody;
        private readonly Animator _animator;
        private readonly MobileHeadquartersConfigMeta _mobileHeadquartersConfig;

        private CinemachinePathBase _path;
        private float _animationBlend;
        private bool _canMove;

        private float distance;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Movement()
        {
            if (_path == null)
                return;
            
            float targetSpeed = _canMove ? _mobileHeadquartersConfig.MovementSpeed : 0f;
            float speedChangeRate = _mobileHeadquartersConfig.SpeedChangeRate;

            distance += targetSpeed * Time.fixedDeltaTime;

            if (distance > _path.PathLength)
                distance -= _path.PathLength;
            
            _mobileHeadquartersEntity.SetPathPosition(distance);
            MoveRigidbody(distance);

            float inputMagnitude = _mobileHeadquartersConfig.MotionSpeed; // 1f

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * speedChangeRate);

            // update animator if using character
            _animator.SetFloat(AnimatorHashes.Speed, _animationBlend);
            _animator.SetFloat(AnimatorHashes.MotionSpeed, inputMagnitude);
        }

        public void ToggleMovement(bool canMove) =>
            _canMove = canMove;

        public void ChangePath(CinemachinePath path)
        {
            _path = path;
            MoveRigidbody(pos: 0);
        }

        public void SetPosition(float position) => MoveRigidbody(position);

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ToggleMoveAnimation(bool canMove) =>
            _animator.SetBool(id: AnimatorHashes.CanMove, value: canMove);

        private void MoveRigidbody(float pos)
        {
            Vector3 position = _path.EvaluatePositionAtUnit(pos, CinemachinePathBase.PositionUnits.Distance);
            Quaternion rotation = _path.EvaluateOrientationAtUnit(pos, CinemachinePathBase.PositionUnits.Distance);

            _rigidbody.MovePosition(position);
            _rigidbody.MoveRotation(rotation);
        }
    }
}