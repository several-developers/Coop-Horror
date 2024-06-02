using GameCore.Enums.Gameplay;
using GameCore.Gameplay.CamerasManagement;
using GameCore.Gameplay.Entities.Player.CameraManagement;
using GameCore.Gameplay.EntitiesSystems.Health;
using GameCore.Gameplay.EntitiesSystems.Inventory;

namespace GameCore.Gameplay.Entities.Player.States
{
    public class DeathState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public DeathState(PlayerEntity playerEntity, ICamerasManager camerasManager)
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
            SetZeroHealth();
            DropAllItems();
            DisableMovement();
            SetCameraSpectatorStatus();
            SendDiedEvent();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SetZeroHealth()
        {
            HealthSystem healthSystem = _playerReferences.HealthSystem;
            healthSystem.Kill();
        }

        private void DropAllItems()
        {
            PlayerInventory inventory = _playerEntity.GetInventory();
            inventory.DropAllItems();
        }

        private void DisableMovement()
        {
            PlayerMovementController movementController = _playerReferences.PlayerMovementController;
            movementController.ToggleMovementState(isEnabled: false);
        }

        private void SetCameraSpectatorStatus() =>
            _camerasManager.SetCameraStatus(CameraStatus.Spectator);
        
        private void SendDiedEvent() =>
            _playerEntity.SendDiedEvent();
    }
}