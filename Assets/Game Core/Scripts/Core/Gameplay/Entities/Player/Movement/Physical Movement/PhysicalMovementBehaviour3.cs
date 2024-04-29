using GameCore.Gameplay.InputHandlerTEMP;
using GameCore.Utilities;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Player.Movement
{
    public class PhysicalMovementBehaviour3 : IMovementBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public PhysicalMovementBehaviour3(PlayerEntity playerEntity)
        {
            PlayerReferences playerReferences = playerEntity.References;

            _inputReader = playerReferences.InputReader;
            _movementConfig = playerReferences.PlayerConfig.MovementConfig3;
            _transform = playerEntity.transform;
            _rigidbody = playerReferences.Rigidbody;
            _collider = playerReferences.Collider;
            _animator = playerReferences.Animator;
            _groundMask = _movementConfig.GroundMask;

            _jumpComponent = new JumpComponent(playerEntity, movementBehaviour: this);

            _globalDownDirection = Vector3.down.normalized;

            _inputReader.OnSprintEvent += OnSprint;
            _inputReader.OnSprintCanceledEvent += OnSprintCanceled;
            _inputReader.OnJumpEvent += OnJump;
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public bool IsGrounded => _isGrounded;
        
        // FIELDS: --------------------------------------------------------------------------------

        private readonly InputReader _inputReader;
        private readonly MovementConfig3 _movementConfig;
        private readonly JumpComponent _jumpComponent;
        private readonly Transform _transform;
        private readonly Rigidbody _rigidbody;
        private readonly CapsuleCollider _collider;
        private readonly Animator _animator;
        private readonly LayerMask _groundMask;

        private Vector2 _moveInput;

        private Vector3 _moveDirection;
        private Vector3 _globalMoveDirection;
        private Vector3 _downDirection;
        private Vector3 _globalDownDirection;
        
        private RaycastHit _groundHitInfo;
        private Vector3 _groundPoint;
        private Vector3 _groundNormal;

        private float _moveSpeed;
        private float _slopeAngle;

        private bool _performSprint;

        private bool _isGrounded;
        private bool _isSloping;
        private bool _isEnabled;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Tick()
        {
            CheckGrounded();
            CheckSlopeAndDirections();

            ControlDrag();
            ControlSpeed();

            if (_isEnabled)
                _jumpComponent.Jump();

            GetMoveDirection();

            _jumpComponent.ResetJump();
        }

        public void FixedTick()
        {
            Movement();
            AdditionalMovement();
            ApplyGravity();
        }

        public void Dispose()
        {
            _inputReader.OnSprintEvent -= OnSprint;
            _inputReader.OnSprintCanceledEvent -= OnSprintCanceled;
            _inputReader.OnJumpEvent -= OnJump;
        }

        public void ToggleState(bool isEnabled) =>
            _isEnabled = isEnabled;

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

            bool isSloping = Physics.Raycast(origin: _transform.position, direction: Vector3.down, out _groundHitInfo,
                maxDistance, _groundMask);

            if (isSloping)
            {
                _groundNormal = _groundHitInfo.normal;
                _groundPoint = _groundHitInfo.point;
                
                bool equalsOne = Mathf.Approximately(a: _groundHitInfo.normal.y, b: 1f);

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

                    Vector3 slopeMoveDirection = Vector3.ProjectOnPlane(vector: _globalMoveDirection, _groundNormal);

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

                    _slopeAngle = Vector3.Angle(Vector3.up, _groundNormal);
                    _isSloping = true;
                }

                _downDirection = Vector3.ProjectOnPlane(Vector3.down, _groundNormal);
            }
            else
            {
                _downDirection = Vector3.down.normalized;
                _groundNormal = Vector3.zero;
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

            float forceMagnitude = Mathf.Min(_moveDirection.normalized.magnitude, 0.51f);
            bool canMove = forceMagnitude > 0.05f;
            
            _animator.SetFloat(id: AnimatorHashes.MoveSpeedBlend, forceMagnitude);
            _animator.SetBool(id: AnimatorHashes.CanMove, canMove);
        }

        private void AdditionalMovement()
        {
            if (!_isGrounded)
                return;

            Rigidbody rigidbody = _groundHitInfo.rigidbody;
            bool hitRigidbody = rigidbody != null && rigidbody.isKinematic;

            if (!hitRigidbody)
                return;
            
            float moveSpeedMultiplier = _movementConfig.MoveSpeedMultiplier;
            float fdt = Time.fixedDeltaTime;
            
            Vector3 velocity = rigidbody.GetVelocityAtPoint(_groundPoint);
            //_rigidbody.velocity += velocity;
            
            velocity *= moveSpeedMultiplier * fdt;
            _rigidbody.AddForce(velocity, ForceMode.Acceleration);
        }
        
        private void GetMoveDirection()
        {
            var moveVector = _inputReader.GameInput.Gameplay.Move.ReadValue<Vector2>();

            if (!_isEnabled)
                moveVector = Vector2.zero;
            
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
            bool flag1 = !Mathf.Approximately(a: _groundNormal.y, b: 0f);
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
            _jumpComponent.ActivateJump();
    }
}