using UnityEngine;

namespace GameCore.Gameplay.Entities.Player.Rotation
{
    public class FreeRotationBehaviour : IRotationBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public FreeRotationBehaviour(PlayerEntity playerEntity, Transform cinemachineCameraTarget)
        {
            _transform = playerEntity.transform;
            _cinemachineCameraTarget = cinemachineCameraTarget;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private const float Threshold = 0.01f;

        private readonly Transform _transform;
        private readonly Transform _cinemachineCameraTarget;

        private Vector2 _lookVector;
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;
        private bool _lockCameraPosition;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Rotate()
        {
            float sensitivity = 1;
            float bottomClamp = -60;
            float topClamp = 70;
            float cameraAngleOverride = 1;
            float deltaTime = Time.deltaTime;

            // if there is an input and camera position is not fixed
            if (_lookVector.sqrMagnitude >= Threshold && !_lockCameraPosition)
            {
                _cinemachineTargetYaw += _lookVector.x * deltaTime * sensitivity;
                _cinemachineTargetPitch += _lookVector.y * deltaTime * sensitivity;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, bottomClamp, topClamp);

            // Cinemachine will follow this target
            float x = _cinemachineTargetPitch + cameraAngleOverride;
            float y = _cinemachineTargetYaw;

            //_cinemachineCameraTarget.rotation = Quaternion.Euler(x, y, 0f);
            _cinemachineCameraTarget.rotation = Quaternion.Euler(x, y, 0f);
        }

        public void SetLookVector(Vector2 lookVector) =>
            _lookVector = lookVector;

        public void ChangeLockCameraPosition() =>
            _lockCameraPosition = !_lockCameraPosition;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f)
                lfAngle += 360f;

            if (lfAngle > 360f)
                lfAngle -= 360f;

            return Mathf.Clamp(value: lfAngle, lfMin, lfMax);
        }
    }
}