using Cinemachine;
using GameCore.Configs.Gameplay.MobileHeadquarters;
using UnityEngine;

namespace GameCore.Gameplay.Entities.MobileHeadquarters
{
    public class RigidbodyPathMovement
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public RigidbodyPathMovement(Transform transform, Animator animator, CinemachineDollyCart dollyCart,
            CinemachinePathBase path, MobileHeadquartersConfigMeta mobileHeadquartersConfig)
        {
            _transform = transform;
            _rigidbody = transform.GetComponent<Rigidbody>();
            _animator = animator;
            _dollyCart = dollyCart;
            _path = path;
            _dollyCart.m_Path = path;
            _mobileHeadquartersConfig = mobileHeadquartersConfig;

            //_dollyCart.enabled = true;
        }
        
        // FIELDS: --------------------------------------------------------------------------------

        private readonly Transform _transform;
        private readonly Rigidbody _rigidbody;
        private readonly CinemachineDollyCart _dollyCart;
        private readonly CinemachinePathBase _path;
        private readonly Animator _animator;
        private readonly MobileHeadquartersConfigMeta _mobileHeadquartersConfig;
        
        private CinemachinePathBase _nextPath;
        private float _animationBlend;
        private bool _canMove;

        private float distance;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Movement()
        {
            float targetSpeed = _canMove ? _mobileHeadquartersConfig.MovementSpeed : 0f;
            float speedChangeRate = _mobileHeadquartersConfig.SpeedChangeRate;
            
            distance += targetSpeed * Time.fixedDeltaTime;
            
            if (distance > _path.PathLength)
                distance -= _path.PathLength;

            Vector3 position = _path.EvaluatePositionAtUnit(distance, CinemachinePathBase.PositionUnits.Distance);
            Quaternion rotation = _path.EvaluateOrientationAtUnit(distance, CinemachinePathBase.PositionUnits.Distance);

            _rigidbody.MovePosition(position);
            _rigidbody.MoveRotation(rotation);
            
            float inputMagnitude = _mobileHeadquartersConfig.MotionSpeed; // 1f
            
            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * speedChangeRate);
            
            // update animator if using character
            _animator.SetFloat(AnimatorHashes.Speed, _animationBlend);
            _animator.SetFloat(AnimatorHashes.MotionSpeed, inputMagnitude);
        }
        
        public void ToggleMovement(bool canMove) =>
            _canMove = canMove;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ChangeToNextPath()
        {
            ChangePath(_nextPath);

            _nextPath = null;
        }

        private void ChangePath(CinemachinePathBase path)
        {
            _dollyCart.m_Position = 0;
            _dollyCart.m_Path = path;
        }

        private void ToggleMoveAnimation(bool canMove) =>
            _animator.SetBool(id: AnimatorHashes.CanMove, value: canMove);
    }
}