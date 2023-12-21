using NetcodePlus;
using NetcodePlus.Demo;
using System.Collections;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Player
{
    public class PlayerCamera : MonoBehaviour
    {
        public float maxVerticalAngle;
        public float sensitivity;

        private Transform _body;
        private float _mouseVerticalValue;
        private bool _isPlayerFound;

        private float MouseVerticalValue
        {
            get => _mouseVerticalValue;
            set
            {
                if (value == 0) return;

                float verticalAngle = _mouseVerticalValue + value;
                verticalAngle = Mathf.Clamp(verticalAngle, -maxVerticalAngle, maxVerticalAngle);
                _mouseVerticalValue = verticalAngle;
            }
        }

        private void LateUpdate()
        {
            if (!_isPlayerFound)
            {
                SNetworkPlayer first = SNetworkPlayer.GetSelf();

                if (first != null)
                {
                    _isPlayerFound = true;
                    _body = first.transform;
                }

                return;
            }

            MouseVerticalValue = Input.GetAxis("Mouse Y");
            float yRotation = _body.localRotation.eulerAngles.y + Input.GetAxis("Mouse X") * sensitivity;

            Quaternion finalRotation = Quaternion.Euler(-MouseVerticalValue * sensitivity, yRotation, 0);

            transform.position = _body.position;
            transform.localRotation = finalRotation;

            _body.rotation = Quaternion.Euler(0, yRotation, 0);

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
    }
}