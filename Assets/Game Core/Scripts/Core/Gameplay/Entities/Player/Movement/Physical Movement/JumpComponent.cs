using UnityEngine;

namespace GameCore.Gameplay.Entities.Player.Movement
{
    public class JumpComponent
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public JumpComponent(PlayerEntity playerEntity, PhysicalMovementBehaviour3 movementBehaviour)
        {
            PlayerReferences playerReferences = playerEntity.References;
            
            _movementBehaviour = movementBehaviour;
            _movementConfig = playerReferences.PlayerConfig.MovementConfig3;
            _transform = playerEntity.transform;
            _rigidbody = playerReferences.Rigidbody;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly PhysicalMovementBehaviour3 _movementBehaviour;
        private readonly MovementConfig3 _movementConfig;
        private readonly Transform _transform;
        private readonly Rigidbody _rigidbody;

        private bool _performJump;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void ActivateJump() =>
            _performJump = true;

        public void ResetJump() =>
            _performJump = false;

        public void Jump()
        {
            bool isGrounded = _movementBehaviour.IsGrounded;
            
            if (!_performJump || !isGrounded)
                return;

            float jumpForce = _movementConfig.JumpForce;
            Vector3 force = _transform.up * Mathf.Sqrt(jumpForce * -2f * Physics.gravity.y);
            Vector3 velocity = _rigidbody.velocity;

            _rigidbody.velocity = new Vector3(velocity.x, 0f, velocity.z);
            _rigidbody.AddForce(force, ForceMode.VelocityChange);
        }
    }
}