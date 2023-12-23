using GameCore.Gameplay.Managers;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Player
{
    public class PlayerCamera : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField, Min(0)]
        private float _maxVerticalAngle = 60f;

        [SerializeField, Min(0.1f)]
        private float _sensitivity = 1f;

        // PROPERTIES: ----------------------------------------------------------------------------

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

        public static PlayerCamera Instance;

        private Transform _transform;
        private Transform _target;
        private PlayerInputActions _playerInputActions;

        private Vector2 _lookVector;
        private float _mouseVerticalValue;
        private bool _isPlayerFound;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            Instance = this;

        private void LateUpdate()
        {
            if (!_isPlayerFound)
                return;
            
            _lookVector = _playerInputActions.Player.Look.ReadValue<Vector2>();
            MouseVerticalValue = _lookVector.y;
            
            float yRotation = _target.localRotation.eulerAngles.y + _lookVector.x * _sensitivity;
            Quaternion finalRotation = Quaternion.Euler(-MouseVerticalValue, yRotation, 0);

            _transform.position = _target.position;
            _transform.localRotation = finalRotation;

            _target.rotation = Quaternion.Euler(0, yRotation, 0);
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetTarget(Transform target)
        {
            _transform = transform;
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

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnLook(Vector2 lookVector) =>
            _lookVector = lookVector;
    }
}