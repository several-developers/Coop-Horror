using System;
using System.Collections;
using System.Collections.Generic;
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

        public float inputLerpSpeed = 90f; // movement lerp speed
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

        [Tooltip("Percentage to decrease movement speed when strafing directly left or right.")]
        public float
            strafeSpeedPercentage = 0.8f; // percentage to decrease movement speed while strafing directly left or right

        [Tooltip("Angle of ground surface that player won't be allowed to move over."), Range(0f, 90f)]
        public int
            slopeLimit = 40; // the maximum allowed ground surface/normal angle that the player is allowed to climb

        public AnimationCurve slopeSpeed = new AnimationCurve()
            { keys = new Keyframe[] { new Keyframe(0f, 1f), new Keyframe(1f, 0f) } };

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
        [Tooltip("True if player should be allowed to sprint.")]
        public bool allowSprinting = true;

        [Tooltip("Speed that player moves when sprinting.")]
        public float sprintSpeed = 9f;

        [Tooltip(
            "User may set sprint mode to toggle, hold, or both (toggle on sprint button press, hold on sprint button hold).")]
        public SprintType sprintMode = SprintType.Both;

        public bool forwardSprintOnly = true;

        public bool sprintActive { get; set; } = true; // true when player is allowed to sprint
        public bool sprint { get; set; } // true when sprint button is ready

        public float sprintStopTime { get; set; } // track when sprinting stopped for control of item pickup time in Player script

        public float sprintEnd { get; set; }

        private float sprintDelay = 0.4f;
        private float sprintStart = -2f;
        private bool sprintEndState;
        private bool sprintStartState;
        private bool sprintStopState = true;

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
        public bool allowFallDamage = true;

        [Tooltip("Number of units the player can fall before taking damage.")]
        public float fallDamageThreshold = 5.5f;

        [Tooltip("Multiplier of unit distance fallen to convert to hitpoint damage for the player.")]
        public float fallDamageMultiplier = 2f;

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public bool IsGrounded { get; private set; } // true when capsule cast hits ground surface
        public Vector3 velocity { get; private set; } = Vector3.zero; // total movement velocity vector
        public float airTime { get; private set; } = 0f; // total time that player is airborn

        public float
            fallStartLevel { get; private set; } // the y coordinate that the player lost grounding and started to fall

        public float fallingDistance { get; private set; } // total distance that player has fallen

        // true when player is losing altitude
        public bool _falling;

        public bool falling
        {
            get => _falling;
            set
            {
                _falling = value;
                fallingDistance = 0f;
            }
        }

        public float midPos { get; private set; } =
            0.9f; // camera vertical position which is passed to VerticalBob and HorizontalBob

        public bool IsMoving { get; private set; }
        public float speedMult { get; set; } = 1f;

        // Vlad, set was private
        public float
            camDampSpeed { get; set; } // variable damp speed for vertical camera smoothing for stance changes

        public Vector3 camPos { get; private set; }
        public Vector3 eyePos { get; private set; }

        public Vector3 moveDirection { get; private set; } =
            Vector3.zero; // movement velocity vector, modified by other speed factors like walk, zoom, and crouch states

        public Vector3 moveDirection2D { get; private set; } = Vector3.zero;
        public Vector3 playerBottomPos { get; private set; }
        public Vector3 playerMiddlePos { get; private set; }
        public Vector3 playerTopPos { get; private set; }
        public Vector3 lastOnGroundPosition { get; private set; }
        public RaycastHit groundHitInfo { get; private set; }
        public bool hoverClimbable { get; set; } // Vlad, set was private

        private bool canStep;
        public float yVelocity { get; set; } // Vlad
        private Vector3 playerAirVelocity;
        private float crouchSpeedAmt = 1f;
        private float speedAmtY = 1f; // current player speed per axis which is applied to rigidbody velocity vector
        private float speedAmtX = 1f;
        private float zoomSpeedAmt = 1f;
        private float speed; // combined axis speed of player movement
        private float limitStrafeSpeed = 0f;
        private bool airTimeState;
        private float moveSpeedMult = 1f;
        private float currSpeedMult = 1f;
        private int maxVelocityChange = 5; // maximum rate that player velocity can change
        private Vector3 velocityChange = Vector3.zero;
        private Vector3 futureDirection = Vector3.zero;
        private Vector3 groundNormal;
        private Vector3 groundPoint;
        public float slopeAngle { get; private set; } // Vlad
        private float lastOnGroundPositionTime;

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Player Input
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public float inputXSmoothed { get; private set; } // inputs smoothed using lerps
        public float inputYSmoothed { get; private set; }
        public float inputX { get; set; } // 1 = button pressed 0 = button released
        public float inputY { get; set; }

        private float inputYLerpSpeed;
        private float inputXLerpSpeed;

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

        // FIELDS: --------------------------------------------------------------------------------

        private PlayerJumpingComponent _jumpingComponent;
        private PlayerCrouchingComponent _crouchingComponent;
        private PlayerClimbingComponent _climbingComponent;
        private PlayerLeaningComponent _leaningComponent;
        private PlayerSwimmingComponent _swimmingComponent;
        private PlayerDashComponent _dashComponent;
        private Transform _transform;
        
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

            // clamp movement modifier percentages
            backwardSpeedPercentage = Mathf.Clamp01(backwardSpeedPercentage); // Vlad
            strafeSpeedPercentage = Mathf.Clamp01(strafeSpeedPercentage); // Vlad

            moveSpeedMult = 1f;

            capsule.height = standingCapsuleHeight;

            // add to reach distances if capsule cast height is larger than default
            _crouchingComponent.PlayerHeightMod = standingCapsuleHeight - 2.24f;

            // initialize player height variables
            _crouchingComponent.CrouchCapsuleCheckRadius = capsule.radius * 0.9f;
            
            // set sprint mode to toggle, hold, or both, based on inspector setting
            switch (sprintMode)
            {
                case SprintType.Both:
                    sprintDelay = 0.4f;
                    break;
                case SprintType.Hold:
                    sprintDelay = 0f;
                    break;
                case SprintType.Toggle:
                    sprintDelay = 999f; // time allowed between button down and release to activate toggle
                    break;
            }
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

            playerBottomPos = GetPlayerBottomPosition();
            velocity = Rigidbody.velocity;
            Vector2 move = InputManager.GetMovementInput();
            inputY = move.y;

            bool isSwimming = _swimmingComponent.IsSwimming;

            // manage the CapsuleCast size and distance for frontal collision check based on player stance
            // set the vertical bounds of the capsule used to detect player collisions
            playerMiddlePos = _transform.position; // middle of player capsule
            playerTopPos = playerMiddlePos + _transform.up * (capsule.height * 0.45f); // top of player capsule

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Player Input
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            if (!isSwimming)
            {
                inputXLerpSpeed = inputYLerpSpeed = inputLerpSpeed;
            }
            else
            {
                // player accelerates and decelerates slower in water.
                inputXLerpSpeed = inputYLerpSpeed = 3f;
            }

            if (sprint)
            {
                // only strafe when sprinting after pressing the joystick horizontally further than 0.4, for better control and less zig-zagging
                if (Mathf.Abs(move.x) > 0.4f)
                {
                    inputX = Mathf.Sign(move.x);
                }
                else
                {
                    inputX = 0f;
                }
            }
            else
            {
                // cancel horizontal movement if player is looking up while prone
                inputX = move.x;
            }

            // Smooth our movement inputs using Mathf.Lerp
            inputXSmoothed = Mathf.Lerp(inputXSmoothed, inputX, dt * inputXLerpSpeed);
            inputYSmoothed = Mathf.Lerp(inputYSmoothed, inputY, dt * inputYLerpSpeed);

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Player Movement Speeds
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            // check that player can run and set speed 
            if (sprintActive)
            {
                // offset speeds by 0.1f to prevent small oscillation of speed value
                if (speed < sprintSpeed - 0.1f)
                {
                    speed += 15f * dt; // gradually accelerate to run speed
                }
                else if (speed > sprintSpeed + 0.1f)
                {
                    speed -= 15f * dt; // gradually decelerate to run speed
                }
            }
            else
            {
                if (!isSwimming)
                {
                    if (speed > walkSpeed * moveSpeedMult + 0.1f)
                    {
                        speed -= 15f * dt; // gradually decelerate to walk speed
                    }
                    else if (speed < walkSpeed * moveSpeedMult - 0.1f)
                    {
                        speed += 15f * dt; // gradually accelerate to walk speed
                    }
                }
            }

            if (_swimmingComponent.IsSwimming)
            {
                float swimSpeed = _swimmingComponent.SwimmingConfig.swimSpeed;
                
                if (speed > swimSpeed + 0.1f)
                {
                    speed -= 30f * dt; // gradually decelerate to swim speed
                }
                else if (speed < swimSpeed - 0.1f)
                {
                    speed += 15f * dt; // gradually accelerate to swim speed
                }
            }

            float speedMod = _crouchingComponent.PlayerHeightMod / 2.24f + 1f;

            // also lower to crouch position if player dies
            if (_swimmingComponent.IsSwimming)
            {
                midPos = _swimmingComponent.SwimmingConfig.swimmingCamHeight;
                // gradually increase capsule height
                float nh = Mathf.Min(capsule.height + 4f * (dt * speedMod), _swimmingComponent.SwimmingConfig.swimmingCapsuleHeight);
                capsule.height = nh;
            }
            else if (_crouchingComponent.IsCrouching)
            {
                midPos = _crouchingComponent.CrouchingConfig.crouchingCamHeight;
                // gradually decrease capsule height
                float nh = Mathf.Max(capsule.height - 4f * (dt * speedMod), _crouchingComponent.CrouchingConfig.crouchCapsuleHeight);
                capsule.height = nh;
            }
            else
            {
                midPos = standingCamHeight;
                // gradually increase capsule height
                float nh = Mathf.Min(capsule.height + 4f * (dt * (0.9f * speedMod)), standingCapsuleHeight);
                capsule.height = nh;
            }

            camPos = _transform.position + _transform.up * midPos;

            // This is the start of the large block that performs all movement actions while grounded	
            if (IsGrounded)
            {
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Landing
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                // reset airTimeState var so that airTime will only be set once when player looses grounding
                airTimeState = true;

                // reset falling state and perform actions if player has landed from a fall
                if (falling)
                {
                    playerAirVelocity = Vector3.zero;
                    JumpingComponent.LandStartTime = t; // track the time when player landed

                    if (fallStartLevel - transform.position.y > 2f)
                    {
                        Player.cameraAnimator.SetTrigger(CameraAnimNames.Land);
                        OnLand?.Invoke();
                    }

                    float dist = fallStartLevel - transform.position.y;

                    // track the distance of the fall and apply damage if over falling threshold
                    bool doFallDamage = allowFallDamage
                                        && _transform.position.y < fallStartLevel - fallDamageThreshold
                                        && !_swimmingComponent.InWater;
                    
                    if (doFallDamage)
                        CalculateFallingDamage(dist);

                    // active platform
                    Collider currentGroundCollider = groundHitInfo.collider;
                    if (currentGroundCollider != null)
                    {
                        Rigidbody groundRB = currentGroundCollider.attachedRigidbody;
                        
                        if (groundRB != null && !groundRB.isKinematic)
                        {
                            groundRB.AddForceAtPosition(Vector3.down * Rigidbody.mass, groundPoint,
                                ForceMode.Acceleration);
                        }
                    }

                    falling = false;
                }

                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Sprinting
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                if (allowSprinting)
                {
                    // toggle or hold sprinting state by determining if sprint button is pressed or held
                    if ((Mathf.Abs(inputY) > 0f && forwardSprintOnly) ||
                        (!forwardSprintOnly && (Mathf.Abs(inputX) > 0f) || Mathf.Abs(inputY) > 0f))
                    {
                        if ((InputManager.GetActionKey(InputManager.Action.Sprint) ||
                             InputManager.GetActionKeyDown(InputManager.Action.Sprint)))
                        {
                            if (!sprintStartState)
                            {
                                sprintStart = t; // track time that sprint button was pressed
                                sprintStartState = true; // perform these actions only once
                                sprintEndState = false;
                                // if button is tapped, toggle sprint state
                                if (sprintEnd - sprintStart < sprintDelay * Time.timeScale)
                                {
                                    if (!sprint)
                                    {
                                        // only allow sprint to start or cancel crouch if player is not under obstacle
                                        if (!sprint)
                                        {
                                            sprint = true;
                                        }
                                        else
                                        {
                                            sprint = false; // pressing sprint button again while sprinting stops sprint
                                        }
                                    }
                                    else
                                    {
                                        sprint = false;
                                    }
                                }
                            }
                        }
                        else if (!sprintEndState)
                        {
                            sprintEnd = t; // track time that sprint button was released
                            sprintEndState = true;
                            sprintStartState = false;
                            // if releasing sprint button after holding it down, stop sprinting
                            if (sprintEnd - sprintStart > sprintDelay * Time.timeScale)
                            {
                                sprint = false;
                            }
                        }
                    }
                    else
                    {
                        if (!InputManager.GetActionKey(InputManager.Action.Sprint))
                            sprint = false;
                    }
                }

                if (_dashComponent.DashActive)
                    sprint = false;

                // cancel a sprint in certain situations
                if ((inputY <= 0f && Mathf.Abs(inputX) > 0f &&
                     forwardSprintOnly) // cancel sprint if player sprints into a wall and strafes left or right
                    || (move.y <= 0f && forwardSprintOnly) // cancel sprint if joystick is released
                    || (inputY < 0f && forwardSprintOnly) // cancel sprint if player moves backwards
                    || (JumpingComponent.IsJumping && JumpingComponent.JumpingConfig.jumpCancelsSprint)
                    || isSwimming
                    || _climbingComponent.IsClimbing // cancel sprint if player runs out of breath
                   )
                {
                    sprint = false;
                }

                // determine if player can run
                if (((inputY > 0f && forwardSprintOnly) || (IsMoving && !forwardSprintOnly))
                    && sprint
                    && !_crouchingComponent.IsCrouching
                    && IsGrounded
                   )
                {
                    sprintActive = true;
                    sprintStopState = true;
                }
                else
                {
                    if (sprintStopState)
                    {
                        sprintStopTime = t;
                        sprintStopState = false;
                    }

                    sprintActive = false;
                }

                // check that player can crouch and set speed
                // also check midpos because player can still be under obstacle when crouch button is released 
                if (_crouchingComponent.IsCrouching || midPos < standingCamHeight)
                {
                    if (crouchSpeedAmt > _crouchingComponent.CrouchingConfig.crouchSpeedMult)
                        crouchSpeedAmt -= dt; // gradually decrease crouchSpeedAmt to crouch limit value
                }
                else
                {
                    if (crouchSpeedAmt < 1f)
                        crouchSpeedAmt += dt; // gradually increase crouchSpeedAmt to neutral
                }

                // limit move speed if backpedaling
                if (inputY >= 0)
                {
                    if (speedAmtY < 1f)
                        speedAmtY += dt; // gradually increase speedAmtY to neutral
                }
                else
                {
                    if (speedAmtY > backwardSpeedPercentage)
                        speedAmtY -= dt; // gradually decrease speedAmtY to backpedal limit value
                }

                // allow limiting of move speed if strafing directly sideways and not diagonally
                if (inputX == 0f || inputY != 0f)
                {
                    if (speedAmtX < 1f)
                        speedAmtX += dt; // gradually increase speedAmtX to neutral
                }
                else
                {
                    if (speedAmtX > strafeSpeedPercentage)
                        speedAmtX -= dt; // gradually decrease speedAmtX to strafe limit value
                }
            }
            else
            {
                // Player is airborn
                // keep track of the time that player lost grounding for air manipulation and moving gun while jumping
                if (airTimeState)
                {
                    airTime = t;
                    airTimeState = false;
                }

                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Falling
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                // subtract height we began falling from current position to get falling distance
                if (!isSwimming && !_climbingComponent.IsClimbing)
                    fallingDistance = fallStartLevel - transform.position.y; // this value referenced in other scripts

                if (!falling && !_climbingComponent.IsClimbing && !isSwimming)
                {
                    // playerAirInitVelocity = playerAirVelocity = new Vector3(velocity.x, 0f, velocity.z);
                    falling = true;
                    // start tracking altitude (y position) for fall check
                    fallStartLevel = transform.position.y;
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
            limitStrafeSpeed = Mathf.Abs(inputX) > 0.5f && Mathf.Abs(inputY) > 0.5f ? diagonalStrafeAmt : 1f;

            // We are grounded, so recalculate movedirection directly from axes	
            moveDirection = new Vector3(inputXSmoothed * limitStrafeSpeed, 0f, inputYSmoothed * limitStrafeSpeed);

            // realign moveDirection vector to world space
            if (isSwimming)
            {
                moveDirection = MainCameraTransform.TransformDirection(moveDirection);
            }
            else
            {
                moveDirection = transform.TransformDirection(moveDirection);
            }

            CheckGround(moveDirection.normalized);

            if (_climbingComponent.IsClimbing)
            {
                if (_climbingComponent.ClimbingState == ClimbingState.Grabbed)
                    _climbingComponent.Climbing();
            }
            else
            {
                PhysicMaterial physMaterial = moveDirection.magnitude > 0.1f ? groundedPhysMaterial : stayPhysMaterial;
                capsule.material = IsGrounded ? physMaterial : flyPhysMaterial;
                
                canStep = CanStep(moveDirection.normalized);

                if (IsGrounded)
                {
                    if (!JumpingComponent.IsJumping && !isSwimming)
                        yVelocity = 0f;
                    
                    if (!isSwimming)
                        moveDirection = sprint ? futureDirection : Vector3.Project(moveDirection, futureDirection);
                }
                else
                {
                    if (!yVelocity.IsEquals(_gravityMax) && !isSwimming)
                    {
                        yVelocity = Mathf.Clamp(yVelocity + (Physics.gravity.y * _gravityMultiplier) * fdt, -_gravityMax,
                            _gravityMax);
                    }

                    if (_swimmingComponent.InWater)
                    {
                        if (_swimmingComponent.PlayerWaterInitVelocity.magnitude > 0.1f)
                        {
                            _swimmingComponent.PlayerWaterInitVelocity = Vector3.Lerp(_swimmingComponent.PlayerWaterInitVelocity, Vector3.zero, fdt);
                            moveDirection += new Vector3(_swimmingComponent.PlayerWaterInitVelocity.x, 0f, _swimmingComponent.PlayerWaterInitVelocity.z) *
                                             fdt;
                        }
                        else
                        {
                            _swimmingComponent.PlayerWaterInitVelocity = Vector3.zero;
                        }
                    }
                    else
                    {
                        if (moveDirection.magnitude > 0f)
                        {
                            playerAirVelocity = moveDirection * _airSpeedMultiplier;
                        }
                        else
                        {
                            playerAirVelocity = Vector3.Lerp(playerAirVelocity, Vector3.zero, 5f * fdt);
                        }

                        moveDirection = playerAirVelocity;
                    }
                }

                if (_swimmingComponent.IsBelowWater)
                {
                    if (Mathf.Abs(yVelocity) > 0.1f)
                    {
                        yVelocity = Mathf.Lerp(yVelocity, 0f, 5f * fdt);
                    }
                    else
                    {
                        yVelocity = 0f;
                    }
                }

                currSpeedMult = 1f;

                if (IsGrounded && !isSwimming && !canStep)
                {
                    currSpeedMult = slopeSpeed.Evaluate(slopeAngle / slopeLimit);
                    currSpeedMult *= CanMoveToSlope() ? 1f : 0f;
                }

                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Dash
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                bool canDash = _dashComponent.DashConfig.allowDash
                               && !_dashComponent.DashState
                               && InputManager.GetActionKey(InputManager.Action.Dash)
                               && IsGrounded
                               && !isSwimming
                               && !falling 
                               && !JumpingComponent.IsJumping
                               && IsMoving;
                
                if (canDash)
                    _dashComponent.StartDashCoroutine();

                // apply speed limits to moveDirection vector
                float currSpeed = speed * speedAmtX * speedAmtY * crouchSpeedAmt * zoomSpeedAmt * currSpeedMult *
                                  speedMult;

                moveDirection = moveDirection * currSpeed;
                moveDirection += Vector3.up * yVelocity;
                moveDirection2D = new Vector3(moveDirection.x, 0f, moveDirection.y);

                // apply a force that attempts to reach target velocity
                velocityChange = moveDirection - velocity;

                // limit max speed
                velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
                velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);

                // finally, add movement velocity to player rigidbody velocity
                if (!velocityChange.IsZero()) Rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);

                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Swimming
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                
                _swimmingComponent.FixedUpdateLogic2(t, dt);
                
                if (IsGrounded && Vector3.Angle(groundNormal, Vector3.up) > slopeLimit && Rigidbody.velocity.y <= 0f &&
                    !_swimmingComponent.IsBelowWater)
                {
                    Rigidbody.velocity += transform.up * (Physics.gravity.y * (30f * fdt));
                }

                if (IsGrounded && !isSwimming && !JumpingComponent.IsJumping && groundHitInfo.rigidbody != null &&
                    groundHitInfo.rigidbody.isKinematic)
                {
                    Rigidbody.velocity += groundHitInfo.rigidbody.GetVelocityAtPoint(groundPoint);
                }

                if (_dashComponent.DashActive)
                {
                    moveDirection = (moveDirection.IsZero() ? _dashComponent.DashDirection : moveDirection.normalized) *
                                    (currSpeed + _dashComponent.DashCurrentForce);
                    moveDirection += Vector3.up * yVelocity;
                    velocityChange = moveDirection - velocity;
                    
                    if (!velocityChange.IsZero())
                        Rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
                }
            }

            hoverClimbable = false;
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
            IsMoving = Mathf.Abs(inputY) > 0.01f || Mathf.Abs(inputX) > 0.01f || _dashComponent.DashActive;
        }

        public void Stop()
        {
            _dashComponent.DashActive = false;
            IsMoving = false;
            velocity = Vector3.zero;
            velocityChange = Vector3.zero;
            sprint = false;
        }

        private void CheckGround(Vector3 direction)
        {
            float radius = capsule.radius;
            Vector3 origin = playerBottomPos + Vector3.up * (radius + 0.2f);

            slopeAngle = 0f;
            IsGrounded = false;

            if (_swimmingComponent.IsBelowWater)
                return;

            Vector3 castDirection = -_transform.up;
            float maxDistance = JumpingComponent.IsJumping || falling ? 0.3f : 0.75f;
            
            if (Physics.SphereCast(origin, radius, castDirection, out RaycastHit hit, maxDistance,
                    groundMask.value, QueryTriggerInteraction.Ignore))
            {
                IsGrounded = true;
                groundHitInfo = hit;
                groundNormal = hit.normal;
                groundPoint = hit.point;
                
                if (lastOnGroundPositionTime < Time.time)
                {
                    lastOnGroundPosition = groundPoint;
                    lastOnGroundPositionTime = Time.time + 1f;
                }

                CheckSlope(direction);

                bool applyForce = playerBottomPos.y - hit.point.y > 0.01f
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
            futureDirection = direction.magnitude > 0f ? GetMoveDirection(direction).normalized : Vector3.zero;
            slopeAngle = Vector3.Angle(futureDirection, transform.up);
            
            if (direction.magnitude > 0f)
                slopeAngle = 90f - slopeAngle;
            
            if (_debug)
                Debug.DrawLine(playerBottomPos, playerBottomPos + futureDirection, Color.blue, 5f);
        }

        private Vector3 GetMoveDirection(Vector3 direction)
        {
            float radius = capsule.radius * 2f;
            float dist = radius + 0.2f;
            Vector3 origin = playerBottomPos + Vector3.up * dist;
            Vector3 newGroundNormal = groundNormal;

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
            
            if (futureDirection.magnitude > 0f && futureDirection.y <= 0f)
                return true;
            
            if (_debug)
                Debug.DrawLine(groundPoint + (Vector3.up * 0.1f),
                    groundPoint + (Vector3.up * 0.1f) + (-Vector3.up * 0.2f), Color.magenta, 10f);
            
            if (Physics.Raycast(groundPoint + (Vector3.up * 0.1f), -Vector3.up, out RaycastHit rh, 0.2f,
                    groundMask.value, QueryTriggerInteraction.Ignore))
            {
                if (rh.point.y < playerBottomPos.y)
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
            Vector3 maxStepPos = playerBottomPos + direction * -0.1f + Vector3.up * (maxStepHeight + radius);
            
            if (!Physics.SphereCast(maxStepPos, radius, direction.normalized, out _, stepDistance, groundMask.value,
                    QueryTriggerInteraction.Ignore))
            {
                Vector3 slopeOrigin = maxStepPos + direction * stepDistance;
                
                if (Physics.Raycast(slopeOrigin, -Vector3.up, out RaycastHit rh,
                        (maxStepPos.y - playerBottomPos.y) * 2f, groundMask.value, QueryTriggerInteraction.Ignore))
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

        private void CalculateFallingDamage(float fallDistance)
        {
            if (fallDamageMultiplier >= 1f)
            {
                int dmg = (int)(fallDistance * fallDamageMultiplier);
                OnFallDamage?.Invoke(dmg);
            }
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