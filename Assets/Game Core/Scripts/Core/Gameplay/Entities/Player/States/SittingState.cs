using GameCore.Enums.Gameplay;
using GameCore.Gameplay.CamerasManagement;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Player.States
{
    public class SittingState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public SittingState(PlayerEntity playerEntity, ICamerasManager camerasManager)
        {
            _camerasManager = camerasManager;
            
            PlayerReferences references = playerEntity.References;
            _animator = references.Animator;
            _playerMovementController = references.PlayerMovementController;
            _sittingCameraController = references.SittingCameraController;
        }
        
        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly ICamerasManager _camerasManager;
        private readonly Animator _animator;
        private readonly PlayerMovementController _playerMovementController;
        private readonly SittingCameraController _sittingCameraController;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void Enter()
        {
            SetCameraFirstPersonStatus();
            TogglePlayerController(isEnabled: false);
            ToggleSittingCameraController(isEnabled: true);
            ToggleSittingAnimation(isSitting: true);
        }

        public void Exit()
        {
            TogglePlayerController(isEnabled: true);
            ToggleSittingCameraController(isEnabled: false);
            ToggleSittingAnimation(isSitting: false);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SetCameraFirstPersonStatus() =>
            _camerasManager.SetCameraStatus(CameraStatus.FirstPerson);
        
        private void TogglePlayerController(bool isEnabled) =>
            _playerMovementController.ToggleActiveState(isEnabled);

        private void ToggleSittingCameraController(bool isEnabled)
        {
            if (isEnabled)
                _playerMovementController.DisableAllCameras();
            else
                _playerMovementController.EnableUnCrouchedCamera();
            
            _sittingCameraController.ToggleActiveState(isEnabled);
        }

        private void ToggleSittingAnimation(bool isSitting) =>
            _animator.SetBool(id: AnimatorHashes.IsSitting, isSitting);
    }
}