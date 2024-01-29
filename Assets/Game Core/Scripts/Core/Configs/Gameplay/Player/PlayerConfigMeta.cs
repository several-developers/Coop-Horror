using GameCore.Gameplay.Entities.Player;
using CustomEditors;
using GameCore.Gameplay.Entities.Player.Movement;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Configs.Gameplay.Player
{
    public class PlayerConfigMeta : EditorMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField]
        private MovementConfig2 _movementConfig2;
        
        [SerializeField]
        private MovementConfig3 _movementConfig3;

        public MovementConfig2 MovementConfig2 => _movementConfig2;
        public MovementConfig3 MovementConfig3 => _movementConfig3;

        [Title("Movement specifics")]
        [Tooltip("Layers where the player can stand on")]
        [SerializeField]
        private LayerMask _groundMask;
        
        [SerializeField, Min(0)]
        [Tooltip("Base player speed")]
        private float _movementSpeed = 14f;
        
        [SerializeField, Range(0f, 1f)]
        [Tooltip("Minimum input value to trigger movement")]
        private float _crouchSpeedMultiplier = 0.248f;
        
        [SerializeField, Range(0.01f, 0.99f)]
        [Tooltip("Minimum input value to trigger movement")]
        private float _movementThreshold = 0.01f;
        
        [Space(height: 10)]

        [SerializeField, Min(0)]
        [Tooltip("Speed up multiplier")]
        private float _dampSpeedUp = 0.2f;
        
        [SerializeField, Min(0)]
        [Tooltip("Speed down multiplier")]
        private float _dampSpeedDown = 0.1f;
        
        
        
        [Title("Jump and gravity specifics")]
        [SerializeField, Min(0)]
        [Tooltip("Jump velocity")]
        private float _jumpVelocity = 20f;
        
        [SerializeField]
        [Tooltip("Multiplier applied to gravity when the player is falling")]
        private float _fallMultiplier = 1.7f;
        
        [SerializeField]
        [Tooltip("Multiplier applied to gravity when the player is holding jump")]
        private float _holdJumpMultiplier = 5f;
        
        [SerializeField, Range(0f, 1f)]
        [Tooltip("Player friction against floor")]
        private float _frictionAgainstFloor = 0.189f;
        
        [SerializeField, Range(0.01f, 0.99f)]
        [Tooltip("Player friction against wall")]
        private float _frictionAgainstWall = 0.082f;
        
        [Space(10)]

        [SerializeField]
        [Tooltip("Player can long jump")]
        private bool _canLongJump = true;
        
        
        
        [Title("Slope and step specifics")]
        [SerializeField, Min(0)]
        [Tooltip("Distance from the player feet used to check if the player is touching the ground")]
        public float _groundCheckerThreshold = 0.1f;
        
        [SerializeField, Min(0)]
        [Tooltip("Distance from the player feet used to check if the player is touching a slope")]
        private float _slopeCheckerThreshold = 0.51f;
        
        [Tooltip("Distance from the player center used to check if the player is touching a step")]
        public float stepCheckerThreshold = 0.6f;
        
        [Space(10)]

        [SerializeField, Range(1f, 89f)]
        [Tooltip("Max climbable slope angle")]
        private float _maxClimbableSlopeAngle = 53.6f;
        
        [SerializeField, Min(0)]
        [Tooltip("Max climbable step height")]
        private float _maxStepHeight = 0.74f;
        
        [Space(10)]

        [SerializeField]
        [Tooltip("Speed multiplier based on slope angle")]
        private AnimationCurve _speedMultiplierOnAngle = AnimationCurve
            .EaseInOut(timeStart: 0f, valueStart: 0f, timeEnd: 1f, valueEnd: 1f);
        
        [SerializeField, Range(0.01f, 1f)]
        [Tooltip("Multiplier factor on climbable slope")]
        private float _canSlideMultiplierCurve = 0.061f;
        
        [SerializeField, Range(0.01f, 1f)]
        [Tooltip("Multiplier factor on non climbable slope")]
        private float _cantSlideMultiplierCurve = 0.039f;
        
        [SerializeField, Range(0.01f, 1f)]
        [Tooltip("Multiplier factor on step")]
        private float _climbingStairsMultiplierCurve = 0.637f;
        
        [Space(10)]

        [SerializeField]
        [Tooltip("Multiplier factor for gravity")]
        private float _gravityMultiplier = 6f;
        
        [SerializeField, Min(0)]
        [Tooltip("Multiplier factor for gravity used on change of normal")]
        private float _gravityMultiplierOnSlideChange = 3f;
        
        [SerializeField, Min(0)]
        [Tooltip("Multiplier factor for gravity used on non climbable slope")]
        private float _gravityMultiplierIfUnclimbableSlope = 30f;
        
        [Space(10)]

        [SerializeField]
        private bool _lockOnSlope;
        
        
        
        [Title("Sprint and crouch specifics")]
        [SerializeField, Min(0)]
        [Tooltip("Sprint speed")]
        private float _sprintSpeed = 20f;
        
        [SerializeField, Min(0)]
        [Tooltip("Multiplier applied to the collider when player is crouching")]
        private float _crouchHeightMultiplier = 0.5f;
        
        [SerializeField]
        [Tooltip("FP camera head height")]
        private Vector3 _povNormalHeadHeight = new(0f, 0.5f, -0.1f);
        
        [SerializeField]
        [Tooltip("FP camera head height when crouching")]
        private Vector3 _povCrouchHeadHeight = new(0f, -0.1f, -0.1f);
        
        
        
        [Title(Constants.Settings)]
        [SerializeField, Min(0)]
        private float _health = 100f;

        [Title(Constants.References)]
        [SerializeField, Required]
        private PlayerEntity _playerPrefab; // TEMP

        // PROPERTIES: ----------------------------------------------------------------------------

        // Movement specifics
        public LayerMask GroundMask => _groundMask;
        public float MovementSpeed => _movementSpeed;
        public float CrouchSpeedMultiplier => _crouchSpeedMultiplier;
        public float MovementThreshold => _movementThreshold;
        public float DampSpeedUp => _dampSpeedUp;
        public float DampSpeedDown => _dampSpeedDown;

        // Jump and gravity specifics
        public float JumpVelocity => _jumpVelocity;
        public float FallMultiplier => _fallMultiplier;
        public float HoldJumpMultiplier => _holdJumpMultiplier;
        public float FrictionAgainstFloor => _frictionAgainstFloor;
        public float FrictionAgainstWall => _frictionAgainstWall;
        public bool CanLongJump => _canLongJump;
        
        // Slope and step specifics
        public float GroundCheckerThreshold => _groundCheckerThreshold;
        public float SlopeCheckerThreshold => _slopeCheckerThreshold;
        public float MaxClimbableSlopeAngle => _maxClimbableSlopeAngle;
        public float MaxStepHeight => _maxStepHeight;
        public AnimationCurve SpeedMultiplierOnAngle => _speedMultiplierOnAngle;
        public float CanSlideMultiplierCurve => _canSlideMultiplierCurve;
        public float CantSlideMultiplierCurve => _cantSlideMultiplierCurve;
        public float ClimbingStairsMultiplierCurve => _climbingStairsMultiplierCurve;
        public float GravityMultiplier => _gravityMultiplier;
        public float GravityMultiplierOnSlideChange => _gravityMultiplierOnSlideChange;
        public float GravityMultiplierIfUnclimbableSlope => _gravityMultiplierIfUnclimbableSlope;
        public bool LockOnSlope => _lockOnSlope;
        
        // Sprint and crouch specifics
        public float SprintSpeed => _sprintSpeed;
        
        public PlayerEntity PlayerPrefab => _playerPrefab;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public override string GetMetaCategory() =>
            EditorConstants.PlayerConfigsCategory;
    }
}
