using GameCore.Utilities;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Player.Movement
{
    public class MovementComponent
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public MovementComponent(PlayerEntity playerEntity, PhysicalMovementBehaviour2 movementBehaviour)
        {
            PlayerReferences playerReferences = playerEntity.References;
            MovementConfig2 movementConfig = playerReferences.PlayerConfig.MovementConfig2;

            _movementBehaviour = movementBehaviour;
            _movementComponentConfig = movementConfig.MovementComponentConfig;
            _transform = playerEntity.transform;
            _rigidbody = playerReferences.Rigidbody;
            _collider = playerReferences.Collider;

            _groundedPhysMaterial = movementConfig.GroundedPhysMaterial;
            _stayPhysMaterial = movementConfig.StayPhysMaterial;
            _flyPhysMaterial = movementConfig.FlyPhysMaterial;
            _groundMask = _movementComponentConfig.GroundMask;
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public float YVelocity
        {
            get => _yVelocity;
            set => _yVelocity = value;
        }
        
        public float SlopeAngle => _slopeAngle;
        public bool IsGrounded => _isGrounded;
        
        // FIELDS: --------------------------------------------------------------------------------

        private readonly PhysicalMovementBehaviour2 _movementBehaviour;
        private readonly MovementComponentConfig _movementComponentConfig;
        private readonly Transform _transform;
        private readonly Rigidbody _rigidbody;
        private readonly CapsuleCollider _collider;
        private readonly PhysicMaterial _groundedPhysMaterial;
        private readonly PhysicMaterial _stayPhysMaterial;
        private readonly PhysicMaterial _flyPhysMaterial;
        private readonly LayerMask _groundMask;

        private JumpComponent _jumpComponent;

        private Vector3 _lastOnGroundPosition;

        private RaycastHit _groundHitInfo;
        private Vector3 _groundNormal;
        private Vector3 _groundPoint;

        private Vector3 _playerBottomPos;

        private Vector2 _moveInput;
        private float _inputX;
        private float _inputY;

        private float _inputXSmoothed; // Inputs smoothed using lerps.
        private float _inputYSmoothed;
        private float _inputYLerpSpeed;
        private float _inputXLerpSpeed;

        private float _speed; // Combined axis speed of player movement.
        private float _speedAmtX = 1f; // Current player speed per axis which is applied to rigidbody velocity vector.
        private float _speedAmtY = 1f;
        private float _crouchSpeedAmt = 1f;
        private float _moveSpeedMultiplier = 1f;
        private float _currentSpeedMultiplier = 1f;
        private float _speedMultiplier = 1f;
        private float _limitStrafeSpeed;
        private float _yVelocity;
        private int _maxVelocityChange = 5; // maximum rate that player velocity can change
        private Vector3 _velocity; // Total movement velocity vector.
        private Vector3 _playerAirVelocity;

        private float _slopeAngle;
        private float _lastOnGroundPositionTime;

        // The Y coordinate that the player lost grounding and started to fall.
        private float _fallStartLevel;

        private Vector3 _moveDirection; // Movement velocity vector, modified by other speed factors like walk, zoom,
        // and crouch states.

        private Vector3 _moveDirection2D;
        private Vector3 _velocityChange;
        private Vector3 _futureDirection;

        private float _airTime; // Total time that player is airborn.
        private bool _airTimeState;

        private bool _canStep;

        private bool _isGrounded;
        private bool _isFalling;
        private bool _isSprinting;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Init()
        {
            _jumpComponent = _movementBehaviour.JumpComponent;
        }

        public void FixedTick()
        {
            float t = Time.time;
            float dt = Time.deltaTime;
            float fdt = Time.fixedDeltaTime;

            float inputLerpSpeed = _movementComponentConfig.InputLerpSpeed;

            _playerBottomPos = GetPlayerBottomPosition();
            _velocity = _rigidbody.velocity;
            _inputY = _moveInput.y;
            _inputXLerpSpeed = _inputYLerpSpeed = inputLerpSpeed;

            if (_isSprinting)
            {
                // Only strafe when sprinting after pressing the joystick horizontally further than 0.4,
                // for better control and less zig-zagging.
                if (Mathf.Abs(_moveInput.x) > 0.4f)
                {
                    _inputX = Mathf.Sign(_moveInput.x);
                }
                else
                {
                    _inputX = 0f;
                }
            }
            else
            {
                // cancel horizontal movement if player is looking up while prone
                _inputX = _moveInput.x;
            }

            // Smooth our movement inputs using Mathf.Lerp
            _inputXSmoothed = Mathf.Lerp(_inputXSmoothed, _inputX, dt * _inputXLerpSpeed);
            _inputYSmoothed = Mathf.Lerp(_inputYSmoothed, _inputY, dt * _inputYLerpSpeed);

            float walkSpeed = _movementComponentConfig.WalkSpeed;

            if (_speed > walkSpeed * _moveSpeedMultiplier + 0.1f)
            {
                _speed -= 15f * dt; // gradually decelerate to walk speed
            }
            else if (_speed < walkSpeed * _moveSpeedMultiplier - 0.1f)
            {
                _speed += 15f * dt; // gradually accelerate to walk speed
            }

            if (_isGrounded)
            {
                // Reset airTimeState var so that airTime will only be set once when player looses grounding.
                _airTimeState = true;

                // Reset falling state and perform actions if player has landed from a fall.
                if (_isFalling)
                {
                    _playerAirVelocity = Vector3.zero;
                    _jumpComponent.LandStartTime = t; // Track the time when player landed.

                    if (_fallStartLevel - _transform.position.y > 2f)
                    {
                        //Player.CameraAnimator.SetTrigger(CameraAnimNames.Land);
                        //OnLand?.Invoke();
                    }

                    float dist = _fallStartLevel - _transform.position.y;

                    // track the distance of the fall and apply damage if over falling threshold
                    // bool doFallDamage = _fallDamageComponent.FallDamageConfig.allowFallDamage
                    //                     && _transform.position.y < _fallStartLevel - _fallDamageComponent.FallDamageConfig.fallDamageThreshold
                    //                     && !_swimmingComponent.InWater;
                    //
                    // if (doFallDamage)
                    //     _fallDamageComponent.CalculateFallingDamage(dist);

                    // active platform
                    Collider currentGroundCollider = _groundHitInfo.collider;

                    if (currentGroundCollider != null)
                    {
                        Rigidbody groundRigidbody = currentGroundCollider.attachedRigidbody;

                        if (groundRigidbody != null && !groundRigidbody.isKinematic)
                        {
                            groundRigidbody.AddForceAtPosition(Vector3.down * _rigidbody.mass, _groundPoint,
                                ForceMode.Acceleration);
                        }
                    }

                    _isFalling = false;
                }

                // Limit move speed if backpedaling.
                if (_inputY >= 0)
                {
                    if (_speedAmtY < 1f)
                        _speedAmtY += dt; // gradually increase speedAmtY to neutral
                }
                else
                {
                    float backwardSpeedPercentage = _movementComponentConfig.BackwardSpeedPercentage;

                    if (_speedAmtY > backwardSpeedPercentage)
                        _speedAmtY -= dt; // gradually decrease speedAmtY to backpedal limit value
                }

                // Allow limiting of move speed if strafing directly sideways and not diagonally.
                if (_inputX == 0f || _speedAmtY != 0f)
                {
                    if (_speedAmtX < 1f)
                        _speedAmtX += dt; // gradually increase speedAmtX to neutral
                }
                else
                {
                    float strafeSpeedPercentage = _movementComponentConfig.StrafeSpeedPercentage;

                    if (_speedAmtX > strafeSpeedPercentage)
                        _speedAmtX -= dt; // gradually decrease speedAmtX to strafe limit value
                }
            }
            // Player is airborn
            else
            {
                // keep track of the time that player lost grounding for air manipulation and moving gun while jumping
                if (_airTimeState)
                {
                    _airTime = t;
                    _airTimeState = false;
                }

                // subtract height we began falling from current position to get falling distance
                //FallingDistance = _fallStartLevel - transform.position.y; // this value referenced in other scripts

                if (!_isFalling)
                {
                    _isFalling = true;

                    // Start tracking altitude (y position) for fall check.
                    _fallStartLevel = _transform.position.y;
                }
            }

            // Player velocity

            float diagonalStrafeAmt = _movementComponentConfig.DiagonalStrafeAmt;

            // limit speed if strafing diagonally
            _limitStrafeSpeed = Mathf.Abs(_inputX) > 0.5f && Mathf.Abs(_inputY) > 0.5f ? diagonalStrafeAmt : 1f;

            // We are grounded, so recalculate move direction directly from axes	
            _moveDirection = new Vector3(_inputXSmoothed * _limitStrafeSpeed, 0f, _inputYSmoothed * _limitStrafeSpeed);

            // realign moveDirection vector to world space
            _moveDirection = _transform.TransformDirection(_moveDirection);

            CheckGround(_moveDirection.normalized);

            PhysicMaterial physMaterial = _moveDirection.magnitude > 0.1f ? _groundedPhysMaterial : _stayPhysMaterial;
            _collider.material = _isGrounded ? physMaterial : _flyPhysMaterial;

            _canStep = CanStep(_moveDirection.normalized);

            if (_isGrounded)
            {
                if (!_jumpComponent.IsJumping)
                    _yVelocity = 0f;

                _moveDirection = _isSprinting
                    ? _futureDirection
                    : Vector3.Project(_moveDirection, _futureDirection);
            }
            else
            {
                float gravityMax = _movementComponentConfig.GravityMax;

                if (!_yVelocity.IsEquals(gravityMax))
                {
                    float gravityMultiplier = _movementComponentConfig.GravityMultiplier;
                    float value = _yVelocity + Physics.gravity.y * gravityMultiplier * fdt;
                    _yVelocity = Mathf.Clamp(value, -gravityMax, gravityMax);
                }

                if (_moveDirection.magnitude > 0f)
                {
                    float airSpeedMultiplier = _movementComponentConfig.AirSpeedMultiplier;
                    _playerAirVelocity = _moveDirection * airSpeedMultiplier;
                }
                else
                {
                    _playerAirVelocity = Vector3.Lerp(_playerAirVelocity, Vector3.zero, 5f * fdt);
                }

                _moveDirection = _playerAirVelocity;
            }

            _currentSpeedMultiplier = 1f;

            if (_isGrounded && !_canStep)
            {
                AnimationCurve slopeSpeed = _movementComponentConfig.SlopeSpeed;
                float slopeLimit = _movementComponentConfig.SlopeLimit;

                _currentSpeedMultiplier = slopeSpeed.Evaluate(_slopeAngle / slopeLimit);
                _currentSpeedMultiplier *= CanMoveToSlope() ? 1f : 0f;
            }

            // apply speed limits to moveDirection vector
            float currSpeed = _speed * _speedAmtX * _speedAmtY * _crouchSpeedAmt * _currentSpeedMultiplier *
                              _speedMultiplier;

            _moveDirection *= currSpeed;
            _moveDirection += Vector3.up * _yVelocity;
            _moveDirection2D = new Vector3(_moveDirection.x, 0f, _moveDirection.y);

            // apply a force that attempts to reach target velocity
            _velocityChange = _moveDirection - _velocity;

            // limit max speed
            _velocityChange.x = Mathf.Clamp(_velocityChange.x, -_maxVelocityChange, _maxVelocityChange);
            _velocityChange.z = Mathf.Clamp(_velocityChange.z, -_maxVelocityChange, _maxVelocityChange);

            // Finally, add movement velocity to player rigidbody velocity.
            if (!_velocityChange.IsZero())
                _rigidbody.AddForce(_velocityChange, ForceMode.VelocityChange);

            bool hitPlatform = _isGrounded
                               && !_jumpComponent.IsJumping
                               && _groundHitInfo.rigidbody != null
                               && _groundHitInfo.rigidbody.isKinematic;

            if (hitPlatform)
            {
                //Rigidbody.velocity += GroundHitInfo.rigidbody.GetVelocityAtPoint(_groundPoint);
                Vector3 velocity = _groundHitInfo.rigidbody.GetVelocityAtPoint(_groundPoint);

                _rigidbody.velocity += velocity;
            }
        }

        public void SetMoveInput(Vector2 moveVector) =>
            _moveInput = moveVector;

        // TEMP
        public void ToggleSprinting(bool isSprinting) =>
            _isSprinting = isSprinting;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CheckGround(Vector3 direction)
        {
            float radius = _collider.radius;
            Vector3 origin = _playerBottomPos + Vector3.up * (radius + 0.2f);

            _slopeAngle = 0f;
            _isGrounded = false;

            Vector3 castDirection = -_transform.up;
            float maxDistance = _jumpComponent.IsJumping || _isFalling ? 0.3f : 0.75f;

            bool sphereCast = Physics.SphereCast(origin, radius, castDirection, out RaycastHit hit, maxDistance,
                _groundMask.value, QueryTriggerInteraction.Ignore);

            if (sphereCast)
            {
                _isGrounded = true;
                _groundHitInfo = hit;
                _groundNormal = hit.normal;
                _groundPoint = hit.point;

                if (_lastOnGroundPositionTime < Time.time)
                {
                    _lastOnGroundPosition = _groundPoint;
                    _lastOnGroundPositionTime = Time.time + 1f;
                }

                CheckSlope(direction);

                bool applyForce = _playerBottomPos.y - hit.point.y > 0.01f
                                  && _rigidbody.velocity.y <= 0.01f;
                if (!applyForce)
                    return;

                Vector3 newForce = 30f * Physics.gravity.y * Time.fixedDeltaTime * _transform.up;

                if (newForce.IsZero())
                    return;

                _rigidbody.AddForce(newForce, ForceMode.VelocityChange);
            }
        }

        private void CheckSlope(Vector3 direction)
        {
            _futureDirection = direction.magnitude > 0f ? GetMoveDirection(direction).normalized : Vector3.zero;
            _slopeAngle = Vector3.Angle(_futureDirection, _transform.up);

            if (direction.magnitude > 0f)
                _slopeAngle = 90f - _slopeAngle;

            //if (_debug)
            //Debug.DrawLine(PlayerBottomPos, PlayerBottomPos + _futureDirection, Color.blue, 5f);
        }

        private Vector3 GetMoveDirection(Vector3 direction)
        {
            float radius = _collider.radius * 2f;
            float distance = radius + 0.2f;
            float maxDistance = distance + 0.5f;
            Vector3 origin = _playerBottomPos + Vector3.up * distance;
            Vector3 newGroundNormal = _groundNormal;

            bool groundIsStatic = true;

            bool sphereCast = Physics.SphereCast(origin, radius, -_transform.up, out RaycastHit hit,
                maxDistance, _groundMask.value, QueryTriggerInteraction.Ignore);

            if (sphereCast)
            {
                Rigidbody rigidbody = hit.collider.gameObject.GetComponent<Rigidbody>();
                groundIsStatic = rigidbody == null || rigidbody.isKinematic;
                newGroundNormal = hit.normal;
            }

            if (!groundIsStatic)
                return direction;

            if (Vector3.Angle(Vector3.up, newGroundNormal) > 89f)
                return direction + newGroundNormal;

            Vector3 finalDirection = -Vector3.Cross(direction, newGroundNormal).normalized;
            finalDirection = Vector3.Cross(finalDirection, newGroundNormal).normalized;

            return finalDirection;
        }

        private Vector3 GetPlayerBottomPosition() =>
            _transform.position + -_transform.up * (_collider.height * 0.5f);

        private bool CanStep(Vector3 direction)
        {
            if (!_isGrounded)
                return false;

            float stepHeightRadius = _movementComponentConfig.StepHeightRadius;
            float maxStepHeight = _movementComponentConfig.MaxStepHeight;

            float radius = _collider.radius;
            float stepDistance = radius + radius * stepHeightRadius;
            Vector3 maxStepPos = _playerBottomPos + direction * -0.1f + Vector3.up * (maxStepHeight + radius);

            bool sphereCast = Physics.SphereCast(maxStepPos, radius, direction.normalized, out _, stepDistance,
                _groundMask.value, QueryTriggerInteraction.Ignore);

            if (sphereCast)
                return false;

            Vector3 slopeOrigin = maxStepPos + direction * stepDistance;
            float maxDistance = (maxStepPos.y - _playerBottomPos.y) * 2f;

            bool raycast = Physics.Raycast(slopeOrigin, -Vector3.up, out RaycastHit hitInfo, maxDistance,
                _groundMask.value, QueryTriggerInteraction.Ignore);

            if (raycast)
            {
                float slopeLimit = _movementComponentConfig.SlopeLimit;

                if (Vector3.Angle(hitInfo.normal, Vector3.up) < slopeLimit)
                    return true;
            }
            else
            {
                return true;
            }

            return false;
        }

        private bool CanMoveToSlope()
        {
            if (_futureDirection.magnitude > 0f && _futureDirection.y <= 0f)
                return true;

            //if (_debug)
            //Debug.DrawLine(_groundPoint + (Vector3.up * 0.1f),
            //_groundPoint + (Vector3.up * 0.1f) + (-Vector3.up * 0.2f), Color.magenta, 10f);

            bool raycast = Physics.Raycast(_groundPoint + (Vector3.up * 0.1f), -Vector3.up,
                out RaycastHit hitInfo, 0.2f, _groundMask.value, QueryTriggerInteraction.Ignore);

            if (raycast)
            {
                if (hitInfo.point.y < _playerBottomPos.y)
                    return true;

                float slopeLimit = _movementComponentConfig.SlopeLimit;

                return Vector3.Angle(hitInfo.normal, Vector3.up) < slopeLimit;
            }

            return true;
        }
    }
}