using GameCore.Gameplay.Entities.MobileHeadquarters;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.InputHandlerTEMP;
using GameCore.Infrastructure.Providers.Global;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.PlayerCamera
{
    public class CameraController : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IConfigsProvider configsProvider) =>
            _inputReader = configsProvider.GetInputReader();

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField, Min(0)]
        private float _maxVerticalAngle = 60f;

        [SerializeField, Min(0.1f)]
        private float _sensitivity = 1f;
        
        [Title(Constants.References)]
        [SerializeField]
        private CameraReferences _cameraReferences;

        // PROPERTIES: ----------------------------------------------------------------------------

        public CameraReferences CameraReferences => _cameraReferences;
        
        private float MouseVerticalValue
        {
            get => _mouseVerticalValue;
            set
            {
                if (value == 0)
                    return;

                float verticalAngle = _mouseVerticalValue + value * _sensitivity;
                float min = -_maxVerticalAngle;
                float max = _maxVerticalAngle;
                verticalAngle = Mathf.Clamp(value: verticalAngle, min, max);
                _mouseVerticalValue = verticalAngle;
            }
        }
        
        // FIELDS: --------------------------------------------------------------------------------

        private static CameraController _instance;
        
        private InputReader _inputReader;
        private PlayerEntity _playerEntity;
        private Transform _transform;
        private Transform _target;
        private Transform _headPoint;
        private Transform _mobileHqTransform;
        
        private Vector3 _lastFrameMobileHqRotation;
        private Vector2 _lookVector;
        private float _mouseVerticalValue;
        private bool _isInitialized;

        // GAME ENGINE METHODS: -------------------------------------------------------------------
        
        private void Awake()
        {
            _instance = this;
            _transform = transform;
            
            _inputReader.OnLookEvent += OnLook;
        }

        private void OnDestroy() =>
            _inputReader.OnLookEvent -= OnLook;

        private void LateUpdate()
        {
            if (!_isInitialized)
                return;
            
            _lookVector = _inputReader.GameInput.Gameplay.Look.ReadValue<Vector2>();
            MouseVerticalValue = _lookVector.y;
            Vector3 mobileHeadquartersRotation = _mobileHqTransform.rotation.eulerAngles;

            float yRotationTemp = _target.rotation.eulerAngles.y;

            if (_playerEntity.IsInsideMobileHQ)
            {
                Vector3 difference = mobileHeadquartersRotation - _lastFrameMobileHqRotation;
                yRotationTemp += difference.y;
            }
            
            float yRotation = yRotationTemp + _lookVector.x * _sensitivity;
            
            Quaternion finalRotation = Quaternion.Euler(-MouseVerticalValue, yRotation, 0);

            _transform.position = _headPoint.position;
            _transform.localRotation = finalRotation;

            _target.rotation = Quaternion.Euler(0, yRotation, 0);

            _lastFrameMobileHqRotation = mobileHeadquartersRotation;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Init(PlayerEntity playerEntity)
        {
            _playerEntity = playerEntity;
            _target = playerEntity.transform;
            _headPoint = playerEntity.References.HeadPoint;
            _isInitialized = true;

            MobileHeadquartersEntity mobileHeadquartersEntity = MobileHeadquartersEntity.Get();
            _mobileHqTransform = mobileHeadquartersEntity.transform;
        }
        
        public static CameraController Get() => _instance;

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnLook(Vector2 lookVector) =>
            _lookVector = lookVector;
    }
}