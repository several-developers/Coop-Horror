using ECM2;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Player
{
    /// <summary>
    /// This example shows how to extend a Character (through composition) to perform a sprint ability.
    /// This one use the new simulation OnBeforeSimulationUpdate event (introduced in v1.4),
    /// to easily modify the character's state within Character's simulation loop.
    /// </summary>
    public class MySprintAbility : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField, Min(0)]
        private float _maxSprintSpeed = 10.0f;

        // PROPERTIES: ----------------------------------------------------------------------------

        public float MaxSprintSpeed => _maxSprintSpeed;
        
        // FIELDS: --------------------------------------------------------------------------------
        
        private Character _character;

        private float _cachedMaxWalkSpeed;
        private bool _isSprinting;
        private bool _sprintInputPressed;

        // GAME ENGINE METHODS: -------------------------------------------------------------------
        
        private void Awake() =>
            _character = GetComponent<Character>();

        private void OnEnable() =>
            _character.BeforeSimulationUpdated += OnBeforeSimulationUpdated;

        private void OnDisable() =>
            _character.BeforeSimulationUpdated -= OnBeforeSimulationUpdated;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        /// <summary>
        /// Request the character to start to sprint. 
        /// </summary>
        public void Sprint() =>
            _sprintInputPressed = true;

        /// <summary>
        /// Request the character to stop sprinting. 
        /// </summary>
        public void StopSprinting() =>
            _sprintInputPressed = false;

        /// <summary>
        /// Return true if the character is sprinting, false otherwise.
        /// </summary>
        public bool IsSprinting() => _isSprinting;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        /// <summary>
        /// Determines if the character, is able to sprint in its current state.
        /// </summary>
        private bool CanSprint() =>
            _character.IsWalking() && !_character.IsCrouched();

        /// <summary>
        /// Handles sprint input and adjusts character speed accordingly.
        /// </summary>
        private void CheckSprintInput()
        {
            if (!_isSprinting && _sprintInputPressed && CanSprint())
            {
                _isSprinting = true;

                _cachedMaxWalkSpeed = _character.maxWalkSpeed;
                _character.maxWalkSpeed = _maxSprintSpeed;

            }
            else if (_isSprinting && (!_sprintInputPressed || !CanSprint()))
            {
                _isSprinting = false;
                
                _character.maxWalkSpeed = _cachedMaxWalkSpeed;
            }
        }
        
        private void OnBeforeSimulationUpdated(float deltaTime)
        {
            // Handle sprinting
            
            CheckSprintInput();
        }
    }
}