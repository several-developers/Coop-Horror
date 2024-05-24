using Cinemachine;
using GameCore.Configs.Gameplay.MobileHeadquarters;
using UnityEngine;

namespace GameCore.Gameplay.Entities.MobileHeadquarters
{
    public class TEMPPathMovement
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public TEMPPathMovement(Transform transform, Animator animator, CinemachineDollyCart dollyCart, CinemachinePathBase cinemachinePath,
            MobileHeadquartersConfigMeta mobileHeadquartersConfig)
        {
            _transform = transform;
            _animator = animator;
            _dollyCart = dollyCart;
            _dollyCart.m_Path = cinemachinePath;
            _mobileHeadquartersConfig = mobileHeadquartersConfig;

            _dollyCart.enabled = true;
        }
        
        // FIELDS: --------------------------------------------------------------------------------

        private readonly Transform _transform;
        private readonly CinemachineDollyCart _dollyCart;
        private readonly Animator _animator;
        private readonly MobileHeadquartersConfigMeta _mobileHeadquartersConfig;
        
        private CinemachinePathBase _nextPath;
        private float _animationBlend;
        private bool _canMove;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Movement()
        {
            bool isPathFinished = _dollyCart.m_Path == null;
            float targetSpeed = _canMove ? _mobileHeadquartersConfig.MovementSpeed : 0f;
            float speedChangeRate = _mobileHeadquartersConfig.SpeedChangeRate;

            if (isPathFinished)
            {
                targetSpeed = 0;

                ToggleMoveAnimation(canMove: false);
            }
            else
            {
                ToggleMoveAnimation(canMove: true);
            }
            
            float inputMagnitude = _mobileHeadquartersConfig.MotionSpeed; // 1f
            const float speedOffset = 0.1f;
            
            float finalSpeed;
            float currentSpeed = _dollyCart.m_Speed;
            bool changeSpeedSmoothly = currentSpeed < targetSpeed - speedOffset ||
                                       currentSpeed > targetSpeed + speedOffset;

            // accelerate or decelerate to target speed
            if (changeSpeedSmoothly)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                finalSpeed = Mathf.Lerp(currentSpeed, targetSpeed * inputMagnitude, Time.deltaTime * speedChangeRate);

                // round speed to 3 decimal places
                finalSpeed = Mathf.Round(finalSpeed * 1000f) / 1000f;
            }
            else
            {
                finalSpeed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * speedChangeRate);
            _dollyCart.m_Speed = finalSpeed;
            
            if (finalSpeed < speedOffset)
                ToggleMoveAnimation(canMove: false);
            
            // update animator if using character
            _animator.SetFloat(AnimatorHashes.Speed, _animationBlend);
            _animator.SetFloat(AnimatorHashes.MotionSpeed, inputMagnitude);

            if (isPathFinished)
                return;

            float pathLength = _dollyCart.m_Path.PathLength;
            float position = _dollyCart.m_Position;
            bool changePath = _nextPath == null && position >= pathLength;

            if (changePath)
                ChangeToNextPath();
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