using ECM2;
using GameCore.Gameplay.Systems.InputManagement;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Player
{
    /// <summary>
    /// This example shows how to implement a Cinemachine-based first person controller,
    /// using Cinemachine Virtual Camera’s 3rd Person Follow.
    ///
    /// Additionally, shows how to implement a Crouch / UnCrouch animation.
    ///
    /// Must be added to a Character.
    /// </summary>
    public class PlayerMovementController : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title("Cinemachine")]
        [SerializeField]
        [Tooltip("How far in degrees can you move the camera up.")]
        private float _maxPitch = 80.0f;

        [SerializeField]
        [Tooltip("How far in degrees can you move the camera down.")]
        private float _minPitch = -80.0f;

        [SerializeField]
        [Tooltip("Mouse look sensitivity")]
        private Vector2 _lookSensitivity = new(x: 1.5f, y: 1.25f);

        [Title(Constants.References)]
        [SerializeField, Required]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow.")]
        private Transform _cameraTarget;

        [SerializeField, Required]
        [Tooltip("Cinemachine Virtual Camera positioned at desired crouched height.")]
        private GameObject _crouchedCamera;

        [SerializeField, Required]
        [Tooltip("Cinemachine Virtual Camera positioned at desired un-crouched height.")]
        private GameObject _unCrouchedCamera;

        // FIELDS: --------------------------------------------------------------------------------

        private PlayerEntity _playerEntity;
        private InputReader _inputReader;
        private Character _character;
        private MySprintAbility _sprintAbility;
        private Vector2 _moveInput;
        private Vector2 _lookVector;

        // Current camera target pitch
        private float _cameraTargetPitch;

        private bool _isInitialized;
        private bool _performJump;
        private bool _cancelJump;
        private bool _performCrouch;
        private bool _cancelCrouch;

        private bool _isEnabled;
        private bool _canMove;
        private bool _isCameraEnabled;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _character = GetComponent<Character>();
            _sprintAbility = GetComponent<MySprintAbility>();
            _crouchedCamera.SetActive(false);
            _unCrouchedCamera.SetActive(false);
        }

        private void Start()
        {
            // Disable Character's rotation mode, we'll handle it here
            _character.SetRotationMode(Character.RotationMode.None);
        }

        private void Update()
        {
            if (!_isInitialized || !_isEnabled)
                return;

            // Movement direction relative to Character's forward

            Vector3 movementDirection = Vector3.zero;

            movementDirection += _character.GetRightVector() * _moveInput.x;
            movementDirection += _character.GetForwardVector() * _moveInput.y;

            // Set Character movement direction

            if (_canMove)
                _character.SetMovementDirection(movementDirection);

            if (_isCameraEnabled)
            {
                // Look input
            
                _lookVector = _inputReader.GameInput.Gameplay.Look.ReadValue<Vector2>();
                
                // Add yaw input, this update character's yaw rotation

                AddControlYawInput(_lookVector.x * _lookSensitivity.x);
                
                // Add pitch input (look up / look down), this update cameraTarget's local rotation

                AddControlPitchInput(value: _lookVector.y * _lookSensitivity.y, _minPitch, _maxPitch);
            }

            if (!_canMove)
                return;

            // Crouch input

            if (_performCrouch)
            {
                _performCrouch = false;
                _character.Crouch();
            }
            else if (_cancelCrouch)
            {
                _cancelCrouch = false;
                _character.UnCrouch();
            }

            // Jump input

            if (_performJump)
            {
                _performJump = false;
                _character.Jump();
            }
            else if (_cancelJump)
            {
                _cancelJump = false;
                _character.StopJumping();
            }
        }

        private void OnDestroy()
        {
            if (!_isInitialized)
                return;

            _inputReader.OnMoveEvent -= OnMove;
            _inputReader.OnJumpEvent -= OnJump;
            _inputReader.OnJumpCanceledEvent -= OnJumpCanceled;
            _inputReader.OnCrouchEvent -= OnCrouch;
            _inputReader.OnCrouchCanceledEvent -= OnCrouchCanceled;
            _inputReader.OnSprintEvent -= OnSprint;
            _inputReader.OnSprintCanceledEvent -= OnSprintCanceled;
        }

        private void OnEnable()
        {
            // Subscribe to Character events

            _character.Crouched += OnCrouched;
            _character.UnCrouched += OnUnCrouched;
        }

        private void OnDisable()
        {
            // Unsubscribe to Character events

            _character.Crouched -= OnCrouched;
            _character.UnCrouched -= OnUnCrouched;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Setup(PlayerEntity playerEntity)
        {
            _isInitialized = true;
            _isEnabled = true;
            _isCameraEnabled = true;
            _canMove = true;
            _playerEntity = playerEntity;
            _inputReader = playerEntity.InputReader;

            EnableCamera();
            
            _crouchedCamera.transform.SetParent(null);
            _unCrouchedCamera.transform.SetParent(null);

            _playerEntity.IsCrouching += IsCrouching;
            _playerEntity.IsSprinting += IsSprinting;

            _inputReader.OnMoveEvent += OnMove;
            _inputReader.OnJumpEvent += OnJump;
            _inputReader.OnJumpCanceledEvent += OnJumpCanceled;
            _inputReader.OnCrouchEvent += OnCrouch;
            _inputReader.OnCrouchCanceledEvent += OnCrouchCanceled;
            _inputReader.OnSprintEvent += OnSprint;
            _inputReader.OnSprintCanceledEvent += OnSprintCanceled;
        }

        public void ToggleActiveState(bool isEnabled) =>
            _isEnabled = isEnabled;

        public void ToggleMovementState(bool canMove) =>
            _canMove = canMove;

        public void ToggleCameraState(bool isEnabled) =>
            _isCameraEnabled = isEnabled;

        public void DisableAllCameras()
        {
            _crouchedCamera.SetActive(false);
            _unCrouchedCamera.SetActive(false);
        }

        public void EnableCamera()
        {
            bool isCrouched = _character.IsCrouched();
            _crouchedCamera.SetActive(isCrouched);
            _unCrouchedCamera.SetActive(!isCrouched);
        }

        public void ResetCameraTarget()
        {
            _cameraTargetPitch = 0f;
            _cameraTarget.localRotation = Quaternion.Euler(Vector3.zero);
        }

        public void SetCameraTargetPitch(float value) =>
            _cameraTargetPitch = value;

        public Transform GetCameraTarget() => _cameraTarget;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        /// <summary>
        /// Add input (affecting Yaw).
        /// This is applied to the Character's rotation.
        /// </summary>
        public void AddControlYawInput(float value) =>
            _character.AddYawInput(value);

        /// <summary>
        /// Add input (affecting Pitch).
        /// This is applied to the cameraTarget's local rotation.
        /// </summary>
        private void AddControlPitchInput(float value, float minValue = -80.0f, float maxValue = 80.0f)
        {
            if (value == 0.0f)
                return;

            _cameraTargetPitch = MathLib.ClampAngle(_cameraTargetPitch + value, minValue, maxValue);
            _cameraTarget.localRotation = Quaternion.Euler(-_cameraTargetPitch, 0.0f, 0.0f);
        }

        private bool IsCrouching() =>
            _character.IsCrouched();

        private bool IsSprinting() =>
            _sprintAbility.IsSprinting();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnMove(Vector2 movementVector) =>
            _moveInput = movementVector;

        private void OnJump()
        {
            if (!_isEnabled || !_canMove)
                return;

            _performJump = true;
        }

        private void OnJumpCanceled()
        {
            if (!_isEnabled || !_canMove)
                return;

            _cancelJump = true;
        }

        private void OnCrouch()
        {
            if (!_isEnabled || !_canMove)
                return;

            _performCrouch = true;
        }

        private void OnCrouchCanceled()
        {
            if (!_isEnabled || !_canMove)
                return;

            _cancelCrouch = true;
        }

        private void OnSprint()
        {
            if (!_isEnabled || !_canMove)
                return;

            _sprintAbility.Sprint();
        }

        private void OnSprintCanceled()
        {
            if (!_isEnabled || !_canMove)
                return;

            _sprintAbility.StopSprinting();
        }

        /// <summary>
        /// When character crouches, toggle Crouched / UnCrouched cameras.
        /// </summary>
        private void OnCrouched()
        {
            _crouchedCamera.SetActive(true);
            _unCrouchedCamera.SetActive(false);
        }

        /// <summary>
        /// When character un-crouches, toggle Crouched / UnCrouched cameras.
        /// </summary>
        private void OnUnCrouched()
        {
            _crouchedCamera.SetActive(false);
            _unCrouchedCamera.SetActive(true);
        }
    }
}