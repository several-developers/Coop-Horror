﻿using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Managers.Cameras;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Player.States
{
    public class SittingState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public SittingState(PlayerEntity playerEntity, ICamerasManager camerasManager)
        {
            
            _playerEntity = playerEntity;
            _references = playerEntity.GetReferences();
            _camerasManager = camerasManager;
            _animator = _references.Animator;
            _playerMovementController = _references.PlayerMovementController;
            _sittingCameraController = _references.SittingCameraController;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly PlayerEntity _playerEntity;
        private readonly PlayerReferences _references;
        private readonly ICamerasManager _camerasManager;
        private readonly Animator _animator;
        private readonly PlayerMovementController _playerMovementController;
        private readonly SittingCameraController _sittingCameraController;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            Rigidbody rigidbody = _references.Rigidbody;
            rigidbody.velocity = Vector3.zero;

            SetCameraFirstPersonStatus();
            // TogglePlayerController(isEnabled: false);
            _playerMovementController.ToggleCameraState(isEnabled: false);
            ToggleSittingCameraController(isEnabled: true);
            ToggleSittingAnimation(isSitting: true);

            _playerEntity.InputReader.OnMoveEvent += OnMove;
            _playerEntity.InputReader.OnCrouchEvent += OnCrouch;
        }

        public void Exit()
        {
            // TogglePlayerController(isEnabled: true);
            _playerMovementController.ToggleCameraState(isEnabled: true);
            ToggleSittingCameraController(isEnabled: false);
            ToggleSittingAnimation(isSitting: false);

            _playerEntity.SendLeftMobileHQSeat();

            _playerEntity.InputReader.OnMoveEvent -= OnMove;
            _playerEntity.InputReader.OnCrouchEvent -= OnCrouch;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SetCameraFirstPersonStatus() =>
            _camerasManager.SetCameraStatus(CameraStatus.FirstPerson);

        private void TogglePlayerController(bool isEnabled) =>
            _playerMovementController.ToggleActiveState(isEnabled);

        private void ToggleSittingCameraController(bool isEnabled)
        {
            if (isEnabled)
            {
                _playerMovementController.DisableAllCameras();
            }
            else
            {
#warning НАВЕСТИ КРАСОТУ СУЧЕЧКА
                float verticalAxis = _sittingCameraController.GetVerticalAxis();
                float horizontalAxis = _sittingCameraController.GetHorizontalAxis();

                Transform cameraTarget = _playerMovementController.GetCameraTarget();
                Vector3 eulerAngles = cameraTarget.localRotation.eulerAngles;
                eulerAngles.x = verticalAxis;
                cameraTarget.localRotation = Quaternion.Euler(eulerAngles);

                //_playerMovementController.ResetCameraTarget();
                //_playerMovementController.AddControlYawInput(horizontalAxis);
                _playerMovementController.SetCameraTargetPitch(-eulerAngles.x);
                _playerMovementController.EnableCamera();
            }

            _sittingCameraController.ToggleActiveState(isEnabled);
        }

        private void ToggleSittingAnimation(bool isSitting) =>
            _animator.SetBool(id: AnimatorHashes.IsSitting, isSitting);

        private void TryLeftSeat() => EnterAliveState();

        private void EnterAliveState() =>
            _playerEntity.EnterAliveState();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnMove(Vector2 moveVector) => TryLeftSeat();

        private void OnCrouch() => EnterAliveState();
    }
}