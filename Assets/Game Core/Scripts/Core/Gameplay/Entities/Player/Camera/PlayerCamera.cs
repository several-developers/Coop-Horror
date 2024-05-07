using GameCore.Gameplay.InputManagement;
using GameCore.Infrastructure.Providers.Global;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Entities.Player.CameraManagement
{
    public class PlayerCamera : MonoBehaviour
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

        [SerializeField, Min(0f)]
        private float _cameraSpeed = 10f;

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

        private InputReader _inputReader;
        private Transform _transform;
        private Transform _target;
        private Transform _headPoint;

        private Vector2 _lookVector;
        private float _mouseVerticalValue;
        private bool _snap;
        private bool _isInitialized;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _transform = transform;

        private void Start()
        {
            _transform.SetParent(null);
            _transform.SetAsLastSibling();
        }

        private void LateUpdate()
        {
            if (!_isInitialized)
                return;

            _lookVector = _inputReader.GameInput.Gameplay.Look.ReadValue<Vector2>();
            MouseVerticalValue = _lookVector.y;

            float yRotation = _target.rotation.eulerAngles.y + _lookVector.x * _sensitivity;
            Quaternion finalRotation = Quaternion.Euler(-MouseVerticalValue, yRotation, 0);

            if (_snap)
            {
                _snap = false;
                _transform.position = _headPoint.position;
            }
            else
            {
                _transform.position =
                    Vector3.Lerp(_transform.position, _headPoint.position, _cameraSpeed * Time.deltaTime);
            }

            //_transform.localRotation = finalRotation;
            //_target.rotation = Quaternion.Euler(0, yRotation, 0);
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void EnableSnap() =>
            _snap = true;
    }
}