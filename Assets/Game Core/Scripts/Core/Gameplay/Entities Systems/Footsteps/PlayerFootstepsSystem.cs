using ECM2;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.InputManagement;
using UnityEngine;

namespace GameCore.Gameplay.EntitiesSystems.Footsteps
{
    public class PlayerFootstepsSystem : FootstepsSystemBase
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField, Min(0f)]
        private float _crouchSpeedMultiplier = 1.5f;
        
        [SerializeField, Min(0f)]
        private float _sprintingSpeedMultiplier = 0.5f;

        // FIELDS: --------------------------------------------------------------------------------

        private InputReader _inputReader;
        private Character _character;
        private MySprintAbility _sprintAbility;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            GetStepSpeedMultiplierEvent += GetStepSpeedMultiplier;
            GetGroundedEvent += IsGrounded;
            GetCustomChecksEvent += GetCustomChecks;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Setup(PlayerEntity playerEntity)
        {
            _inputReader = playerEntity.InputReader;

            PlayerReferences references = playerEntity.References;
            _character = references.Character;
            _sprintAbility = references.SprintAbility;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private Vector2 GetInput() =>
            _inputReader.GameInput.Gameplay.Move.ReadValue<Vector2>();

        private float GetStepSpeedMultiplier()
        {
            if (_character.IsCrouched())
                return _crouchSpeedMultiplier;

            if (_sprintAbility.IsSprinting())
                return _sprintingSpeedMultiplier;

            return 1f;
        }

        private bool IsGrounded() =>
            _character.IsGrounded();

        private bool GetCustomChecks()
        {
            Vector2 input = GetInput();
            bool isInputZero = input.magnitude < 0.05f;
            return !isInputZero && IsVelocityValid();
        }

        private bool IsVelocityValid()
        {
            Vector3 velocity = _character.velocity;
            bool isVelocityValid = velocity.magnitude > 0.15f;
            return isVelocityValid;
        }
    }
}