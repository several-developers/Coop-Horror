using GameCore.Gameplay.EntitiesSystems.Health;

namespace GameCore.Gameplay.Entities.Player.States
{
    public class ReviveState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ReviveState(PlayerEntity playerEntity)
        {
            _playerEntity = playerEntity;
            _playerReferences = playerEntity.References;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly PlayerEntity _playerEntity;
        private readonly PlayerReferences _playerReferences;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            ResetHealth();
            EnableMovement();
            DisableRagdoll();
            ToggleNotDead();
            EnterSittingState();
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
            movementController.ToggleActiveState(isEnabled: true);
        }

        private void DisableRagdoll() =>
            _playerEntity.ToggleRagdollServerRpc(enable: false);

        private void ToggleNotDead() =>
            _playerEntity.ToggleDead(isDead: false);

        private void EnterSittingState() =>
            _playerEntity.EnterSittingState();
    }
}