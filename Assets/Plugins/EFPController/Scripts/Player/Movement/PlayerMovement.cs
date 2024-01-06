using System;
using EFPController.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace EFPController
{
    [DefaultExecutionOrder(-10)]
    public class PlayerMovement : MonoBehaviour
    {
        public bool _debug;
        public CapsuleCollider capsule;

        [Tooltip("Mask of layers that player will detect collisions with.")]
        public LayerMask groundMask = Game.LayerMask.Default;

        public float _inputLerpSpeed = 90f; // movement lerp speed
        public PhysicMaterial groundedPhysMaterial;
        public PhysicMaterial stayPhysMaterial;
        public PhysicMaterial flyPhysMaterial;
        
        [SerializeField]
        private float _gravityMultiplier = 2f;
        
        [SerializeField]
        private float _gravityMax = 30f;
        
        [SerializeField]
        private float _airSpeedMultiplier = 0.5f;

        [Tooltip("Speed that player moves when walking.")]
        public float walkSpeed = 4f;

        [Tooltip("Percentage to decrease movement speed when strafing diagonally.")]
        public float diagonalStrafeAmt = 0.7071f;

        [Tooltip("Percentage to decrease movement speed when moving backwards.")]
        public float backwardSpeedPercentage = 0.6f; // percentage to decrease movement speed while moving backwards

        // percentage to decrease movement speed while strafing directly left or right
        [Tooltip("Percentage to decrease movement speed when strafing directly left or right.")]
        public float strafeSpeedPercentage = 0.8f;

        [Tooltip("Angle of ground surface that player won't be allowed to move over."), Range(0f, 90f)]
        public int
            slopeLimit = 40; // the maximum allowed ground surface/normal angle that the player is allowed to climb

        public AnimationCurve slopeSpeed = new()
            { keys = new Keyframe[] { new(0f, 1f), new(1f, 0f) } };

        [Tooltip("Maximum height of step that will be climbed.")]
        public float maxStepHeight = 0.8f;

        public float stepHeightRadius = 1.6f;

        [Tooltip("Y position/height of camera when standing.")]
        public float standingCamHeight = 0.89f;

        [Tooltip("Height of player capsule while standing.")]
        public float standingCapsuleHeight = 2f;

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Crouching
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [Header("Crouching")]
        [SerializeField, Required]
        private PlayerCrouchingConfig _playerCrouchingConfig;

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Jumping
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [Header("Jumping")]
        [SerializeField, Required]
        private PlayerJumpingConfig _playerJumpingConfig;

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Sprinting
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public enum SprintType
        {
            Hold = 0,
            Toggle = 1,
            Both = 2
        }

        [Header("Sprinting")]
        [SerializeField, Required]
        private PlayerSprintConfig _playerSprintConfig;
        
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Dash
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [Header("Dash")]
        [SerializeField, Required]
        private PlayerDashConfig _playerDashConfig;
     
        public AudioClip[] dashSounds;

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Swimming
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [Header("Swimming")]
        [SerializeField, Required]
        private PlayerSwimmingConfig _playerSwimmingConfig;
        
        public AudioSource underwaterAudioSource;
        public AudioSource swimmingAudioSource;
        public AudioClip swimOnSurfaceSound;
        public AudioClip swimUnderSurfaceSound;
        public AudioClip[] fallInWaterSounds;
        public AudioClip fallInDeepWaterSound;
        public AudioClip[] divingInSounds;
        public AudioClip[] divingOutSounds;

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Leaning
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [Header("Leaning")]
        [SerializeField, Required]
        private PlayerLeaningConfig _playerLeaningConfig;
        
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Climbing
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [Header("Climbing")]
        [SerializeField, Required]
        private PlayerClimbingConfig _playerClimbingConfig;
        
        public AudioSource _climbingAudioSource;

        public enum ClimbingState
        {
            None,
            Grabbing,
            Grabbed,
            Releasing
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Fall Damage
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [Header("Fall Damage")]
        [SerializeField, Required]
        private PlayerFallDamageConfig _playerFallDamageConfig;

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public bool IsGrounded { get; private set; } // True when capsule cast hits ground surface.
        public Vector3 Velocity { get; private set; } = Vector3.zero; // Total movement velocity vector.
        public float FallingDistance { get; private set; } // total distance that player has fallen

        public bool IsFalling
        {
            get => _isFalling;
            set
            {
                _isFalling = value;
                FallingDistance = 0f;
            }
        }

        // Camera vertical position which is passed to VerticalBob and HorizontalBob.
        public float MidPos { get; private set; } = 0.9f;

        public bool IsMoving { get; private set; }
        public float SpeedMult { get; set; } = 1f;

        // Vlad, set was private
        public float
            CamDampSpeed { get; set; } // variable damp speed for vertical camera smoothing for stance changes

        public Vector3 CamPos { get; private set; }
        public Vector3 EyePos { get; private set; }

        public Vector3 MoveDirection { get; private set; } =
            Vector3.zero; // movement velocity vector, modified by other speed factors like walk, zoom, and crouch states

        public Vector3 MoveDirection2D { get; private set; } = Vector3.zero;
        public Vector3 PlayerBottomPos { get; private set; }
        public Vector3 PlayerMiddlePos { get; private set; }
        public Vector3 PlayerTopPos { get; private set; }
        public Vector3 LastOnGroundPosition { get; private set; }
        public RaycastHit GroundHitInfo { get; private set; }
        public bool HoverClimbable { get; set; } // Vlad, set was private
        public float YVelocity { get; set; } // Vlad, was private field mb

        private bool _canStep;
        private Vector3 _playerAirVelocity;
        private float _crouchSpeedAmt = 1f;
        private float _speedAmtY = 1f; // current player speed per axis which is applied to rigidbody velocity vector
        private float _speedAmtX = 1f;
        private float _zoomSpeedAmt = 1f;
        private float _speed; // combined axis speed of player movement
        private float _limitStrafeSpeed = 0f;
        private bool _airTimeState;
        private float _moveSpeedMult = 1f;
        private float _currSpeedMult = 1f;
        private int _maxVelocityChange = 5; // maximum rate that player velocity can change
        private Vector3 _velocityChange = Vector3.zero;
        private Vector3 _futureDirection = Vector3.zero;
        private Vector3 _groundNormal;
        private Vector3 _groundPoint;
        public float SlopeAngle { get; private set; } // Vlad, was private field mb
        private float _lastOnGroundPositionTime;

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Player Input
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public float InputX { get; set; } // 1 = button pressed 0 = button released.
        public float InputY { get; set; }

        private float _inputXSmoothed; // Inputs smoothed using lerps.
        private float _inputYSmoothed;
        private float _inputYLerpSpeed;
        private float _inputXLerpSpeed;

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // References
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public InputManager InputManager { get; set; }
        public Rigidbody Rigidbody { get; set; }
        public Player Player { get; set; }
        public Transform MainCameraTransform { get; set; }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Events
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public event Action<bool> OnWater;
        public event Action OnJump;
        public event Action OnLand;
        public event Action<bool> OnCrouch;
        public event Action<int> OnFallDamage;

        // PROPERTIES: ----------------------------------------------------------------------------

        public PlayerJumpingComponent JumpingComponent => _jumpingComponent;
        public PlayerCrouchingComponent CrouchingComponent => _crouchingComponent;
        public PlayerClimbingComponent ClimbingComponent => _climbingComponent;
        public PlayerLeaningComponent LeaningComponent => _leaningComponent;
        public PlayerSwimmingComponent SwimmingComponent => _swimmingComponent;
        public PlayerDashComponent DashComponent => _dashComponent;
        public PlayerSprintComponent SprintComponent => _sprintComponent;

        // FIELDS: --------------------------------------------------------------------------------

        private PlayerJumpingComponent _jumpingComponent;
        private PlayerCrouchingComponent _crouchingComponent;
        private PlayerClimbingComponent _climbingComponent;
        private PlayerLeaningComponent _leaningComponent;
        private PlayerSwimmingComponent _swimmingComponent;
        private PlayerDashComponent _dashComponent;
        private PlayerSprintComponent _sprintComponent;
        private PlayerFallDamageComponent _fallDamageComponent;
        private Transform _transform;
        
        // The y coordinate that the player lost grounding and started to fall.
        private float _fallStartLevel;
        
        private float _airTime; // Total time that player is airborn.
        private bool _isFalling; // True when player is losing altitude.

        // GAME ENGINE METHODS: -------------------------------------------------------------------
        
        private void OnEnable()
        {
            Rigidbody = GetComponent<Rigidbody>();
            Rigidbody.freezeRotation = true;
        }

        private void Start()
        {
            _transform = transform;
            
            Player = GetComponent<Player>();
            MainCameraTransform = Player.playerCamera.transform;
            InputManager = InputManager.instance;
            
            _jumpingComponent = new PlayerJumpingComponent(Player, _playerJumpingConfig);
            _crouchingComponent = new PlayerCrouchingComponent(Player, _playerCrouchingConfig);
            _climbingComponent = new PlayerClimbingComponent(Player, _playerClimbingConfig);
            _leaningComponent = new PlayerLeaningComponent(Player, _playerLeaningConfig);
            _swimmingComponent = new PlayerSwimmingComponent(Player, _playerSwimmingConfig);
            _dashComponent = new PlayerDashComponent(Player, _playerDashConfig);
            _sprintComponent = new PlayerSprintComponent(Player, _playerSprintConfig);
            _fallDamageComponent = new PlayerFallDamageComponent(Player, _playerFallDamageConfig);

            // clamp movement modifier percentages
            backwardSpeedPercentage = Mathf.Clamp01(backwardSpeedPercentage); // Vlad
            strafeSpeedPercentage = Mathf.Clamp01(strafeSpeedPercentage); // Vlad

            _moveSpeedMult = 1f;

            capsule.height = standingCapsuleHeight;

            // add to reach distances if capsule cast height is larger than default
            _crouchingComponent.PlayerHeightMod = standingCapsuleHeight - 2.24f;

            // initialize player height variables
            _crouchingComponent.CrouchCapsuleCheckRadius = capsule.radius * 0.9f;
        }

        private void Update()
        {
            if (!Player.canControl)
                return;

            Vector3 currentPosition = _transform.position;
            float t = Time.time;
            
            _crouchingComponent.CrouchCapsuleCheckRadius = capsule.radius * 0.9f;

            bool canStandUpOrJump = CanStandUpOrJump(currentPosition, out Vector3 p2);

            _crouchingComponent.CrouchUpdateLogic(t, canStandUpOrJump, currentPosition, p2);
            DetermineIfMoving();
            _jumpingComponent.JumpingUpdateLogic(t, canStandUpOrJump);
            _climbingComponent.ClimbingUpdateLogic();
            _swimmingComponent.InWaterUpdateLogic();
        }

        private void LateUpdate()
        {
            if (!Player.canControl) return;
            transform.eulerAngles = Vector3.up * Player.smoothLook.transform.eulerAngles.y;
/*#if UNITY_2022_1_OR_NEWER
			transform.eulerAngles = Vector3.up * player.smoothLook.transform.eulerAngles.y;
#else
            transform.Rotate(Vector3.up * player.smoothLook.lookInput.x, Space.Self);
#endif*/
        }

        private void FixedUpdate()
        {
            if (!Player.canControl)
                return;

            float t = Time.time;
            float fdt = Time.fixedDeltaTime;
            float dt = Time.deltaTime;

            if (_swimmingComponent.IsSwimming && JumpingComponent.IsJumping)
                JumpingComponent.IsJumping = false;

            PlayerBottomPos = GetPlayerBottomPosition();
            Velocity = Rigidbody.velocity;
            Vector2 move = InputManager.GetMovementInput();
            InputY = move.y;

            bool isSwimming = _swimmingComponent.IsSwimming;

            // manage the CapsuleCast size and distance for frontal collision check based on player stance
            // set the vertical bounds of the capsule used to detect player collisions
            PlayerMiddlePos = _transform.position; // middle of player capsule
            PlayerTopPos = PlayerMiddlePos + _transform.up * (capsule.height * 0.45f); // top of player capsule

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Player Input
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            if (!isSwimming)
            {
                _inputXLerpSpeed = _inputYLerpSpeed = _inputLerpSpeed;
            }
            else
            {
                // player accelerates and decelerates slower in water.
                _inputXLerpSpeed = _inputYLerpSpeed = 3f;
            }

            if (_sprintComponent.Sprint)
            {
                // only strafe when sprinting after pressing the joystick horizontally further than 0.4, for better control and less zig-zagging
                if (Mathf.Abs(move.x) > 0.4f)
                {
                    InputX = Mathf.Sign(move.x);
                }
                else
                {
                    InputX = 0f;
                }
            }
            else
            {
                // cancel horizontal movement if player is looking up while prone
                InputX = move.x;
            }

            // Smooth our movement inputs using Mathf.Lerp
            _inputXSmoothed = Mathf.Lerp(_inputXSmoothed, InputX, dt * _inputXLerpSpeed);
            _inputYSmoothed = Mathf.Lerp(_inputYSmoothed, InputY, dt * _inputYLerpSpeed);

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Player Movement Speeds
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            // check that player can run and set speed 
            if (_sprintComponent.SprintActive)
            {
                float sprintSpeed = _sprintComponent.SprintConfig.sprintSpeed;
                
                // offset speeds by 0.1f to prevent small oscillation of speed value
                if (_speed < sprintSpeed - 0.1f)
                {
                    _speed += 15f * dt; // gradually accelerate to run speed
                }
                else if (_speed > sprintSpeed + 0.1f)
                {
                    _speed -= 15f * dt; // gradually decelerate to run speed
                }
            }
            else
            {
                if (!isSwimming)
                {
                    if (_speed > walkSpeed * _moveSpeedMult + 0.1f)
                    {
                        _speed -= 15f * dt; // gradually decelerate to walk speed
                    }
                    else if (_speed < walkSpeed * _moveSpeedMult - 0.1f)
                    {
                        _speed += 15f * dt; // gradually accelerate to walk speed
                    }
                }
            }

            if (_swimmingComponent.IsSwimming)
            {
                float swimSpeed = _swimmingComponent.SwimmingConfig.swimSpeed;
                
                if (_speed > swimSpeed + 0.1f)
                {
                    _speed -= 30f * dt; // gradually decelerate to swim speed
                }
                else if (_speed < swimSpeed - 0.1f)
                {
                    _speed += 15f * dt; // gradually accelerate to swim speed
                }
            }

            float speedMod = _crouchingComponent.PlayerHeightMod / 2.24f + 1f;

            // also lower to crouch position if player dies
            if (_swimmingComponent.IsSwimming)
            {
                MidPos = _swimmingComponent.SwimmingConfig.swimmingCamHeight;
                // gradually increase capsule height
                float nh = Mathf.Min(capsule.height + 4f * (dt * speedMod), _swimmingComponent.SwimmingConfig.swimmingCapsuleHeight);
                capsule.height = nh;
            }
            else if (_crouchingComponent.IsCrouching)
            {
                MidPos = _crouchingComponent.CrouchingConfig.crouchingCamHeight;
                // gradually decrease capsule height
                float nh = Mathf.Max(capsule.height - 4f * (dt * speedMod), _crouchingComponent.CrouchingConfig.crouchCapsuleHeight);
                capsule.height = nh;
            }
            else
            {
                MidPos = standingCamHeight;
                // gradually increase capsule height
                float nh = Mathf.Min(capsule.height + 4f * (dt * (0.9f * speedMod)), standingCapsuleHeight);
                capsule.height = nh;
            }

            CamPos = _transform.position + _transform.up * MidPos;

            // This is the start of the large block that performs all movement actions while grounded	
            if (IsGrounded)
            {
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Landing
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                // reset airTimeState var so that airTime will only be set once when player looses grounding
                _airTimeState = true;

                // reset falling state and perform actions if player has landed from a fall
                if (IsFalling)
                {
                    _playerAirVelocity = Vector3.zero;
                    JumpingComponent.LandStartTime = t; // track the time when player landed

                    if (_fallStartLevel - transform.position.y > 2f)
                    {
                        Player.cameraAnimator.SetTrigger(CameraAnimNames.Land);
                        OnLand?.Invoke();
                    }

                    float dist = _fallStartLevel - transform.position.y;

                    // track the distance of the fall and apply damage if over falling threshold
                    bool doFallDamage = _fallDamageComponent.FallDamageConfig.allowFallDamage
                                        && _transform.position.y < _fallStartLevel - _fallDamageComponent.FallDamageConfig.fallDamageThreshold
                                        && !_swimmingComponent.InWater;
                    
                    if (doFallDamage)
                        _fallDamageComponent.CalculateFallingDamage(dist);

                    // active platform
                    Collider currentGroundCollider = GroundHitInfo.collider;
                    
                    if (currentGroundCollider != null)
                    {
                        Rigidbody groundRigidbody = currentGroundCollider.attachedRigidbody;
                        
                        if (groundRigidbody != null && !groundRigidbody.isKinematic)
                        {
                            groundRigidbody.AddForceAtPosition(Vector3.down * Rigidbody.mass, _groundPoint,
                                ForceMode.Acceleration);
                        }
                    }

                    IsFalling = false;
                }

                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Sprinting
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                _sprintComponent.FixedUpdateLogic(t, move, isSwimming);

                // check that player can crouch and set speed
                // also check midpos because player can still be under obstacle when crouch button is released 
                if (_crouchingComponent.IsCrouching || MidPos < standingCamHeight)
                {
                    if (_crouchSpeedAmt > _crouchingComponent.CrouchingConfig.crouchSpeedMult)
                        _crouchSpeedAmt -= dt; // gradually decrease crouchSpeedAmt to crouch limit value
                }
                else
                {
                    if (_crouchSpeedAmt < 1f)
                        _crouchSpeedAmt += dt; // gradually increase crouchSpeedAmt to neutral
                }

                // limit move speed if backpedaling
                if (InputY >= 0)
                {
                    if (_speedAmtY < 1f)
                        _speedAmtY += dt; // gradually increase speedAmtY to neutral
                }
                else
                {
                    if (_speedAmtY > backwardSpeedPercentage)
                        _speedAmtY -= dt; // gradually decrease speedAmtY to backpedal limit value
                }

                // allow limiting of move speed if strafing directly sideways and not diagonally
                if (InputX == 0f || InputY != 0f)
                {
                    if (_speedAmtX < 1f)
                        _speedAmtX += dt; // gradually increase speedAmtX to neutral
                }
                else
                {
                    if (_speedAmtX > strafeSpeedPercentage)
                        _speedAmtX -= dt; // gradually decrease speedAmtX to strafe limit value
                }
            }
            else
            {
                // Player is airborn
                // keep track of the time that player lost grounding for air manipulation and moving gun while jumping
                if (_airTimeState)
                {
                    _airTime = t;
                    _airTimeState = false;
                }

                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Falling
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                // subtract height we began falling from current position to get falling distance
                if (!isSwimming && !_climbingComponent.IsClimbing)
                    FallingDistance = _fallStartLevel - transform.position.y; // this value referenced in other scripts

                if (!IsFalling && !_climbingComponent.IsClimbing && !isSwimming)
                {
                    // playerAirInitVelocity = playerAirVelocity = new Vector3(velocity.x, 0f, velocity.z);
                    IsFalling = true;
                    // start tracking altitude (y position) for fall check
                    _fallStartLevel = transform.position.y;
                }
            }

            if (IsGrounded || _climbingComponent.IsClimbing)
            {
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Leaning
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                _leaningComponent.FixedUpdateLogic(move);
            }

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Swimming
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            _swimmingComponent.FixedUpdateLogic(t, _debug);

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Player Velocity
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            // limit speed if strafing diagonally
            _limitStrafeSpeed = Mathf.Abs(InputX) > 0.5f && Mathf.Abs(InputY) > 0.5f ? diagonalStrafeAmt : 1f;

            // We are grounded, so recalculate movedirection directly from axes	
            MoveDirection = new Vector3(_inputXSmoothed * _limitStrafeSpeed, 0f, _inputYSmoothed * _limitStrafeSpeed);

            // realign moveDirection vector to world space
            if (isSwimming)
            {
                MoveDirection = MainCameraTransform.TransformDirection(MoveDirection);
            }
            else
            {
                MoveDirection = transform.TransformDirection(MoveDirection);
            }

            CheckGround(MoveDirection.normalized);

            if (_climbingComponent.IsClimbing)
            {
                if (_climbingComponent.ClimbingState == ClimbingState.Grabbed)
                    _climbingComponent.Climbing();
            }
            else
            {
                PhysicMaterial physMaterial = MoveDirection.magnitude > 0.1f ? groundedPhysMaterial : stayPhysMaterial;
                capsule.material = IsGrounded ? physMaterial : flyPhysMaterial;
                
                _canStep = CanStep(MoveDirection.normalized);

                if (IsGrounded)
                {
                    if (!JumpingComponent.IsJumping && !isSwimming)
                        YVelocity = 0f;
                    
                    if (!isSwimming)
                        MoveDirection = _sprintComponent.Sprint ? _futureDirection : Vector3.Project(MoveDirection, _futureDirection);
                }
                else
                {
                    if (!YVelocity.IsEquals(_gravityMax) && !isSwimming)
                    {
                        YVelocity = Mathf.Clamp(YVelocity + (Physics.gravity.y * _gravityMultiplier) * fdt, -_gravityMax,
                            _gravityMax);
                    }

                    if (_swimmingComponent.InWater)
                    {
                        if (_swimmingComponent.PlayerWaterInitVelocity.magnitude > 0.1f)
                        {
                            _swimmingComponent.PlayerWaterInitVelocity = Vector3.Lerp(_swimmingComponent.PlayerWaterInitVelocity, Vector3.zero, fdt);
                            MoveDirection += new Vector3(_swimmingComponent.PlayerWaterInitVelocity.x, 0f, _swimmingComponent.PlayerWaterInitVelocity.z) *
                                             fdt;
                        }
                        else
                        {
                            _swimmingComponent.PlayerWaterInitVelocity = Vector3.zero;
                        }
                    }
                    else
                    {
                        if (MoveDirection.magnitude > 0f)
                        {
                            _playerAirVelocity = MoveDirection * _airSpeedMultiplier;
                        }
                        else
                        {
                            _playerAirVelocity = Vector3.Lerp(_playerAirVelocity, Vector3.zero, 5f * fdt);
                        }

                        MoveDirection = _playerAirVelocity;
                    }
                }

                if (_swimmingComponent.IsBelowWater)
                {
                    if (Mathf.Abs(YVelocity) > 0.1f)
                    {
                        YVelocity = Mathf.Lerp(YVelocity, 0f, 5f * fdt);
                    }
                    else
                    {
                        YVelocity = 0f;
                    }
                }

                _currSpeedMult = 1f;

                if (IsGrounded && !isSwimming && !_canStep)
                {
                    _currSpeedMult = slopeSpeed.Evaluate(SlopeAngle / slopeLimit);
                    _currSpeedMult *= CanMoveToSlope() ? 1f : 0f;
                }

                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Dash
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                bool canDash = _dashComponent.DashConfig.allowDash
                               && !_dashComponent.DashState
                               && InputManager.GetActionKey(InputManager.Action.Dash)
                               && IsGrounded
                               && !isSwimming
                               && !IsFalling 
                               && !JumpingComponent.IsJumping
                               && IsMoving;
                
                if (canDash)
                    _dashComponent.StartDashCoroutine();

                // apply speed limits to moveDirection vector
                float currSpeed = _speed * _speedAmtX * _speedAmtY * _crouchSpeedAmt * _zoomSpeedAmt * _currSpeedMult *
                                  SpeedMult;

                MoveDirection = MoveDirection * currSpeed;
                MoveDirection += Vector3.up * YVelocity;
                MoveDirection2D = new Vector3(MoveDirection.x, 0f, MoveDirection.y);

                // apply a force that attempts to reach target velocity
                _velocityChange = MoveDirection - Velocity;

                // limit max speed
                _velocityChange.x = Mathf.Clamp(_velocityChange.x, -_maxVelocityChange, _maxVelocityChange);
                _velocityChange.z = Mathf.Clamp(_velocityChange.z, -_maxVelocityChange, _maxVelocityChange);

                // finally, add movement velocity to player rigidbody velocity
                if (!_velocityChange.IsZero()) Rigidbody.AddForce(_velocityChange, ForceMode.VelocityChange);

                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Swimming
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                
                _swimmingComponent.FixedUpdateLogic2(t, dt);
                
                if (IsGrounded && Vector3.Angle(_groundNormal, Vector3.up) > slopeLimit && Rigidbody.velocity.y <= 0f &&
                    !_swimmingComponent.IsBelowWater)
                {
                    Rigidbody.velocity += transform.up * (Physics.gravity.y * (30f * fdt));
                }

                if (IsGrounded && !isSwimming && !JumpingComponent.IsJumping && GroundHitInfo.rigidbody != null &&
                    GroundHitInfo.rigidbody.isKinematic)
                {
                    Rigidbody.velocity += GroundHitInfo.rigidbody.GetVelocityAtPoint(_groundPoint);
                }

                if (_dashComponent.DashActive)
                {
                    MoveDirection = (MoveDirection.IsZero() ? _dashComponent.DashDirection : MoveDirection.normalized) *
                                    (currSpeed + _dashComponent.DashCurrentForce);
                    MoveDirection += Vector3.up * YVelocity;
                    _velocityChange = MoveDirection - Velocity;
                    
                    if (!_velocityChange.IsZero())
                        Rigidbody.AddForce(_velocityChange, ForceMode.VelocityChange);
                }
            }

            HoverClimbable = false;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SendOnWaterEvent(bool inWater) =>
            OnWater?.Invoke(inWater);
        
        public void SendOnJumpEvent() =>
            OnJump?.Invoke();
        
        public void SendOnLandEvent() =>
            OnLand?.Invoke();

        public void SendOnCrouchEvent(bool isCrouching) =>
            OnCrouch?.Invoke(isCrouching);

        public void SendFallDamageEvent(int damage) =>
            OnFallDamage?.Invoke(damage);

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private bool CanStandUpOrJump(Vector3 currentPosition, out Vector3 p2)
        {
            float crouchCapsuleCheckRadius = _crouchingComponent.CrouchCapsuleCheckRadius;
            
            // set the vertical bounds of the capsule used to detect player collisions
            Vector3 p1 = currentPosition - Vector3.up *
                (currentPosition.y - 0.1f - (capsule.bounds.min.y + crouchCapsuleCheckRadius));
            
            // top point for capsule cast when standing
            p2 =
                p1 + Vector3.up *
                (standingCapsuleHeight - 0.1f -
                 crouchCapsuleCheckRadius * 2f); // subtract 2 radii because one is added for p1

            bool canStandUpOrJump = !Physics.CheckCapsule(
                currentPosition + _transform.up * 0.75f,
                p2,
                crouchCapsuleCheckRadius * 0.9f,
                groundMask.value, QueryTriggerInteraction.Ignore);

            return canStandUpOrJump;
        }
        
        private void DetermineIfMoving()
        {
            // Determine if player is moving. This var is accessed by other scripts.
            IsMoving = Mathf.Abs(InputY) > 0.01f || Mathf.Abs(InputX) > 0.01f || _dashComponent.DashActive;
        }

        public void Stop()
        {
            _dashComponent.DashActive = false;
            IsMoving = false;
            Velocity = Vector3.zero;
            _velocityChange = Vector3.zero;
            _sprintComponent.Sprint = false;
        }

        private void CheckGround(Vector3 direction)
        {
            float radius = capsule.radius;
            Vector3 origin = PlayerBottomPos + Vector3.up * (radius + 0.2f);

            SlopeAngle = 0f;
            IsGrounded = false;

            if (_swimmingComponent.IsBelowWater)
                return;

            Vector3 castDirection = -_transform.up;
            float maxDistance = JumpingComponent.IsJumping || IsFalling ? 0.3f : 0.75f;
            
            if (Physics.SphereCast(origin, radius, castDirection, out RaycastHit hit, maxDistance,
                    groundMask.value, QueryTriggerInteraction.Ignore))
            {
                IsGrounded = true;
                GroundHitInfo = hit;
                _groundNormal = hit.normal;
                _groundPoint = hit.point;
                
                if (_lastOnGroundPositionTime < Time.time)
                {
                    LastOnGroundPosition = _groundPoint;
                    _lastOnGroundPositionTime = Time.time + 1f;
                }

                CheckSlope(direction);

                bool applyForce = PlayerBottomPos.y - hit.point.y > 0.01f
                                  && Rigidbody.velocity.y <= 0.01f
                                  && !_swimmingComponent.IsBelowWater
                                  && !_climbingComponent.IsClimbing;

                if (!applyForce)
                    return;
                
                Vector3 newForce = 30f * Physics.gravity.y * Time.fixedDeltaTime * transform.up;

                if (newForce.IsZero())
                    return;
                
                Rigidbody.AddForce(newForce, ForceMode.VelocityChange);
            }
        }

        private void CheckSlope(Vector3 direction)
        {
            _futureDirection = direction.magnitude > 0f ? GetMoveDirection(direction).normalized : Vector3.zero;
            SlopeAngle = Vector3.Angle(_futureDirection, transform.up);
            
            if (direction.magnitude > 0f)
                SlopeAngle = 90f - SlopeAngle;
            
            if (_debug)
                Debug.DrawLine(PlayerBottomPos, PlayerBottomPos + _futureDirection, Color.blue, 5f);
        }

        private Vector3 GetMoveDirection(Vector3 direction)
        {
            float radius = capsule.radius * 2f;
            float dist = radius + 0.2f;
            Vector3 origin = PlayerBottomPos + Vector3.up * dist;
            Vector3 newGroundNormal = _groundNormal;

            bool groundIsStatic = true;

            if (Physics.SphereCast(origin, radius, -transform.up, out RaycastHit hit, dist + 0.5f, groundMask.value,
                    QueryTriggerInteraction.Ignore))
            {
                Rigidbody rbody = hit.collider.gameObject.GetComponent<Rigidbody>();
                groundIsStatic = rbody == null || rbody.isKinematic;
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
            _transform.position + -_transform.up * (capsule.height * 0.5f);

        private bool CanMoveToSlope()
        {
            if (_swimmingComponent.IsSwimming)
                return true;
            
            if (_futureDirection.magnitude > 0f && _futureDirection.y <= 0f)
                return true;
            
            if (_debug)
                Debug.DrawLine(_groundPoint + (Vector3.up * 0.1f),
                    _groundPoint + (Vector3.up * 0.1f) + (-Vector3.up * 0.2f), Color.magenta, 10f);
            
            if (Physics.Raycast(_groundPoint + (Vector3.up * 0.1f), -Vector3.up, out RaycastHit rh, 0.2f,
                    groundMask.value, QueryTriggerInteraction.Ignore))
            {
                if (rh.point.y < PlayerBottomPos.y)
                    return true;
                
                return Vector3.Angle(rh.normal, Vector3.up) < slopeLimit;
            }

            return true;
        }

        private bool CanStep(Vector3 direction)
        {
            if (_swimmingComponent.IsSwimming)
                return true;

            if (!IsGrounded)
                return false;
            
            float radius = capsule.radius;
            float stepDistance = radius + radius * stepHeightRadius;
            Vector3 maxStepPos = PlayerBottomPos + direction * -0.1f + Vector3.up * (maxStepHeight + radius);
            
            if (!Physics.SphereCast(maxStepPos, radius, direction.normalized, out _, stepDistance, groundMask.value,
                    QueryTriggerInteraction.Ignore))
            {
                Vector3 slopeOrigin = maxStepPos + direction * stepDistance;
                
                if (Physics.Raycast(slopeOrigin, -Vector3.up, out RaycastHit rh,
                        (maxStepPos.y - PlayerBottomPos.y) * 2f, groundMask.value, QueryTriggerInteraction.Ignore))
                {
                    if (Vector3.Angle(rh.normal, Vector3.up) < slopeLimit)
                        return true;
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        private void OnTriggerEnter(Collider other)
        {
            // Climbing
            if (_climbingComponent.TriggerEnterLogic(other))
                return;

            // Water
            _swimmingComponent.TriggerEnterLogic(other);
        }

        private void OnTriggerStay(Collider other)
        {
            // Climbing
            _climbingComponent.TriggerStayLogic(other);
        }

        private void OnTriggerExit(Collider other)
        {
            // Water
            _swimmingComponent.TriggerExitLogic(other);
        }
    }
}