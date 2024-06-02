using GameCore.Enums.Gameplay;
using GameCore.Gameplay.CamerasManagement;
using GameCore.Gameplay.EntitiesSystems.Health;

namespace GameCore.Gameplay.Entities.Player.States
{
    public class ReviveState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ReviveState(PlayerEntity playerEntity, ICamerasManager camerasManager)
        {
            _playerEntity = playerEntity;
            _playerReferences = playerEntity.References;
            _camerasManager = camerasManager;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly PlayerEntity _playerEntity;
        private readonly PlayerReferences _playerReferences;
        private readonly ICamerasManager _camerasManager;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            ResetHealth();
            EnableMovement();
            SetCameraFirstPersonStatus();
            SendRevivedEvent();
            EnterAliveState();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ResetHealth()
        {
            HealthSystem healthSystem = _playerReferences.HealthSystem;
            healthSystem.Reset();
        }
        
        private void EnableMovement()
        {
            PlayerMovementController movementController = _playerReferences.PlayerMovementController;
            movementController.ToggleMovementState(isEnabled: true);
        }

        private void SetCameraFirstPersonStatus() =>
            _camerasManager.SetCameraStatus(CameraStatus.FirstPerson);

        private void SendRevivedEvent() =>
            _playerEntity.SendRevivedEvent();

        private void EnterAliveState() =>
            _playerEntity.EnterAliveState();
    }
}