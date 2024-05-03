using CustomEditors;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Player.Movement
{
    public class MovementConfig3 : EditorMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title("Movement specifics")]
        [SerializeField, Min(0)]
        private float _walkSpeed = 6f;
        
        [SerializeField, Min(0)]
        private float _sprintSpeed = 9f;

        [SerializeField, Min(0)]
        private float _acceleration = 10f;

        [SerializeField, Min(0)]
        private float _moveSpeedMultiplier = 10f;

        [SerializeField, Range(0, 1)]
        private float _moveSpeedAirMultiplier = 0.4f;

        [SerializeField, Min(0)]
        private float _groundDrag = 6f;
        
        [SerializeField]
        private float _airDrag = 1f;

        [Title("Jump specifics")]
        [SerializeField, Min(0)]
        private float _jumpForce = 6f;

        [Title("Step specifics")]
        [SerializeField, Min(0)]
        private float _stepHeight = 0.3f;
        
        [SerializeField, Min(0)]
        private float _stepSmooth = 0.1f;

        [SerializeField, Min(0)]
        [Tooltip("Distance from the player center used to check if the player is touching a step")]
        private float _stepCheckerThreshold = 0.6f;

        [Title("Slope specifics")]
        [SerializeField, Range(0, 95)]
        private float _maxSlopeAngle = 55f;
        
        [SerializeField]
        [Tooltip("Speed multiplier based on slope angle")]
        private AnimationCurve _speedMultiplierOnAngle = AnimationCurve
            .EaseInOut(timeStart: 0f, valueStart: 0f, timeEnd: 1f, valueEnd: 1f);
        
        [Title("Gravity specifics")]
        [SerializeField, Min(0)]
        private float _gravityMultiplier = 3f;
        
        [SerializeField, Min(0)]
        [Tooltip("Multiplier factor for gravity used on non climbable slope")]
        private float _gravityMultiplierIfUnclimbableSlope = 30f;
        
        [Title("Other")]
        [SerializeField]
        private LayerMask _groundMask;
        
        [SerializeField, Range(0f, 1f)]
        [Tooltip("Player friction against floor")]
        private float _frictionAgainstFloor = 0.189f;
        
        [SerializeField, Range(0.01f, 0.99f)]
        [Tooltip("Player friction against wall")]
        private float _frictionAgainstWall = 0.082f;

        // PROPERTIES: ----------------------------------------------------------------------------

        // Movement
        public float WalkSpeed => _walkSpeed;
        public float SprintSpeed => _sprintSpeed;
        public float Acceleration => _acceleration;
        public float MoveSpeedMultiplier => _moveSpeedMultiplier;
        public float MoveSpeedAirMultiplier => _moveSpeedAirMultiplier;
        public float GroundDrag => _groundDrag;
        public float AirDrag => _airDrag;
        
        // Jump
        public float JumpForce => _jumpForce;
        
        // Step
        public float StepHeight => _stepHeight;
        public float StepSmooth => _stepSmooth;
        public float StepCheckerThreshold => _stepCheckerThreshold;
        
        // Slope
        public float MaxSlopeAngle => _maxSlopeAngle;
        public AnimationCurve SpeedMultiplierOnAngle => _speedMultiplierOnAngle;
        
        // Gravity
        public float GravityMultiplier => _gravityMultiplier;
        public float GravityMultiplierIfUnclimbableSlope => _gravityMultiplierIfUnclimbableSlope;
        
        // Other
        public LayerMask GroundMask => _groundMask;
        public float FrictionAgainstFloor => _frictionAgainstFloor;
        public float FrictionAgainstWall => _frictionAgainstWall;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override string GetMetaCategory() =>
            EditorConstants.PlayerConfigsCategory;
    }
}