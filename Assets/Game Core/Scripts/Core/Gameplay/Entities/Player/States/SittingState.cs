﻿using GameCore.Enums.Gameplay;
using GameCore.Gameplay.CamerasManagement;
using GameCore.Gameplay.GameManagement;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Player.States
{
    public class SittingState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public SittingState(
            PlayerEntity playerEntity,
            IGameManagerDecorator gameManagerDecorator,
            ICamerasManager camerasManager
        )
        {
            _playerEntity = playerEntity;
            _gameManagerDecorator = gameManagerDecorator;
            _camerasManager = camerasManager;

            PlayerReferences references = playerEntity.References;
            _animator = references.Animator;
            _playerMovementController = references.PlayerMovementController;
            _sittingCameraController = references.SittingCameraController;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly PlayerEntity _playerEntity;
        private readonly IGameManagerDecorator _gameManagerDecorator;
        private readonly ICamerasManager _camerasManager;
        private readonly Animator _animator;
        private readonly PlayerMovementController _playerMovementController;
        private readonly SittingCameraController _sittingCameraController;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            Rigidbody rigidbody = _playerEntity.References.Rigidbody;
            rigidbody.velocity = Vector3.zero;
            
            SetCameraFirstPersonStatus();
            TogglePlayerController(isEnabled: false);
            ToggleSittingCameraController(isEnabled: true);
            ToggleSittingAnimation(isSitting: true);

            _playerEntity.InputReader.OnMoveEvent += OnMove;
            _playerEntity.InputReader.OnCrouchEvent += OnCrouch;
        }

        public void Exit()
        {
            TogglePlayerController(isEnabled: true);
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
                _playerMovementController.ResetCameraTarget();
                _playerMovementController.EnableCamera();
            }

            _sittingCameraController.ToggleActiveState(isEnabled);
        }

        private void ToggleSittingAnimation(bool isSitting) =>
            _animator.SetBool(id: AnimatorHashes.IsSitting, isSitting);

        private void TryLeftSeat()
        {
            // GameState gameState = _gameManagerDecorator.GetGameState();
            // bool isGameStateValid = gameState == GameState.ReadyToLeaveTheLocation;
            //
            // if (!isGameStateValid)
            //     return;
            
            EnterAliveState();
        }
        
        private void EnterAliveState() =>
            _playerEntity.EnterAliveState();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnMove(Vector2 moveVector) => TryLeftSeat();

        private void OnCrouch() => EnterAliveState();
    }
}