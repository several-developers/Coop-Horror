using CustomEditors;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Player.Movement
{
    public class MovementConfig2 : EditorMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        [Tooltip("Mask of layers that player will detect collisions with.")]
        private LayerMask _groundMask;
        
        [SerializeField, Min(0)]
        private float _inputLerpSpeed = 90f; // Movement lerp speed.

        [SerializeField, Range(0f, 90f)]
        [Tooltip("Angle of ground surface that player won't be allowed to move over.")]
        private int _slopeLimit = 40; // The maximum allowed ground surface/normal angle that the player
                                      // is allowed to climb.

        [SerializeField, Min(0)]
        [Tooltip("Speed that player moves when walking.")]
        private float _walkSpeed = 5f;

        [SerializeField, Range(0, 1)]
        [Tooltip("Percentage to decrease movement speed when moving backwards.")]
        private float _backwardSpeedPercentage = 0.6f;

        [SerializeField, Range(0, 1)]
        [Tooltip("Percentage to decrease movement speed when strafing directly left or right.")]
        private float _strafeSpeedPercentage = 0.8f;

        [SerializeField, Range(0, 1)]
        [Tooltip("Percentage to decrease movement speed when strafing diagonally.")]
        private float _diagonalStrafeAmt = 0.7071f;

        [SerializeField, Min(0)]
        [Tooltip("Maximum height of step that will be climbed.")]
        private float _maxStepHeight = 0.8f;

        [SerializeField, Min(0)]
        private float _stepHeightRadius = 1.6f;

        [SerializeField, Min(0)]
        private float _gravityMultiplier = 2f;

        [SerializeField, Min(0)]
        private float _gravityMax = 30f;

        [SerializeField, Min(0)]
        private float _airSpeedMultiplier = 1f;

        [SerializeField]
        private AnimationCurve _slopeSpeed = new()
            { keys = new Keyframe[] { new(time: 0f, value: 1f), new(time: 1f, value: 0f) } };
        
        [SerializeField, Min(0)]
        [Tooltip("Time in seconds allowed between player jumps.")]
        private float _antiBunnyHopFactor = 0.1f;
        
        [SerializeField, Min(0)]
        [Tooltip("Vertical speed of player jump.")]
        private float _jumpSpeed = 5f;
        
        [SerializeField]
        [Tooltip("True if player can Jump while sprinting and not cancel sprint.")]
        private bool _jumpCancelsSprint;

        [Title(Constants.References)]
        [SerializeField]
        private PhysicMaterial _groundedPhysMaterial;

        [SerializeField]
        private PhysicMaterial _stayPhysMaterial;

        [SerializeField]
        private PhysicMaterial _flyPhysMaterial;

        // PROPERTIES: ----------------------------------------------------------------------------

        public LayerMask GroundMask => _groundMask;
        public float InputLerpSpeed => _inputLerpSpeed;
        public int SlopeLimit => _slopeLimit;
        public float WalkSpeed => _walkSpeed;
        public float BackwardSpeedPercentage => _backwardSpeedPercentage;
        public float StrafeSpeedPercentage => _strafeSpeedPercentage;
        public float DiagonalStrafeAmt => _diagonalStrafeAmt;
        public float MaxStepHeight => _maxStepHeight;
        public float StepHeightRadius => _stepHeightRadius;
        public float GravityMultiplier => _gravityMultiplier;
        public float GravityMax => _gravityMax;
        public float AirSpeedMultiplier => _airSpeedMultiplier;
        public AnimationCurve SlopeSpeed => _slopeSpeed;
        public float AntiBunnyHopFactor => _antiBunnyHopFactor;
        public float JumpSpeed => _jumpSpeed;
        public bool JumpCancelsSprint => _jumpCancelsSprint;
        
        public PhysicMaterial GroundedPhysMaterial => _groundedPhysMaterial;
        public PhysicMaterial StayPhysMaterial => _stayPhysMaterial;
        public PhysicMaterial FlyPhysMaterial => _flyPhysMaterial;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override string GetMetaCategory() =>
            EditorConstants.PlayerConfigsCategory;
    }
}