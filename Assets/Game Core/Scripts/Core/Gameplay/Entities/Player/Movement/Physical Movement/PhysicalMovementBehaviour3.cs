using GameCore.Gameplay.InputHandlerTEMP;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Player.Movement
{
    public class PhysicalMovementBehaviour3 : IMovementBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public PhysicalMovementBehaviour3(PlayerEntity playerEntity)
        {
            PlayerReferences playerReferences = playerEntity.References;

            _playerEntity = playerEntity;
            _inputReader = playerReferences.InputReader;
            _movementConfig = playerReferences.PlayerConfig.MovementConfig3;
            _transform = playerEntity.transform;
            _rigidbody = playerReferences.Rigidbody;
            _collider = playerReferences.Collider;
            _groundMask = _movementConfig.GroundMask;

            _globalDownDirection = Vector3.down.normalized;

            _inputReader.OnSprintEvent += OnSprint;
            _inputReader.OnSprintCanceledEvent += OnSprintCanceled;
            _inputReader.OnJumpEvent += OnJump;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly PlayerEntity _playerEntity;
        private readonly InputReader _inputReader;
        private readonly MovementConfig3 _movementConfig;
        private readonly Transform _transform;
        private readonly Rigidbody _rigidbody;
        private readonly CapsuleCollider _collider;
        private readonly LayerMask _groundMask;

        private Vector2 _moveInput;

        private Vector3 _moveDirection;
        private Vector3 _globalMoveDirection;
        private Vector3 _downDirection;
        private Vector3 _globalDownDirection;
        private Vector3 _slopeNormal;
        private RaycastHit _slopeInfo;

        private float _moveSpeed;
        private float _slopeAngle;

        private bool _performJump;
        private bool _performSprint;

        private bool _isGrounded;
        private bool _isSloping;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Tick()
        {
            CheckGrounded();
            CheckSlopeAndDirections();

            ControlDrag();
            ControlSpeed();

            Jump();

            GetMoveDirection();

            ResetJump();
        }

        public void FixedTick()
        {
            Movement();
            ApplyGravity();
        }

        public void Dispose()
        {
            _inputReader.OnSprintEvent -= OnSprint;
            _inputReader.OnSprintCanceledEvent -= OnSprintCanceled;
            _inputReader.OnJumpEvent -= OnJump;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CheckGrounded()
        {
            const float radius = 0.15f;
            Vector3 position = _transform.position - new Vector3(x: 0, y: _collider.height * 0.5f, z: 0);

            _isGrounded = Physics.CheckSphere(position, radius, _groundMask);
        }

        private void CheckSlopeAndDirections()
        {
            _moveDirection = _globalMoveDirection;

            float frictionAgainstFloor = _movementConfig.FrictionAgainstFloor;
            float maxDistance = _collider.height * 0.5f + 0.5f;

            bool isSloping = Physics.Raycast(origin: _transform.position, direction: Vector3.down, out _slopeInfo,
                maxDistance);

            if (isSloping)
            {
                _slopeNormal = _slopeInfo.normal;
                bool equalsOne = Mathf.Approximately(a: _slopeInfo.normal.y, b: 1f);

                if (equalsOne)
                {
                    _slopeAngle = 0;
                    _isSloping = false;

                    SetFriction(frictionAgainstFloor, isMinimum: true);
                }
                else
                {
                    AnimationCurve speedMultiplierOnAngle = _movementConfig.SpeedMultiplierOnAngle;
                    float maxSlopeAngle = _movementConfig.MaxSlopeAngle;
                    float speedMultiplier = speedMultiplierOnAngle.Evaluate(time: _slopeAngle / 90f);

                    Vector3 slopeMoveDirection = Vector3.ProjectOnPlane(vector: _globalMoveDirection, _slopeNormal);

                    if (_slopeAngle <= maxSlopeAngle)
                    {
                        _moveDirection = slopeMoveDirection * speedMultiplier;

                        SetFriction(frictionAgainstFloor, isMinimum: true);
                    }
                    else
                    {
                        _moveDirection = slopeMoveDirection * speedMultiplier;

                        SetFriction(frictionWall: 0f, isMinimum: true);
                    }

                    _slopeAngle = Vector3.Angle(Vector3.up, _slopeNormal);
                    _isSloping = true;
                }

                _downDirection = Vector3.ProjectOnPlane(Vector3.down, _slopeNormal);
            }
            else
            {
                _downDirection = Vector3.down.normalized;
                _slopeNormal = Vector3.zero;
                _isSloping = false;

                SetFriction(frictionAgainstFloor, isMinimum: true);
            }
        }

        private void ControlDrag()
        {
            float drag = _isGrounded ? _movementConfig.GroundDrag : _movementConfig.AirDrag;
            _rigidbody.drag = drag;
        }

        private void ControlSpeed()
        {
            float acceleration = _movementConfig.Acceleration;
            float t = acceleration * Time.deltaTime;

            if (_performSprint && _isGrounded)
            {
                float sprintSpeed = _movementConfig.SprintSpeed;
                _moveSpeed = Mathf.Lerp(_moveSpeed, sprintSpeed, t);
            }
            else
            {
                float walkSpeed = _movementConfig.WalkSpeed;
                _moveSpeed = Mathf.Lerp(_moveSpeed, walkSpeed, t);
            }
        }

        private void Movement()
        {
            float moveSpeedMultiplier = _movementConfig.MoveSpeedMultiplier;
            float additionalMultiplier = 1f;
            float fdt = Time.fixedDeltaTime;

            if (!_isGrounded)
            {
                float airMultiplier = _movementConfig.MoveSpeedAirMultiplier;
                additionalMultiplier = airMultiplier;
            }

            Vector3 force = _moveDirection.normalized * (_moveSpeed * moveSpeedMultiplier * additionalMultiplier * fdt);
            _rigidbody.AddForce(force, ForceMode.Acceleration);
        }

        private void Jump()
        {
            if (!_performJump || !_isGrounded)
                return;

            float jumpForce = _movementConfig.JumpForce;
            Vector3 force = _transform.up * Mathf.Sqrt(jumpForce * -2f * Physics.gravity.y);
            Vector3 velocity = _rigidbody.velocity;

            _rigidbody.velocity = new Vector3(velocity.x, 0f, velocity.z);
            _rigidbody.AddForce(force, ForceMode.VelocityChange);
        }

        private void ResetJump() =>
            _performJump = false;

        private void GetMoveDirection()
        {
            var moveVector = _inputReader.GameInput.Gameplay.Move.ReadValue<Vector2>();
            _globalMoveDirection = _transform.forward * moveVector.y + _transform.right * moveVector.x;
        }

        private void ApplyGravity()
        {
            float maxSlopeAngle = _movementConfig.MaxSlopeAngle;
            float gravityMultiplier = _movementConfig.GravityMultiplier;
            Vector3 gravity = Vector3.zero;

            if (!_isSloping)
                gravity = _globalDownDirection * (gravityMultiplier * -Physics.gravity.y);

            // Slide if angle too big
            bool flag1 = !Mathf.Approximately(a: _slopeNormal.y, b: 0f);
            bool flag2 = _slopeAngle > maxSlopeAngle;
            bool canSlide = flag1 && flag2;

            if (canSlide)
            {
                float gravityMultiplierIfUnclimbableSlope = _movementConfig.GravityMultiplierIfUnclimbableSlope;

                if (_slopeAngle is > 0f and <= 30f)
                {
                    gravity = _globalDownDirection * (gravityMultiplierIfUnclimbableSlope * -Physics.gravity.y);
                }
                else if (_slopeAngle is > 30f and <= 89f)
                {
                    gravity = _globalDownDirection * (gravityMultiplierIfUnclimbableSlope * 0.5f * -Physics.gravity.y);
                }
            }

            _rigidbody.AddForce(gravity);
        }

        private void SetFriction(float frictionWall, bool isMinimum)
        {
            PhysicMaterial physicMaterial = _collider.material;
            physicMaterial.dynamicFriction = 0.6f * frictionWall;
            physicMaterial.staticFriction = 0.6f * frictionWall;

            PhysicMaterialCombine materialCombine = isMinimum
                ? PhysicMaterialCombine.Minimum
                : PhysicMaterialCombine.Maximum;

            physicMaterial.frictionCombine = materialCombine;
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnSprint() =>
            _performSprint = true;

        private void OnSprintCanceled() =>
            _performSprint = false;

        private void OnJump() =>
            _performJump = true;
    }
}