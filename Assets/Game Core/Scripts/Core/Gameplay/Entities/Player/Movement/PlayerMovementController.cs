using ECM2;
using GameCore.Gameplay.InputManagement;
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
        private GameObject _cameraTarget;

        [SerializeField, Required]
        [Tooltip("Cinemachine Virtual Camera positioned at desired crouched height.")]
        private GameObject _crouchedCamera;

        [SerializeField, Required]
        [Tooltip("Cinemachine Virtual Camera positioned at desired un-crouched height.")]
        private GameObject _unCrouchedCamera;

        // FIELDS: --------------------------------------------------------------------------------

        private InputReader _inputReader;
        private Character _character;
        private MySprintAbility _sprintAbility;
        private Vector2 _moveInput;
        private Vector2 _lookVector;
        
        // Current camera target pitch
        private float _cameraTargetPitch;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _character = GetComponent<Character>();
            _sprintAbility = GetComponent<MySprintAbility>();
            _crouchedCamera.transform.SetParent(null);
            _unCrouchedCamera.transform.SetParent(null);
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;

            // Disable Character's rotation mode, we'll handle it here

            _character.SetRotationMode(Character.RotationMode.None);
        }

        private void Update()
        {
            // Movement direction relative to Character's forward

            Vector3 movementDirection = Vector3.zero;

            movementDirection += _character.GetRightVector() * _moveInput.x;
            movementDirection += _character.GetForwardVector() * _moveInput.y;

            // Set Character movement direction

            _character.SetMovementDirection(movementDirection);

            // Look input

            _lookVector = _inputReader.GameInput.Gameplay.Look.ReadValue<Vector2>();

            // Add yaw input, this update character's yaw rotation

            AddControlYawInput(_lookVector.x * _lookSensitivity.x);

            // Add pitch input (look up / look down), this update cameraTarget's local rotation

            AddControlPitchInput(_lookVector.y * _lookSensitivity.y, _minPitch, _maxPitch);

            // Crouch input

            if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.C))
                _character.Crouch();
            else if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.C))
                _character.UnCrouch();

            // Jump input

            if (Input.GetButtonDown("Jump"))
                _character.Jump();
            else if (Input.GetButtonUp("Jump"))
                _character.StopJumping();
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
            PlayerReferences playerReferences = playerEntity.References;
            _inputReader = playerReferences.InputReader; // TEMP
            
            _inputReader.OnMoveEvent += OnMove;
            _inputReader.OnLookEvent += OnLook;
            _inputReader.OnSprintEvent += OnSprint;
            _inputReader.OnSprintCanceledEvent += OnSprintCanceled;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        /// <summary>
        /// Add input (affecting Yaw).
        /// This is applied to the Character's rotation.
        /// </summary>
        private void AddControlYawInput(float value) =>
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
            _cameraTarget.transform.localRotation = Quaternion.Euler(-_cameraTargetPitch, 0.0f, 0.0f);
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

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnMove(Vector2 movementVector) =>
            _moveInput = movementVector;
        
        private void OnLook(Vector2 lookVector) =>
            _lookVector = lookVector;

        private void OnSprint() =>
            _sprintAbility.Sprint();

        private void OnSprintCanceled() =>
            _sprintAbility.StopSprinting();
    }
}