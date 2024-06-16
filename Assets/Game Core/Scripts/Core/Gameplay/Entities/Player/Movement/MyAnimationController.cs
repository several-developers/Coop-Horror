using System;
using ECM2;
using GameCore.Gameplay.Entities.Player.CameraManagement;
using GameCore.Gameplay.InputManagement;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Player
{
    /// <summary>
    /// This example shows how to animate a Character,
    /// using the Character data (movement direction, velocity, is jumping, etc) to feed your Animator.
    /// </summary>
    public class MyAnimationController : MonoBehaviour
    {
        private enum AnimationState
        {
            Base,
            Locomotion,
            Jump,
            Fall,
            Crouch
        }

        private enum GaitState
        {
            Idle,
            Walk,
            Run,
            Sprint
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Header("Main Settings")]
        [Tooltip("Whether the character always faces the camera facing direction")]
        [SerializeField]
        private bool _alwaysStrafe = true;

        [SerializeField]
        private float _rotationSmoothing = 10f;

        [Header("Shuffles")]
        [Tooltip("Threshold for button hold duration.")]
        [SerializeField]
        private float _buttonHoldThreshold = 0.15f;

        [Header("Player Strafing")]
        [Tooltip("Minimum threshold for forward strafing angle.")]
        [SerializeField]
        private float _forwardStrafeMinThreshold = -55.0f;

        [Tooltip("Maximum threshold for forward strafing angle.")]
        [SerializeField]
        private float _forwardStrafeMaxThreshold = 125.0f;

        [Header("Grounded Angle")]
        [Tooltip("Position of the rear ray for grounded angle check.")]
        [SerializeField]
        private Transform _rearRayPos;

        [Tooltip("Position of the front ray for grounded angle check.")]
        [SerializeField]
        private Transform _frontRayPos;

        [Tooltip("Layer mask for checking ground.")]
        [SerializeField]
        private LayerMask _groundLayerMask;

        [Tooltip("Useful for rough ground")]
        [SerializeField]
        private float _groundedOffset = -0.14f;

        [Header("Player Lean")]
        [Tooltip("Flag indicating if leaning is enabled.")]
        [SerializeField]
        private bool _enableLean = true;

        [Tooltip("Delay for leaning.")]
        [SerializeField]
        private float _leanDelay;

        [Tooltip("Current value for leaning.")]
        [SerializeField]
        private float _leanValue;

        [Tooltip("Curve for leaning.")]
        [SerializeField]
        private AnimationCurve _leanCurve;

        [Tooltip("Delay for head leaning looks.")]
        [SerializeField]
        private float _leansHeadLooksDelay;

        [Tooltip("Flag indicating if an animation clip has ended.")]
        [SerializeField]
        private bool _animationClipEnd;

        [Header("Player Head Look")]
        [Tooltip("Flag indicating if head turning is enabled.")]
        [SerializeField]
        private bool _enableHeadTurn = true;
        [Tooltip("Delay for head turning.")]
        [SerializeField]
        private float _headLookDelay;
        [Tooltip("X-axis value for head turning.")]
        [SerializeField]
        private float _headLookX;
        [Tooltip("Y-axis value for head turning.")]
        [SerializeField]
        private float _headLookY;
        [Tooltip("Curve for X-axis head turning.")]
        [SerializeField]
        private AnimationCurve _headLookXCurve;
        
        [Header("Player Body Look")]
        [Tooltip("Flag indicating if body turning is enabled.")]
        [SerializeField]
        private bool _enableBodyTurn = true;
        [Tooltip("Delay for body turning.")]
        [SerializeField]
        private float _bodyLookDelay;
        [Tooltip("X-axis value for body turning.")]
        [SerializeField]
        private float _bodyLookX;
        [Tooltip("Y-axis value for body turning.")]
        [SerializeField]
        private float _bodyLookY;
        [Tooltip("Curve for X-axis body turning.")]
        [SerializeField]
        private AnimationCurve _bodyLookXCurve;

        // PROPERTIES: ----------------------------------------------------------------------------

        private Vector3 Velocity => _character.velocity;
        private float WalkSpeed => _character.minAnalogWalkSpeed;
        private float RunSpeed => _character.maxWalkSpeed;
        private float SprintSpeed => _sprintAbility.MaxSprintSpeed;
        private float SpeedChangeDamping => _character.maxAcceleration;
        private bool IsGrounded => _character.IsGrounded();
        private bool IsCrouching => _character.IsCrouched();
        private bool IsWalking => _character.IsWalking();
        private bool IsSprinting => _sprintAbility.IsSprinting();

        // FIELDS: --------------------------------------------------------------------------------

        #region Animation Variable Hashes

        private readonly int _movementInputTappedHash = Animator.StringToHash(name: "MovementInputTapped");
        private readonly int _movementInputPressedHash = Animator.StringToHash(name: "MovementInputPressed");
        private readonly int _movementInputHeldHash = Animator.StringToHash(name: "MovementInputHeld");
        private readonly int _shuffleDirectionXHash = Animator.StringToHash(name: "ShuffleDirectionX");
        private readonly int _shuffleDirectionZHash = Animator.StringToHash(name: "ShuffleDirectionZ");

        private readonly int _moveSpeedHash = Animator.StringToHash(name: "MoveSpeed");
        private readonly int _currentGaitHash = Animator.StringToHash(name: "CurrentGait");

        private readonly int _isJumpingAnimHash = Animator.StringToHash(name: "IsJumping");
        private readonly int _fallingDurationHash = Animator.StringToHash(name: "FallingDuration");

        private readonly int _inclineAngleHash = Animator.StringToHash(name: "InclineAngle");

        private readonly int _strafeDirectionXHash = Animator.StringToHash(name: "StrafeDirectionX");
        private readonly int _strafeDirectionZHash = Animator.StringToHash(name: "StrafeDirectionZ");

        private readonly int _forwardStrafeHash = Animator.StringToHash(name: "ForwardStrafe");
        private readonly int _cameraRotationOffsetHash = Animator.StringToHash(name: "CameraRotationOffset");
        private readonly int _isStrafingHash = Animator.StringToHash(name: "IsStrafing");
        private readonly int _isTurningInPlaceHash = Animator.StringToHash(name: "IsTurningInPlace");

        private readonly int _isCrouchingHash = Animator.StringToHash(name: "IsCrouching");

        private readonly int _isWalkingHash = Animator.StringToHash(name: "IsWalking");
        private readonly int _isStoppedHash = Animator.StringToHash(name: "IsStopped");
        private readonly int _isStartingHash = Animator.StringToHash(name: "IsStarting");

        private readonly int _isGroundedHash = Animator.StringToHash(name: "IsGrounded");

        private readonly int _leanValueHash = Animator.StringToHash(name: "LeanValue");
        private readonly int _headLookXHash = Animator.StringToHash(name: "HeadLookX");
        private readonly int _headLookYHash = Animator.StringToHash(name: "HeadLookY");

        private readonly int _bodyLookXHash = Animator.StringToHash(name: "BodyLookX");
        private readonly int _bodyLookYHash = Animator.StringToHash(name: "BodyLookY");

        private readonly int _locomotionStartDirectionHash = Animator.StringToHash(name: "LocomotionStartDirection");

        #endregion

        private Character _character;
        private PlayerMovementController _playerMovementController;
        private MySprintAbility _sprintAbility;
        private InputReader _inputReader;
        private Animator _animator;
        private Animator _armsAnimator;
        private Camera _mainCamera;
        private bool _isInitialized;

        private Vector2 _moveVector;

        // Runtime Properties
        [ShowInInspector]
        private AnimationState _currentState = AnimationState.Base;

        private GaitState _currentGait;

        private Vector3 _moveDirection;
        private Vector3 _currentRotation;
        private Vector3 _previousRotation;
        private float _newDirectionDifferenceAngle;
        private float _locomotionStartDirection;
        private float _locomotionStartTimer;
        private float _movementInputDuration;
        private float _currentMaxSpeed;
        private float _speed2D;
        private float _inclineAngle;
        private float _forwardStrafe = 1f;
        private float _strafeAngle;
        private float _strafeDirectionX;
        private float _strafeDirectionZ;
        private float _shuffleDirectionX;
        private float _shuffleDirectionZ;
        private float _cameraRotationOffset;
        private float _fallingDuration;
        private bool _movementInputTapped;
        private bool _movementInputPressed;
        private bool _movementInputHeld;
        private bool _isStarting;
        private bool _isStopped = true;
        private bool _isStrafing;
        private bool _isTurningInPlace;

        // Base State Variables
        private const float AnimationDampTime = 5f;
        private const float StrafeDirectionDampTime = 20f;

        private Vector3 _targetVelocity;
        private Vector3 _cameraForward;
        private float _targetMaxSpeed;
        private float _fallStartTime;
        private float _rotationRate;
        private float _initialLeanValue;
        private float _initialTurnValue;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Start() =>
            _isStrafing = _alwaysStrafe;

        private void Update()
        {
            if (!_isInitialized)
                return;

            switch (_currentState)
            {
                case AnimationState.Locomotion:
                    UpdateLocomotionState();
                    break;

                case AnimationState.Jump:
                    UpdateJumpState();
                    break;

                case AnimationState.Fall:
                    UpdateFallState();
                    break;

                case AnimationState.Crouch:
                    UpdateCrouchState();
                    break;
            }
        }

        private void OldUpdate()
        {
            float deltaTime = Time.deltaTime;

            // Compute input move vector in local space

            Vector3 move = transform.InverseTransformDirection(_character.GetMovementDirection());

            // Update the animator parameters

            float forwardAmount = _character.useRootMotion && _character.GetRootMotionController()
                ? move.z
                : Mathf.InverseLerp(a: 0.0f, b: _character.GetMaxSpeed(), value: _character.GetSpeed());

            bool isMovingBackwards = move.z < 0f;

            if (!isMovingBackwards)
            {
#warning НЕПРАВИЛЬНО РАБОТАЕТ, в правую сторону -1 даёт
                float turnAmount = Mathf.Atan2(y: move.x, x: move.z);

                _animator.SetFloat(id: AnimatorHashes.Turn, value: turnAmount, dampTime: 0.15f, deltaTime);
                _armsAnimator.SetFloat(id: AnimatorHashes.Turn, value: turnAmount, dampTime: 0.15f, deltaTime);
            }
            else
                forwardAmount *= -1f;

            float sprintAmount = _sprintAbility.IsSprinting() ? forwardAmount : 0.0f;
            bool isGrounded = _character.IsGrounded();
            bool isCrouched = _character.IsCrouched();

            _animator.SetFloat(id: AnimatorHashes.Forward, value: forwardAmount, dampTime: 0.15f, deltaTime);
            _animator.SetFloat(id: AnimatorHashes.Sprint, value: sprintAmount, dampTime: 0.3f, deltaTime);

            _armsAnimator.SetFloat(id: AnimatorHashes.Forward, value: forwardAmount, dampTime: 0.15f, deltaTime);
            _armsAnimator.SetFloat(id: AnimatorHashes.Sprint, value: sprintAmount, dampTime: 0.3f, deltaTime);

            _animator.SetBool(id: AnimatorHashes.Ground, value: isGrounded);
            _animator.SetBool(id: AnimatorHashes.Crouch, value: isCrouched);

            _armsAnimator.SetBool(id: AnimatorHashes.Ground, value: isGrounded);
            _armsAnimator.SetBool(id: AnimatorHashes.Crouch, value: isCrouched);

            if (_character.IsFalling())
            {
                float yVelocity = _character.GetVelocity().y;

                _animator.SetFloat(id: AnimatorHashes.Jump, value: yVelocity, dampTime: 0.1f, deltaTime);
                _armsAnimator.SetFloat(id: AnimatorHashes.Jump, value: yVelocity, dampTime: 0.1f, deltaTime);
            }

            // Calculate which leg is behind, so as to leave that leg trailing in the jump animation
            // (This code is reliant on the specific run cycle offset in our animations,
            // and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)

            //float runCycle = Mathf.Repeat(animator.GetCurrentAnimatorStateInfo(0).normalizedTime + 0.2f, 1.0f);
            //float jumpLeg = (runCycle < 0.5f ? 1.0f : -1.0f) * forwardAmount;

            // if (_character.IsGrounded())
            //     animator.SetFloat(JumpLeg, jumpLeg);
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Setup(Character character, PlayerMovementController playerMovementController,
            InputReader inputReader, CameraReferences cameraReferences)
        {
            _character = character;
            _playerMovementController = playerMovementController;
            _inputReader = inputReader;
            _mainCamera = cameraReferences.MainCamera;
            _animator = character.GetAnimator();
            _armsAnimator = cameraReferences.PlayerArmsAnimator;
            _sprintAbility = character.GetComponent<MySprintAbility>();
            _isInitialized = true;

            SwitchState(AnimationState.Locomotion);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        #region Other

        private void UpdateAnimatorController()
        {
            //_animator.SetFloat(_leanValueHash, _leanValue);
            _animator.SetFloat(_headLookXHash, _headLookX);
            _animator.SetFloat(_headLookYHash, _headLookY);
            //_animator.SetFloat(_bodyLookXHash, _bodyLookX);
            //_animator.SetFloat(_bodyLookYHash, _bodyLookY);

            _animator.SetFloat(_isStrafingHash, _isStrafing ? 1.0f : 0.0f);

            _animator.SetFloat(_inclineAngleHash, _inclineAngle);

            _animator.SetFloat(_moveSpeedHash, _speed2D);
            _animator.SetInteger(id: _currentGaitHash, value: (int)_currentGait);

            _animator.SetFloat(_strafeDirectionXHash, _strafeDirectionX);
            _animator.SetFloat(_strafeDirectionZHash, _strafeDirectionZ);
            _animator.SetFloat(_forwardStrafeHash, _forwardStrafe);
            //_animator.SetFloat(_cameraRotationOffsetHash, _cameraRotationOffset);

            _animator.SetBool(_movementInputHeldHash, _movementInputHeld);
            _animator.SetBool(_movementInputPressedHash, _movementInputPressed);
            _animator.SetBool(_movementInputTappedHash, _movementInputTapped);
            _animator.SetFloat(_shuffleDirectionXHash, _shuffleDirectionX);
            _animator.SetFloat(_shuffleDirectionZHash, _shuffleDirectionZ);

            _animator.SetBool(_isTurningInPlaceHash, _isTurningInPlace);
            _animator.SetBool(_isCrouchingHash, IsCrouching);

            _animator.SetFloat(_fallingDurationHash, _fallingDuration);
            _animator.SetBool(_isGroundedHash, IsGrounded);

            _animator.SetBool(_isWalkingHash, IsWalking);
            _animator.SetBool(_isStoppedHash, _isStopped);

            _animator.SetFloat(_locomotionStartDirectionHash, _locomotionStartDirection);
        }

        /// <summary>
        /// Gets the normalised forward vector of the camera with the Y value zeroed.
        /// </summary>
        /// <returns>The normalised forward vector of the camera with the Y value zeroed.</returns>
        private Vector3 GetCameraForwardZeroedYNormalised() =>
            GetCameraForwardZeroedY().normalized;

        /// <summary>
        /// Gets the normalised right vector of the camera with the Y value zeroed.
        /// </summary>
        /// <returns>The normalised right vector of the camera with the Y value zeroed.</returns>
        private Vector3 GetCameraRightZeroedYNormalised() =>
            GetCameraRightZeroedY().normalized;

        /// <summary>
        /// Gets the forward vector of the camera with the Y value zeroed.
        /// </summary>
        /// <returns>The forward vector of the camera with the Y value zeroed.</returns>
        private Vector3 GetCameraForwardZeroedY()
        {
            Vector3 forward = _mainCamera.transform.forward;
            return new Vector3(forward.x, y: 0, forward.z);
        }

        /// <summary>
        /// Gets the right vector of the camera with the Y value zeroed.
        /// </summary>
        /// <returns>The right vector of the camera with the Y value zeroed.</returns>
        private Vector3 GetCameraRightZeroedY()
        {
            Vector3 right = _mainCamera.transform.right;
            return new Vector3(right.x, y: 0, right.z);
        }

        #endregion

        #region State Change

        private void SwitchState(AnimationState newState)
        {
            ExitCurrentState();
            EnterState(newState);
        }

        private void EnterState(AnimationState stateToEnter)
        {
            _currentState = stateToEnter;

            switch (_currentState)
            {
                case AnimationState.Base:
                    //EnterBaseState();
                    break;

                case AnimationState.Locomotion:
                    EnterLocomotionState();
                    break;

                case AnimationState.Jump:
                    EnterJumpState();
                    break;

                case AnimationState.Fall:
                    EnterFallState();
                    break;

                case AnimationState.Crouch:
                    EnterCrouchState();
                    break;
            }
        }

        private void ExitCurrentState()
        {
            switch (_currentState)
            {
                case AnimationState.Locomotion:
                    ExitLocomotionState();
                    break;

                case AnimationState.Jump:
                    ExitJumpState();
                    break;

                case AnimationState.Crouch:
                    ExitCrouchState();
                    break;
            }
        }

        #endregion

        #region Movement

        private void CalculateMoveDirection()
        {
            CalculateInput();

            if (!IsGrounded)
                _targetMaxSpeed = _currentMaxSpeed;
            else if (IsCrouching)
                _targetMaxSpeed = WalkSpeed;
            else if (IsSprinting)
                _targetMaxSpeed = SprintSpeed;
            else if (IsWalking)
                _targetMaxSpeed = WalkSpeed;
            else
                _targetMaxSpeed = RunSpeed;

            _currentMaxSpeed = Mathf.Lerp(
                a: _currentMaxSpeed,
                b: _targetMaxSpeed,
                t: AnimationDampTime * Time.deltaTime
            );

            _targetVelocity.x = _moveDirection.x * _currentMaxSpeed;
            _targetVelocity.z = _moveDirection.z * _currentMaxSpeed;

            _speed2D = new Vector3(Velocity.x, y: 0f, Velocity.z).magnitude;
            _speed2D = Mathf.Round(f: _speed2D * 1000f) / 1000f;

            Vector3 playerForwardVector = transform.forward;

            _newDirectionDifferenceAngle = playerForwardVector != _moveDirection
                ? Vector3.SignedAngle(from: playerForwardVector, to: _moveDirection, axis: Vector3.up)
                : 0f;

            CalculateGait();
        }

        private void CalculateInput()
        {
            if (_inputReader.MovementInputDetected)
            {
                if (_movementInputDuration == 0f)
                {
                    _movementInputTapped = true;
                }
                else if (_movementInputDuration > 0 && _movementInputDuration < _buttonHoldThreshold)
                {
                    _movementInputTapped = false;
                    _movementInputPressed = true;
                    _movementInputHeld = false;
                }
                else
                {
                    _movementInputTapped = false;
                    _movementInputPressed = false;
                    _movementInputHeld = true;
                }

                _movementInputDuration += Time.deltaTime;
            }
            else
            {
                _movementInputDuration = 0;
                _movementInputTapped = false;
                _movementInputPressed = false;
                _movementInputHeld = false;
            }

            Vector3 forwardZeroedYNormalised = GetCameraForwardZeroedYNormalised();
            Vector3 rightZeroedYNormalised = GetCameraRightZeroedYNormalised();

            _moveDirection = forwardZeroedYNormalised * _moveVector.y + rightZeroedYNormalised * _moveVector.x;
            _moveDirection = _character.GetMovementDirection();
        }

        private void CalculateGait()
        {
            float runThreshold = (WalkSpeed + RunSpeed) / 2f;
            float sprintThreshold = (RunSpeed + SprintSpeed) / 2f;

            if (_speed2D < 0.01f)
                _currentGait = GaitState.Idle;
            else if (_speed2D < runThreshold)
                _currentGait = GaitState.Walk;
            else if (_speed2D < sprintThreshold)
                _currentGait = GaitState.Run;
            else
                _currentGait = GaitState.Sprint;
        }

        private void CheckIfStarting()
        {
            _locomotionStartTimer = VariableOverrideDelayTimer(_locomotionStartTimer);

            bool isStartingCheck = false;

            if (_locomotionStartTimer <= 0.0f)
            {
                if (_moveDirection.magnitude > 0.01f && _speed2D < 1f && !_isStrafing)
                {
                    isStartingCheck = true;
                }

                if (isStartingCheck)
                {
                    if (!_isStarting)
                    {
                        _locomotionStartDirection = _newDirectionDifferenceAngle;
                        _animator.SetFloat(_locomotionStartDirectionHash, _locomotionStartDirection);
                    }

                    float delayTime = 0.2f;
                    //_leanDelay = delayTime;
                    //_headLookDelay = delayTime;
                    //_bodyLookDelay = delayTime;

                    _locomotionStartTimer = delayTime;
                }
            }
            else
            {
                isStartingCheck = true;
            }

            _isStarting = isStartingCheck;
            _animator.SetBool(_isStartingHash, _isStarting);
        }

        private void CheckIfStopped() =>
            _isStopped = _moveDirection.magnitude == 0f && _speed2D < .5f;

        private void UpdateStrafeDirection(float targetZ, float targetX)
        {
            _strafeDirectionZ = Mathf.Lerp(a: _strafeDirectionZ, b: targetZ, t: AnimationDampTime * Time.deltaTime);
            _strafeDirectionX = Mathf.Lerp(a: _strafeDirectionX, b: targetX, t: AnimationDampTime * Time.deltaTime);
            _strafeDirectionZ = Mathf.Round(f: _strafeDirectionZ * 1000f) / 1000f;
            _strafeDirectionX = Mathf.Round(f: _strafeDirectionX * 1000f) / 1000f;
        }

        private void FaceMoveDirection()
        {
            Transform thisTransform = transform;
            Vector3 forward = thisTransform.forward;
            Vector3 right = thisTransform.right;
            Vector3 characterForward = new Vector3(forward.x, y: 0f, forward.z).normalized;
            Vector3 characterRight = new Vector3(right.x, y: 0f, right.z).normalized;
            Vector3 directionForward = new Vector3(_moveDirection.x, y: 0f, _moveDirection.z).normalized;

            _cameraForward = GetCameraForwardZeroedYNormalised();
            Quaternion strafingTargetRotation = Quaternion.LookRotation(_cameraForward);

            _strafeAngle = characterForward != directionForward
                ? Vector3.SignedAngle(from: characterForward, to: directionForward, axis: Vector3.up)
                : 0f;

            if (_isStrafing)
            {
                if (_moveDirection.magnitude > 0.01f)
                {
                    if (_cameraForward != Vector3.zero)
                    {
                        // Shuffle direction values - these are separate from the strafe values as we
                        // don't want to lerp, we need to know immediately
                        // what direction to shuffle, and then lock the value so it doesn't return to zero once
                        // we lose input (so the blend tree works to the end of the anim clip)
                        _shuffleDirectionZ = Vector3.Dot(lhs: characterForward, rhs: directionForward);
                        _shuffleDirectionX = Vector3.Dot(lhs: characterRight, rhs: directionForward);

                        UpdateStrafeDirection(
                            targetZ: Vector3.Dot(lhs: characterForward, rhs: directionForward),
                            targetX: Vector3.Dot(lhs: characterRight, rhs: directionForward)
                        );

                        _cameraRotationOffset = Mathf.Lerp(
                            a: _cameraRotationOffset,
                            b: 0f,
                            t: _rotationSmoothing * Time.deltaTime
                        );

                        bool doStrafe = _strafeAngle > _forwardStrafeMinThreshold &&
                                        _strafeAngle < _forwardStrafeMaxThreshold;

                        float targetValue = doStrafe ? 1f : 0f;

                        if (Mathf.Abs(_forwardStrafe - targetValue) <= 0.001f)
                        {
                            _forwardStrafe = targetValue;
                        }
                        else
                        {
                            float t = Mathf.Clamp01(StrafeDirectionDampTime * Time.deltaTime);
                            _forwardStrafe = Mathf.SmoothStep(_forwardStrafe, targetValue, t);
                        }
                    }

                    // thisTransform.rotation = Quaternion.Slerp(
                    //     a: thisTransform.rotation,
                    //     b: strafingTargetRotation,
                    //     t: _rotationSmoothing * Time.deltaTime
                    // );
                }
                else
                {
                    UpdateStrafeDirection(targetZ: 1f, targetX: 0f);

                    float t = 20 * Time.deltaTime;
                    float newOffset = 0f;

                    if (characterForward != _cameraForward)
                        newOffset = Vector3.SignedAngle(from: characterForward, to: _cameraForward, axis: Vector3.up);

                    _cameraRotationOffset = Mathf.Lerp(_cameraRotationOffset, newOffset, t);

                    if (Mathf.Abs(_cameraRotationOffset) > 10f)
                        _isTurningInPlace = true;
                }
            }
            else
            {
                UpdateStrafeDirection(targetZ: 1f, targetX: 0f);

                _cameraRotationOffset = Mathf.Lerp(
                    a: _cameraRotationOffset,
                    b: 0f,
                    t: _rotationSmoothing * Time.deltaTime
                );

                _shuffleDirectionZ = 1;
                _shuffleDirectionX = 0;

                Vector3 faceDirection = new(Velocity.x, y: 0f, Velocity.z);

                if (faceDirection == Vector3.zero)
                    return;

                // thisTransform.rotation = Quaternion.Slerp(
                //     a: transform.rotation,
                //     b: Quaternion.LookRotation(faceDirection),
                //     t: _rotationSmoothing * Time.deltaTime
                // );
            }
        }

        #endregion

        #region Ground Checks

        private void GroundedCheck()
        {
            if (!IsGrounded)
                return;

            GroundInclineCheck();
        }

        private void GroundInclineCheck()
        {
            const float rayDistance = Mathf.Infinity;
            float rotationX = transform.rotation.x;
            _rearRayPos.rotation = Quaternion.Euler(rotationX, y: 0f, z: 0f);
            _frontRayPos.rotation = Quaternion.Euler(rotationX, y: 0f, z: 0f);

            Physics.Raycast(
                origin: _rearRayPos.position,
                direction: _rearRayPos.TransformDirection(-Vector3.up),
                hitInfo: out RaycastHit rearHit,
                maxDistance: rayDistance,
                layerMask: _groundLayerMask
            );

            Physics.Raycast(
                origin: _frontRayPos.position,
                direction: _frontRayPos.TransformDirection(-Vector3.up),
                hitInfo: out RaycastHit frontHit,
                maxDistance: rayDistance,
                layerMask: _groundLayerMask
            );

            Vector3 hitDifference = frontHit.point - rearHit.point;
            float xPlaneLength = new Vector2(hitDifference.x, hitDifference.z).magnitude;

            _inclineAngle = Mathf.Lerp(
                a: _inclineAngle,
                b: Mathf.Atan2(hitDifference.y, xPlaneLength) * Mathf.Rad2Deg,
                t: 20f * Time.deltaTime
            );
        }

        #endregion

        #region Lean and Offsets

        private void CheckEnableTurns()
        {
            _headLookDelay = VariableOverrideDelayTimer(_headLookDelay);
            _enableHeadTurn = _headLookDelay == 0.0f && !_isStarting;
            _bodyLookDelay = VariableOverrideDelayTimer(_bodyLookDelay);
            _enableBodyTurn = _bodyLookDelay == 0.0f && !(_isStarting || _isTurningInPlace);
        }
        
        private void CheckEnableLean()
        {
            _leanDelay = VariableOverrideDelayTimer(_leanDelay);
            _enableLean = _leanDelay == 0.0f && !(_isStarting || _isTurningInPlace);
        }
        
        private void CalculateRotationalAdditives(bool leansActivated, bool headLookActivated, bool bodyLookActivated)
        {
            if (headLookActivated || leansActivated || bodyLookActivated)
            {
                _currentRotation = transform.forward;

                _rotationRate = _currentRotation != _previousRotation
                    ? Vector3.SignedAngle(_currentRotation, _previousRotation, Vector3.up) / Time.deltaTime * -1f
                    : 0f;
            }

            _initialLeanValue = leansActivated ? _rotationRate : 0f;

            float leanSmoothness = 5;
            float maxLeanRotationRate = 275.0f;

            float referenceValue = _speed2D / SprintSpeed;
            _leanValue = CalculateSmoothedValue(
                _leanValue,
                _initialLeanValue,
                maxLeanRotationRate,
                leanSmoothness,
                _leanCurve,
                referenceValue,
                true
            );

            float headTurnSmoothness = 5f;

            if (headLookActivated && _isTurningInPlace)
            {
                _initialTurnValue = _cameraRotationOffset;
                _headLookX = Mathf.Lerp(_headLookX, _initialTurnValue / 200, 5f * Time.deltaTime);
            }
            else
            {
                _initialTurnValue = headLookActivated ? _rotationRate : 0f;
                _headLookX = CalculateSmoothedValue(
                    _headLookX,
                    _initialTurnValue,
                    maxLeanRotationRate,
                    headTurnSmoothness,
                    _headLookXCurve,
                    _headLookX,
                    false
                );
            }

            float bodyTurnSmoothness = 5f;

            _initialTurnValue = bodyLookActivated ? _rotationRate : 0f;

            _bodyLookX = CalculateSmoothedValue(
                _bodyLookX,
                _initialTurnValue,
                maxLeanRotationRate,
                bodyTurnSmoothness,
                _bodyLookXCurve,
                _bodyLookX,
                false
            );

            float cameraTilt = GetCameraTiltX();
            cameraTilt = (cameraTilt > 180f ? cameraTilt - 360f : cameraTilt) / -180;
            cameraTilt = Mathf.Clamp(cameraTilt, -0.1f, 1.0f);
            _headLookY = cameraTilt;
            _bodyLookY = cameraTilt;

            _previousRotation = _currentRotation;
        }

        private float GetCameraTiltX() =>
            _mainCamera.transform.eulerAngles.x;

        private static float CalculateSmoothedValue(float mainVariable, float newValue, float maxRateChange,
            float smoothness, AnimationCurve referenceCurve, float referenceValue, bool isMultiplier
        )
        {
            float changeVariable = newValue / maxRateChange;

            changeVariable = Mathf.Clamp(value: changeVariable, min: -1.0f, max: 1.0f);

            if (isMultiplier)
            {
                float multiplier = referenceCurve.Evaluate(referenceValue);
                changeVariable *= multiplier;
            }
            else
            {
                changeVariable = referenceCurve.Evaluate(changeVariable);
            }

            if (!changeVariable.Equals(mainVariable))
                changeVariable = Mathf.Lerp(mainVariable, changeVariable, smoothness * Time.deltaTime);

            return changeVariable;
        }

        private static float VariableOverrideDelayTimer(float timeVariable)
        {
            if (timeVariable > 0.0f)
            {
                timeVariable -= Time.deltaTime;
                timeVariable = Mathf.Clamp(value: timeVariable, min: 0.0f, max: 1.0f);
            }
            else
            {
                timeVariable = 0.0f;
            }

            return timeVariable;
        }

        #endregion

        #region Locomotion state

        private void EnterLocomotionState() =>
            _character.Jumped += LocomotionToJumpState;

        private void UpdateLocomotionState()
        {
            GroundedCheck();

            if (!IsGrounded)
                SwitchState(AnimationState.Fall);

            if (IsCrouching)
                SwitchState(AnimationState.Crouch);
            
            CheckEnableTurns();
            CheckEnableLean();
            CalculateRotationalAdditives(_enableLean, _enableHeadTurn, _enableBodyTurn);

            CalculateMoveDirection();
            CheckIfStarting();
            CheckIfStopped();
            FaceMoveDirection();
            UpdateAnimatorController();
        }

        private void ExitLocomotionState() =>
            _character.Jumped -= LocomotionToJumpState;

        private void LocomotionToJumpState() => SwitchState(AnimationState.Jump);

        #endregion

        #region Jump State

        private void EnterJumpState() =>
            _animator.SetBool(_isJumpingAnimHash, value: true);

        private void UpdateJumpState()
        {
            if (Velocity.y <= 0f)
            {
                _animator.SetBool(_isJumpingAnimHash, value: false);
                SwitchState(AnimationState.Fall);
            }

            GroundedCheck();

            CalculateRotationalAdditives(false, _enableHeadTurn, _enableBodyTurn);
            CalculateMoveDirection();
            FaceMoveDirection();
            UpdateAnimatorController();
        }

        private void ExitJumpState() =>
            _animator.SetBool(_isJumpingAnimHash, value: false);

        #endregion

        #region Falling

        private void ResetFallingDuration()
        {
            _fallStartTime = Time.time;
            _fallingDuration = 0f;
        }

        private void UpdateFallingDuration() =>
            _fallingDuration = Time.time - _fallStartTime;

        #endregion

        #region Fall State

        private void EnterFallState() => ResetFallingDuration();

        private void UpdateFallState()
        {
            GroundedCheck();

            CalculateRotationalAdditives(false, _enableHeadTurn, _enableBodyTurn);
            CalculateMoveDirection();
            FaceMoveDirection();

            UpdateAnimatorController();

            if (IsGrounded)
                SwitchState(AnimationState.Locomotion);

            UpdateFallingDuration();
        }

        #endregion

        #region Crouch State

        private void EnterCrouchState() =>
            _character.UnCrouched += CrouchToJumpState;

        private void UpdateCrouchState()
        {
            GroundedCheck();

            if (!IsGrounded)
                SwitchState(AnimationState.Fall);

            if (!IsCrouching)
                SwitchState(AnimationState.Locomotion);
            
            CheckEnableTurns();
            CheckEnableLean();
            CalculateRotationalAdditives(false, _enableHeadTurn, false);

            CalculateMoveDirection();
            CheckIfStarting();
            CheckIfStopped();

            FaceMoveDirection();
            UpdateAnimatorController();
        }

        private void ExitCrouchState() =>
            _character.UnCrouched -= CrouchToJumpState;

        private void CrouchToJumpState() => SwitchState(AnimationState.Jump);

        #endregion
    }
}