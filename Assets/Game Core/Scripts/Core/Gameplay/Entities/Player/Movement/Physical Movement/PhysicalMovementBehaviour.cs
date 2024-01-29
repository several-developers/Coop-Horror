using GameCore.Configs.Gameplay.Player;
using GameCore.Gameplay.InputHandlerTEMP;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Player.Movement
{
    public class PhysicalMovementBehaviour : IMovementBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public PhysicalMovementBehaviour(PlayerEntity playerEntity, Transform cameraTransform)
        {
            PlayerReferences playerReferences = playerEntity.References;

            _playerEntity = playerEntity;
            _playerConfig = playerReferences.PlayerConfig;
            _inputReader = playerReferences.InputReader;
            _cameraTransform = cameraTransform;
            _transform = playerEntity.transform;
            _rigidbody = playerReferences.Rigidbody;
            _collider = playerReferences.Collider;

            _currentLockOnSlope = _playerConfig.LockOnSlope;
            _originalColliderHeight = _collider.height;

            _inputReader.OnMoveEvent += OnMove;
            _inputReader.OnJumpEvent += OnJump;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly PlayerEntity _playerEntity;
        private readonly PlayerConfigMeta _playerConfig;
        private readonly InputReader _inputReader;
        private readonly Transform _cameraTransform;
        private readonly Transform _transform;
        private readonly Rigidbody _rigidbody;
        private readonly CapsuleCollider _collider;

        private readonly float _originalColliderHeight;

        private Vector3 _forward;
        private Vector3 _globalForward; // For what?
        private Vector3 _reactionForward; // For what?
        private Vector3 _down; // For what?
        private Vector3 _globalDown; // For what?
        private Vector3 _reactionGlobalDown; // For what?
        
        private float _currentSurfaceAngle;
        private bool _currentLockOnSlope;
        
        private Vector3 _groundNormal;
        private Vector3 _previousGroundNormal;
        private bool _previousGrounded;

        private float _coyoteJumpMultiplier = 1f;
        
        private bool _isGrounded;
        private bool _isCrouching;
        private bool _isSprinting;
        private bool _isJumping;
        private bool _isTouchingSlope;
        private bool _isTouchingStep;
        
        private Vector2 _moveInputVector;
        private bool _jump;

        private bool _hasJumped;

        // For what?
        private Vector3 _currentVelocity = Vector3.zero;
        private float _targetAngle;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Tick()
        {
            _moveInputVector = _inputReader.GameInput.Gameplay.Move.ReadValue<Vector2>();
        }

        public void FixedTick()
        {
            // Checks
            CheckGrounded();
            CheckStep();
            CheckSlopeAndDirections();

            // Movement
            MoveWalk();
            MoveJump();
            
            // Gravity
            ApplyGravity();
            
            CancelJump();
        }

        public void Dispose()
        {
            _inputReader.OnMoveEvent -= OnMove;
            _inputReader.OnJumpEvent -= OnJump;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        #region Checks

        private void CheckGrounded()
        {
            LayerMask groundMask = _playerConfig.GroundMask;
            Vector3 checkPosition = _transform.position - new Vector3(x: 0, y: _originalColliderHeight / 2f, z: 0);
            float groundCheckerThreshold = _playerConfig.GroundCheckerThreshold;

            _previousGrounded = _isGrounded;
            _isGrounded = Physics.CheckSphere(checkPosition, groundCheckerThreshold, groundMask);
        }

        private void CheckStep()
        {
            LayerMask groundMask = _playerConfig.GroundMask;
            float stepCheckerThreshold = _playerConfig.stepCheckerThreshold;
            float maxStepHeight = _playerConfig.MaxStepHeight;
            bool tmpStep = false;
            bool checkRaycast;
            
            Vector3 bottomStepPos = _transform.position
                                    - new Vector3(x: 0f, y: _originalColliderHeight / 2f, z: 0f)
                                    + new Vector3(x: 0f, y: 0.05f, z: 0f);
            
            Vector3 origin = bottomStepPos + new Vector3(x: 0f, y: maxStepHeight, z: 0f);
            float maxDistance = stepCheckerThreshold + 0.05f;

            checkRaycast = Physics.Raycast(origin: bottomStepPos, direction: _globalForward,
                out RaycastHit stepLowerHit, stepCheckerThreshold, groundMask);
            
            if (checkRaycast)
            {
                bool equalsZero = Mathf.Approximately(a: stepLowerHit.normal.y, b: 0f);

                if (equalsZero)
                {
                    checkRaycast = Physics.Raycast(origin, direction: _globalForward, out RaycastHit stepUpperHit,
                        maxDistance, groundMask);
                    
                    if (!checkRaycast)
                    {
                        //rigidbody.position -= new Vector3(0f, -stepSmooth, 0f);
                        tmpStep = true;
                    }
                }
            }

            checkRaycast = Physics.Raycast(origin: bottomStepPos,
                direction: Quaternion.AngleAxis(angle: 45, _transform.up) * _globalForward,
                out RaycastHit stepLowerHit45, stepCheckerThreshold, groundMask);
                
            if (checkRaycast)
            {
                bool equalsZero = Mathf.Approximately(a: stepLowerHit45.normal.y, b: 0f);
                
                if (equalsZero)
                {
                    Vector3 direction = Quaternion.AngleAxis(angle: 45, Vector3.up) * _globalForward;

                    checkRaycast = Physics.Raycast(origin, direction, out RaycastHit stepUpperHit45, maxDistance,
                        groundMask);
                    
                    if (!checkRaycast)
                    {
                        //rigidbody.position -= new Vector3(0f, -stepSmooth, 0f);
                        tmpStep = true;
                    }
                }
            }

            checkRaycast = Physics.Raycast(origin: bottomStepPos,
                direction: Quaternion.AngleAxis(angle: -45, _transform.up) * _globalForward,
                out RaycastHit stepLowerHitMinus45, stepCheckerThreshold, groundMask);
            
            if (checkRaycast)
            {
                bool equalsZero = Mathf.Approximately(a: stepLowerHitMinus45.normal.y, b: 0f);

                if (equalsZero)
                {
                    Vector3 direction = Quaternion.AngleAxis(angle: -45, Vector3.up) * _globalForward;
                    
                    checkRaycast = Physics.Raycast(origin, direction, out RaycastHit stepUpperHitMinus45, maxDistance,
                        groundMask);
                    
                    if (!checkRaycast)
                    {
                        //rigidbody.position -= new Vector3(0f, -stepSmooth, 0f);
                        tmpStep = true;
                    }
                }
            }

            _isTouchingStep = tmpStep;
        }
        
        private void CheckSlopeAndDirections()
        {
            _previousGroundNormal = _groundNormal;

            LayerMask groundMask = _playerConfig.GroundMask;
            float frictionAgainstFloor = _playerConfig.FrictionAgainstFloor;
            float slopeCheckerThreshold = _playerConfig.SlopeCheckerThreshold;
            bool lockOnSlope = _playerConfig.LockOnSlope;

            bool isSloping = Physics.SphereCast(origin: _transform.position, radius: slopeCheckerThreshold,
                direction: Vector3.down, out RaycastHit slopeHit, maxDistance: _originalColliderHeight / 2f + 0.5f,
                groundMask);

            if (isSloping)
            {
                _groundNormal = slopeHit.normal;
                bool equalsOne = Mathf.Approximately(a: slopeHit.normal.y, b: 1f);

                if (equalsOne)
                {
                    _forward = Quaternion.Euler(0f, _targetAngle, 0f) * Vector3.forward;
                    _globalForward = _forward;
                    _reactionForward = _forward;

                    SetFriction(frictionAgainstFloor, true);
                    _currentLockOnSlope = lockOnSlope;

                    _currentSurfaceAngle = 0f;
                    _isTouchingSlope = false;

                }
                else
                {
                    float maxClimbableSlopeAngle = _playerConfig.MaxClimbableSlopeAngle;
                    AnimationCurve speedMultiplierOnAngle = _playerConfig.SpeedMultiplierOnAngle;
                    
                    float speedMultiplier = speedMultiplierOnAngle.Evaluate(time: _currentSurfaceAngle / 90f);
                    
                    // set forward
                    Vector3 tmpGlobalForward = _transform.forward.normalized;
                    
                    Vector3 tmpForward = new(tmpGlobalForward.x, Vector3
                        .ProjectOnPlane(tmpGlobalForward, slopeHit.normal).normalized.y, tmpGlobalForward.z);
                    
                    Vector3 tmpReactionForward = new(tmpForward.x, tmpGlobalForward.y - tmpForward.y, tmpForward.z);

                    if (_currentSurfaceAngle <= maxClimbableSlopeAngle && !_isTouchingStep)
                    {
                        float canSlideMultiplierCurve = _playerConfig.CanSlideMultiplierCurve;
                        
                        // set forward
                        _forward = tmpForward * (speedMultiplier * canSlideMultiplierCurve + 1f);
                        _globalForward = tmpGlobalForward * (speedMultiplier * canSlideMultiplierCurve + 1f);
                        _reactionForward = tmpReactionForward * (speedMultiplier * canSlideMultiplierCurve + 1f);

                        SetFriction(frictionAgainstFloor, isMinimum: true);
                        _currentLockOnSlope = lockOnSlope;
                    }
                    else if (_isTouchingStep)
                    {
                        float climbingStairsMultiplierCurve = _playerConfig.ClimbingStairsMultiplierCurve;
                        
                        // set forward
                        _forward = tmpForward * (speedMultiplier * climbingStairsMultiplierCurve + 1f);
                        _globalForward = tmpGlobalForward * (speedMultiplier * climbingStairsMultiplierCurve + 1f);
                        _reactionForward = tmpReactionForward * (speedMultiplier * climbingStairsMultiplierCurve + 1f);

                        SetFriction(frictionAgainstFloor, isMinimum: true);
                        _currentLockOnSlope = true;
                    }
                    else
                    {
                        float cantSlideMultiplierCurve = _playerConfig.CantSlideMultiplierCurve;
                        
                        // set forward
                        _forward = tmpForward * (speedMultiplier * cantSlideMultiplierCurve + 1f);
                        _globalForward = tmpGlobalForward * (speedMultiplier * cantSlideMultiplierCurve + 1f);
                        _reactionForward = tmpReactionForward * (speedMultiplier * cantSlideMultiplierCurve + 1f);

                        SetFriction(frictionWall: 0f, isMinimum: true);
                        _currentLockOnSlope = lockOnSlope;
                    }

                    _currentSurfaceAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                    _isTouchingSlope = true;
                }

                // set down
                _down = Vector3.Project(Vector3.down, slopeHit.normal);
                _globalDown = Vector3.down.normalized;
                _reactionGlobalDown = Vector3.up.normalized;
            }
            else
            {
                _groundNormal = Vector3.zero;

                Vector3 vector = new(_moveInputVector.x, 0f, _moveInputVector.y); // _transform.forward
                vector = Quaternion.Euler(0f, _targetAngle, 0f) * Vector3.forward;
                
                _forward = Vector3.ProjectOnPlane(vector, slopeHit.normal).normalized;
                _globalForward = _forward;
                _reactionForward = _forward;

                // set down
                _down = Vector3.down.normalized;
                _globalDown = Vector3.down.normalized;
                _reactionGlobalDown = Vector3.up.normalized;

                SetFriction(frictionAgainstFloor, isMinimum: true);
                _currentLockOnSlope = lockOnSlope;
            }
        }

        #endregion

        #region Movement

        private void MoveWalk()
        {
            float movementThreshold = _playerConfig.MovementThreshold;
            float crouchMultiplier = 1f;

            if (_isCrouching)
            {
                float crouchSpeedMultiplier = _playerConfig.CrouchSpeedMultiplier;
                crouchMultiplier = crouchSpeedMultiplier;
            }

            Vector3 current = _rigidbody.velocity;
            Vector3 target;
            bool canMove = _moveInputVector.magnitude > movementThreshold;
            
            if (canMove)
            {
                _targetAngle = Mathf.Atan2(y: _moveInputVector.x, x: _moveInputVector.y) * Mathf.Rad2Deg
                               + _cameraTransform.eulerAngles.y;

                float movementSpeed = _playerConfig.MovementSpeed;
                float sprintSpeed = _playerConfig.SprintSpeed;
                float speed = _isSprinting ? sprintSpeed : movementSpeed;
                
                target = _forward * (speed * crouchMultiplier);
            }
            else
            {
                target = Vector3.zero * crouchMultiplier;
            }
            
            float dampSpeedUp = _playerConfig.DampSpeedUp;
            float dampSpeedDown = _playerConfig.DampSpeedDown;
            float smoothTime = canMove ? dampSpeedUp : dampSpeedDown;
            
            _rigidbody.velocity = Vector3.SmoothDamp(current, target, ref _currentVelocity, smoothTime);
        }
        
        private void MoveJump()
        {
            float maxClimbableSlopeAngle = _playerConfig.MaxClimbableSlopeAngle;
            bool isTouchingWall = false; // TEMP
            
            // Jumped
            bool jumped = _jump
                          && _isGrounded
                          && ((_isTouchingSlope && _currentSurfaceAngle <= maxClimbableSlopeAngle) || !_isTouchingSlope)
                          && !isTouchingWall;
            
            if (jumped)
            {
                float jumpVelocity = _playerConfig.JumpVelocity;
                //_rigidbody.velocity += Vector3.up * jumpVelocity;
                
                _rigidbody.AddForce(Vector3.up * Mathf.Sqrt(2f * jumpVelocity * -Physics.gravity.y),
                    ForceMode.VelocityChange);
                
                _isJumping = true;
            }
            // Jumped from wall
            // else if (jump && !_isGrounded && isTouchingWall)
            // {
            //     _rigidbody.velocity += wallNormal * jumpFromWallMultiplier + (Vector3.up * jumpFromWallMultiplier) * multiplierVerticalLeap;
            //     isJumping = true;
            //
            //     _targetAngle = Mathf.Atan2(wallNormal.x, wallNormal.z) * Mathf.Rad2Deg;
            //
            //     _forward = wallNormal;
            //     _globalForward = _forward;
            //     _reactionForward = _forward;
            // }

            // Is falling
            if (_rigidbody.velocity.y < 0 && !_isGrounded)
            {
                float fallMultiplier = _playerConfig.FallMultiplier;
                _coyoteJumpMultiplier = fallMultiplier;
            }
            else if (_rigidbody.velocity.y > 0.1f && (_currentSurfaceAngle <= maxClimbableSlopeAngle || _isTouchingStep))
            {
                bool jumpHold = false; // TEMP
                bool canLongJump = _playerConfig.CanLongJump;
                
                // Is short jumping
                if (!jumpHold || !canLongJump)
                {
                    _coyoteJumpMultiplier = 1f;
                }
                // Is long jumping
                else
                {
                    float holdJumpMultiplier = _playerConfig.HoldJumpMultiplier;
                    _coyoteJumpMultiplier = 1f / holdJumpMultiplier;
                }
            }
            else
            {
                _isJumping = false;
                _coyoteJumpMultiplier = 1f;
            }
        }

        #endregion
        
        #region Gravity

        private void ApplyGravity()
        {
            float maxClimbableSlopeAngle = _playerConfig.MaxClimbableSlopeAngle;
            float gravityMultiplier = _playerConfig.GravityMultiplier;
            Vector3 gravity;

            if (_currentLockOnSlope || _isTouchingStep)
                gravity = _down * (gravityMultiplier * -Physics.gravity.y * _coyoteJumpMultiplier);
            else
                gravity = _globalDown * (gravityMultiplier * -Physics.gravity.y * _coyoteJumpMultiplier);

            bool equalsOne = Mathf.Approximately(a: _groundNormal.y, b: 1f);
            
            // Avoid little jump
            bool avoidLittleJump = !equalsOne
                                   && _groundNormal.y != 0
                                   && _isTouchingSlope
                                   && _previousGroundNormal != _groundNormal;
            
            if (avoidLittleJump)
            {
                float gravityMultiplierOnSlideChange = _playerConfig.GravityMultiplierOnSlideChange;

                // Debug.Log("Added correction jump on slope");
                gravity *= gravityMultiplierOnSlideChange;
            }

            // Slide if angle too big
            bool canSlide = !equalsOne
                            && _groundNormal.y != 0
                            && _currentSurfaceAngle > maxClimbableSlopeAngle
                            && !_isTouchingStep; 
            
            if (canSlide)
            {
                float gravityMultiplierIfUnclimbableSlope = _playerConfig.GravityMultiplierIfUnclimbableSlope;
                
                // Debug.Log("Slope angle too high, character is sliding");
                if (_currentSurfaceAngle is > 0f and <= 30f)
                {
                    gravity = _globalDown * (gravityMultiplierIfUnclimbableSlope * -Physics.gravity.y);
                }
                else if (_currentSurfaceAngle is > 30f and <= 89f)
                {
                    gravity = _globalDown * gravityMultiplierIfUnclimbableSlope / 2f * -Physics.gravity.y;
                }
            }

            float frictionAgainstWall = _playerConfig.FrictionAgainstWall;
            bool isTouchingWall = false; // TEMP
            
            // Friction when touching wall
            if (isTouchingWall && _rigidbody.velocity.y < 0)
                gravity *= frictionAgainstWall;

            _rigidbody.AddForce(gravity);
        }

        #endregion

        #region Other

        private void CancelJump()
        {
            if (!_hasJumped)
                return;

            _jump = false;
            _hasJumped = false;
        }

        #endregion
        
        #region Friction and Round

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

        #endregion

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnMove(Vector2 moveVector) =>
            _moveInputVector = moveVector;

        private void OnJump()
        {
            _jump = true;
            _hasJumped = true;
        }
    }
}