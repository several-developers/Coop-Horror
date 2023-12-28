﻿using GameCore.Gameplay.Managers;
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

        // PROPERTIES: ----------------------------------------------------------------------------

        public Camera Camera => _camera;
        
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
        private PlayerInputActions _playerInputActions;

        private Vector2 _lookVector;
        private float _mouseVerticalValue;
        private bool _isPlayerFound;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _instance = this;

        private void LateUpdate()
        {
            if (!_isPlayerFound)
                return;
            
            _lookVector = _playerInputActions.Player.Look.ReadValue<Vector2>();
            MouseVerticalValue = _lookVector.y;
            
            float yRotation = _target.localRotation.eulerAngles.y + _lookVector.x * _sensitivity;
            Quaternion finalRotation = Quaternion.Euler(-MouseVerticalValue, yRotation, 0);

            _transform.position = _cameraPoint.position;
            _transform.localRotation = finalRotation;

            _target.rotation = Quaternion.Euler(0, yRotation, 0);
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