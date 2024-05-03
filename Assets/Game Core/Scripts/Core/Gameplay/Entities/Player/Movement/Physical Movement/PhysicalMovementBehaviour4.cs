using GameCore.Gameplay.InputHandlerTEMP;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Player.Movement
{
    public class PhysicalMovementBehaviour4 : IMovementBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public PhysicalMovementBehaviour4(PlayerEntity playerEntity)
        {
            PlayerReferences playerReferences = playerEntity.References;

            _inputReader = playerReferences.InputReader;
            _movementConfig = playerReferences.PlayerConfig.MovementConfig4;
            _movementInfoDebug = playerReferences.MovementInfoDebug;
            _transform = playerEntity.transform;
            _rigidbody = playerReferences.Rigidbody;
            _collider = playerReferences.Collider;
            _animator = playerReferences.Animator;
            _groundMask = _movementConfig.GroundMask;

            _globalDownDirection = Vector3.down.normalized;

            _inputReader.OnSprintEvent += OnSprint;
            _inputReader.OnSprintCanceledEvent += OnSprintCanceled;
            _inputReader.OnJumpEvent += OnJump;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly InputReader _inputReader;
        private readonly MovementConfig4 _movementConfig;
        private readonly MovementInfoDebug _movementInfoDebug;
        private readonly Transform _transform;
        private readonly Rigidbody _rigidbody;
        private readonly CapsuleCollider _collider;
        private readonly Animator _animator;
        private readonly LayerMask _groundMask;
        private readonly Vector3 _globalDownDirection;
        
        private Vector3 _currentVelocity;
        private Vector3 _globalMoveDirection;
        private bool _isGrounded;
        private bool _performSprint;
        
        private bool _isEnabled;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Tick()
        {
            CheckGrounded();

            ControlDrag();

            GetMoveDirection();
        }

        public void FixedTick()
        {
            Movement();
            ApplyGravity();
        }

        public void Dispose()
        {
        }

        public void ToggleState(bool isEnabled) =>
            _isEnabled = isEnabled;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CheckGrounded()
        {
            float groundedCheckRadius = _movementConfig.GroundedCheckRadius;
            Vector3 position = _transform.position - new Vector3(x: 0, y: _collider.height * 0.5f, z: 0);

            _isGrounded = Physics.CheckSphere(position, groundedCheckRadius, _groundMask);

            _movementInfoDebug.IsGrounded = _isGrounded;
        }

        private void ControlDrag()
        {
            float drag = _isGrounded ? _movementConfig.GroundDrag : _movementConfig.AirDrag;
            _rigidbody.drag = drag;
        }

        private void GetMoveDirection()
        {
            var moveVector = _inputReader.GameInput.Gameplay.Move.ReadValue<Vector2>();

            if (!_isEnabled)
                moveVector = Vector2.zero;

            _globalMoveDirection = _transform.forward * moveVector.y + _transform.right * moveVector.x;

            _movementInfoDebug.GlobalMoveDirection = _globalMoveDirection;
        }

        private void Movement()
        {
            Vector3 targetVelocity;
            float movementThreshold = _movementConfig.MovementThreshold;
            float fdt = Time.fixedDeltaTime;
            float smoothTime;

            const float crouchMultiplier = 1f; // TEMP

            if (_globalMoveDirection.magnitude > movementThreshold)
            {
                float dampSpeedUp = _movementConfig.DampSpeedUp;
                smoothTime = dampSpeedUp;

                if (_performSprint)
                {
                    float sprintSpeed = _movementConfig.SprintSpeed;
                    targetVelocity = _globalMoveDirection * (sprintSpeed * crouchMultiplier);
                }
                else
                {
                    float walkSpeed = _movementConfig.WalkSpeed;
                    smoothTime = dampSpeedUp;
                    targetVelocity = _globalMoveDirection * (walkSpeed * crouchMultiplier);
                }
            }
            else
            {
                float dampSpeedDown = _movementConfig.DampSpeedDown;
                smoothTime = dampSpeedDown;
                targetVelocity = Vector3.zero * crouchMultiplier;
            }
            
            Vector3 velocity = _rigidbody.velocity;
            _rigidbody.velocity = Vector3.SmoothDamp(current: velocity, targetVelocity, ref _currentVelocity,
                smoothTime);

            _movementInfoDebug.CurrentVelocity = _currentVelocity;
        }

        private void ApplyGravity()
        {
            float gravityMultiplier = _movementConfig.GravityMultiplier;
            Vector3 gravity = _globalDownDirection * (gravityMultiplier * -Physics.gravity.y);

            _rigidbody.AddForce(gravity);
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnSprint()
        {
            _performSprint = true;
            
            _movementInfoDebug.PerformSprint = _performSprint;
        }

        private void OnSprintCanceled()
        {
            _performSprint = false;

            _movementInfoDebug.PerformSprint = _performSprint;
        }

        private void OnJump()
        {
        }
    }
}