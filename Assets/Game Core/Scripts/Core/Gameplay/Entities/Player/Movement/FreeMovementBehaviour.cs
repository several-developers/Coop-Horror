using UnityEngine;

namespace GameCore.Gameplay.Entities.Player.Movement
{
    public class FreeMovementBehaviour : IMovementBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public FreeMovementBehaviour(PlayerEntity playerEntity, CharacterController controller,
            Camera camera)
        {
            _controller = controller;
            _transform = playerEntity.transform;
            _cameraTransform = camera.transform;
            _animator = playerEntity.GetAnimator();
            _verticalVelocity = Physics.gravity.y;

            controller.enabled = true;

            playerEntity.OnMovementVectorChangedEvent += OnMovementVectorChangedEvent;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly CharacterController _controller;
        private readonly Transform _transform;
        private readonly Transform _cameraTransform;
        private readonly Animator _animator;
        private readonly float _verticalVelocity;

        private Vector2 _movementVector;
        private float _rotationVelocity;
        private float _targetRotation;
        private float _animationBlend;
        private float _speed;

        private bool _rotateOnMove = true;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Movement()
        {
            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = 1;
            float speedChangeRate = 10;
            float deltaTime = Time.deltaTime;

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (_movementVector == Vector2.zero)
                targetSpeed = 0f;

            // a reference to the players current horizontal velocity
            Vector3 velocity = _controller.velocity;
            float currentHorizontalSpeed = new Vector3(velocity.x, 0f, velocity.z).magnitude;

            const float speedOffset = 0.1f;
            const float inputMagnitude = 1f;
            bool changeSpeedSmoothly = currentHorizontalSpeed < targetSpeed - speedOffset ||
                                       currentHorizontalSpeed > targetSpeed + speedOffset;

            // accelerate or decelerate to target speed
            if (changeSpeedSmoothly)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, deltaTime * speedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, deltaTime * speedChangeRate);

            // normalise input direction
            Vector3 inputDirection = new Vector3(_movementVector.x, 0f, _movementVector.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (_movementVector != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _cameraTransform.eulerAngles.y;

                float rotation = Mathf.SmoothDampAngle(_transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    10f);

                // rotate to face input direction relative to camera position
                if (_rotateOnMove)
                    _transform.rotation = Quaternion.Euler(0f, rotation, 0.0f);
                
                _animator.SetBool(id: AnimatorHashes.CanMove, value: true);
            }
            else
            {
                _animator.SetBool(id: AnimatorHashes.CanMove, value: false);
            }

            Vector3 targetDirection = Quaternion.Euler(0f, _targetRotation, 0f) * Vector3.forward;

            // new Vector3(0f, _verticalVelocity, 0f) * deltaTime
            Vector3 motion = targetDirection.normalized * (_speed * deltaTime) +
                             new Vector3(0f, _verticalVelocity, 0f) * deltaTime;

            // move the player
            _controller.Move(motion);

            // update animator if using character
            _animator.SetFloat(AnimatorHashes.Speed, _animationBlend);
            _animator.SetFloat(AnimatorHashes.MotionSpeed, inputMagnitude);
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnMovementVectorChangedEvent(Vector2 movementVector) =>
            _movementVector = movementVector;
    }
}