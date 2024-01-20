using EFPController;
using GameCore.Gameplay.Entities.MobileHeadquarters;
using GameCore.Gameplay.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Player
{
    public class PlayerCamera : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField, Min(0)]
        private float _maxVerticalAngle = 60f;

        [SerializeField, Min(0.1f)]
        private float _sensitivity = 1f;

        [Title(Constants.References)]
        [SerializeField, Required]
        private Camera _camera;

        [SerializeField, Required]
        private GameObject _weaponRoot;

        [SerializeField, Required]
        private AudioListener _audioListener;

        [SerializeField, Required]
        private SmoothLook _smoothLook;
        
        [SerializeField, Required]
        private CameraRootAnimations _cameraRootAnimations;
        
        [SerializeField, Required]
        private CameraControl _cameraControl;
        
        [SerializeField, Required]
        private Animator _cameraAnimator;

        [SerializeField, Required]
        private Transform _itemPivot;
        
        [SerializeField, Required]
        private Transform _lookAtObject;
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public Camera Camera => _camera;
        public GameObject WeaponRoot => _weaponRoot;
        public GameObject CameraRoot => gameObject;
        public AudioListener AudioListener => _audioListener;
        public SmoothLook SmoothLook => _smoothLook;
        public CameraRootAnimations CameraRootAnimations => _cameraRootAnimations;
        public CameraControl CameraControl => _cameraControl;
        public Animator CameraAnimator => _cameraAnimator;
        public Transform ItemPivot => _itemPivot;
        public Transform LookAtObject => _lookAtObject;

        private float MouseVerticalValue
        {
            get => _mouseVerticalValue;
            set
            {
                if (value == 0) return;

                float verticalAngle = _mouseVerticalValue + value * _sensitivity;
                verticalAngle = Mathf.Clamp(verticalAngle, -_maxVerticalAngle, _maxVerticalAngle);
                _mouseVerticalValue = verticalAngle;
            }
        }

        // FIELDS: --------------------------------------------------------------------------------

        private static PlayerCamera _instance;

        private Transform _transform;
        private Transform _target;
        private Transform _cameraPoint;
        private Transform _mobileHeadquartersTransform;
        private PlayerInputActions _playerInputActions;

        private Vector3 _lastFrameMobileHeadquartersRotation;
        private Vector2 _lookVector;
        private float _mouseVerticalValue;
        private bool _isPlayerFound;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _instance = this;

        private void Start()
        {
            //_mobileHeadquartersTransform = MobileHeadquartersEntity.Get().transform;
            //_lastFrameMobileHeadquartersRotation = _mobileHeadquartersTransform.rotation.eulerAngles;
        }

        private void LateUpdate()
        {
            return;
            if (!_isPlayerFound)
                return;

            _lookVector = _playerInputActions.Player.Look.ReadValue<Vector2>();
            MouseVerticalValue = _lookVector.y;

            Vector3 mobileHeadquartersRotation = _mobileHeadquartersTransform.rotation.eulerAngles;
            Vector3 difference = mobileHeadquartersRotation - _lastFrameMobileHeadquartersRotation;

            float yRotation = _target.rotation.eulerAngles.y + difference.y + _lookVector.x * _sensitivity;
            Quaternion finalRotation = Quaternion.Euler(-MouseVerticalValue, yRotation, 0);

            _transform.position = _cameraPoint.position;
            _transform.localRotation = finalRotation;

            _target.rotation = Quaternion.Euler(0, yRotation, 0);

            _lastFrameMobileHeadquartersRotation = mobileHeadquartersRotation;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetTarget(Transform target, Transform cameraPoint)
        {
            _transform = transform;
            _cameraPoint = cameraPoint;
            _target = target;
            _playerInputActions = InputSystemManager.GetPlayerInputActions();
            _isPlayerFound = true;

            InputSystemManager.OnLookEvent += OnLook;
        }

        public void RemoveTarget()
        {
            _target = null;
            _isPlayerFound = false;
        }

        public static PlayerCamera Get() => _instance;

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnLook(Vector2 lookVector) =>
            _lookVector = lookVector;
    }
}