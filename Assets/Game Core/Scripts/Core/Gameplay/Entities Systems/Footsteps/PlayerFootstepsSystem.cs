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

        private Character _character;
        private MySprintAbility _sprintAbility;
        private InputReader _inputReader;
        
        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            GetInputEvent += GetInput;
            GetStepSpeedMultiplierEvent += GetStepSpeedMultiplier;
            GetGroundedEvent += IsGrounded;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Setup(PlayerReferences references, InputReader inputReader)
        {
            _character = references.Character;
            _inputReader = inputReader;
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
    }
}