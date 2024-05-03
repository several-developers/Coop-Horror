using CustomEditors;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Player.Movement
{
    public class MovementConfig4 : EditorMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title("Movement Specifics")]
        [SerializeField, Min(0)]
        private float _walkSpeed = 7f;
        
        [SerializeField, Min(0)]
        private float _sprintSpeed = 14f;

        [SerializeField, Range(0.01f, 0.99f)]
        [Tooltip("Minimum input value to trigger movement")]
        private float _movementThreshold = 0.01f;
        
        [SerializeField, Min(0)]
        [Tooltip("Speed up multiplier")]
        private float _dampSpeedUp = 0.2f;
        
        [SerializeField, Min(0)]
        [Tooltip("Speed down multiplier")]
        private float _dampSpeedDown = 0.1f;

        [SerializeField, Min(0)]
        private float _groundDrag = 6;
        
        [SerializeField, Min(0)]
        private float _airDrag = 2;
        
        [Title("Gravity Specifics")]
        [SerializeField, Min(0)]
        private float _gravityMultiplier = 3.7f;
        
        [Title("Other")]
        [SerializeField, Min(0)]
        private float _groundedCheckRadius = 0.15f;
        
        [SerializeField]
        private LayerMask _groundMask;

        // PROPERTIES: ----------------------------------------------------------------------------

        // Movement Specifics
        public float WalkSpeed => _walkSpeed;
        public float SprintSpeed => _sprintSpeed;
        public float MovementThreshold => _movementThreshold;
        public float DampSpeedUp => _dampSpeedUp;
        public float DampSpeedDown => _dampSpeedDown;
        public float GroundDrag => _groundDrag;
        public float AirDrag => _airDrag;
        
        // Gravity Specifics
        public float GravityMultiplier => _gravityMultiplier;
        
        // Other
        public float GroundedCheckRadius => _groundedCheckRadius;
        public LayerMask GroundMask => _groundMask;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override string GetMetaCategory() =>
            EditorConstants.PlayerConfigsCategory;
    }
}