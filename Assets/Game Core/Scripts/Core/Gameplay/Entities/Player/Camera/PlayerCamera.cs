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

                float verticalAngle = _mouseVerticalValue + value;
                verticalAngle = Mathf.Clamp(verticalAngle, -_maxVerticalAngle, _maxVerticalAngle);
                _mouseVerticalValue = verticalAngle;
            }
        }

        // FIELDS: --------------------------------------------------------------------------------

        public static PlayerCamera Instance;

        private Transform _target;
        private float _mouseVerticalValue;
        private bool _isPlayerFound;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            Instance = this;

        private void LateUpdate()
        {
            if (!_isPlayerFound)
                return;

            MouseVerticalValue = Input.GetAxis("Mouse Y");
            float yRotation = _target.localRotation.eulerAngles.y + Input.GetAxis("Mouse X") * _sensitivity;

            Quaternion finalRotation = Quaternion.Euler(-MouseVerticalValue * _sensitivity, yRotation, 0);

            transform.position = _target.position;
            transform.localRotation = finalRotation;

            _target.rotation = Quaternion.Euler(0, yRotation, 0);

            if (Input.GetMouseButtonDown(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetTarget(Transform target)
        {
            _target = target;
            _isPlayerFound = true;
        }
    }
}