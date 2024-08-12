using GameCore.Enums.Gameplay;
using GameCore.Gameplay.CamerasManagement;
using GameCore.Gameplay.Systems.Health;
using GameCore.Gameplay.Systems.Inventory;

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
            EnableRagdoll();
            ToggleDead();
            SetCameraSpectatorStatus();
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
            movementController.ToggleActiveState(isEnabled: false);
        }

        private void EnableRagdoll() =>
            _playerEntity.ToggleRagdollServerRpc(enable: true);

        private void ToggleDead() =>
            _playerEntity.ToggleDead(isDead: true);

        private void SetCameraSpectatorStatus() =>
            _camerasManager.SetCameraStatus(CameraStatus.Spectator);
    }
}