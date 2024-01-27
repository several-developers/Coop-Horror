using UnityEngine;

namespace GameCore.Gameplay.Entities.Player.Movement
{
    public class JumpComponent
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public JumpComponent(PlayerEntity playerEntity, PhysicalMovementBehaviour2 movementBehaviour)
        {
            PlayerReferences playerReferences = playerEntity.References;
            MovementConfig2 movementConfig = playerReferences.PlayerConfig.MovementConfig2;
            
            _movementComponentConfig = movementConfig.MovementComponentConfig;
            _jumpComponentConfig = movementConfig.JumpComponentConfig;
            _movementBehaviour = movementBehaviour;
            _rigidbody = playerReferences.Rigidbody;
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public float LandStartTime
        {
            get => _landStartTime;
            set => _landStartTime = value;
        }
        public bool IsJumping
        {
            get => _isJumping;
            set
            {
                _isJumping = value;

                //if (IsJumping)
                    //_playerMovement.SendOnJumpEvent();
            }
        }
        
        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly MovementComponentConfig _movementComponentConfig;
        private readonly JumpComponentConfig _jumpComponentConfig;
        private readonly PhysicalMovementBehaviour2 _movementBehaviour;
        private readonly Rigidbody _rigidbody;

        private MovementComponent _movementComponent;

        private float _landStartTime = -999f; // Time that player landed from jump.
        private float _jumpTimer; // Track the time player began jump.
        private bool _isJumping; // True when player is jumping.
        private bool _performJump;
        private bool _playJumpSFX = true;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Init() =>
            _movementComponent = _movementBehaviour.MovementComponent;

        public void Tick()
        {
            float antiBunnyHopFactor = _jumpComponentConfig.AntiBunnyHopFactor;
            float jumpSpeed = _jumpComponentConfig.JumpSpeed;
            float t = Time.time;
            bool isGrounded = _movementComponent.IsGrounded;

            if (!isGrounded)
                return;
            
            if (_isJumping)
            {
                // Play landing sound effect after landing from jump.
                if (_jumpTimer + 0.1f < t)
                {
                    //_player.CameraAnimator.SetTrigger(CameraAnimNames.Land);
                    //_playerMovement.SendOnLandEvent();

                    _playJumpSFX = true;
                        
                    // Reset jumping var (this check must be before jumping var is set to true below).
                    IsJumping = false;
                }
            }
            else
            {
                float slopeLimit = _movementComponentConfig.SlopeLimit;
                    
                bool jump = _performJump
                            && _landStartTime + antiBunnyHopFactor < t // Check for bunny hop delay before jumping.
                            && _movementComponent.SlopeAngle < slopeLimit;

                if (jump)
                {
                    _jumpTimer = t;
                        
                    //_player.CameraAnimator.SetTrigger(CameraAnimNames.Jump);

                    if (_playJumpSFX)
                    {
                        // Play audio
                        _playJumpSFX = false;
                    }
                        
                    _movementComponent.YVelocity = jumpSpeed;

                    Vector3 force = Vector3.up * Mathf.Sqrt(2f * jumpSpeed * -Physics.gravity.y);
                        
                    // Apply the jump velocity to the player rigidbody.
                    _rigidbody.AddForce(force, ForceMode.VelocityChange);
                        
                    IsJumping = true;
                }
            }

            _performJump = false;
        }

        public void PerformJump() =>
            _performJump = true;

        // PRIVATE METHODS: -----------------------------------------------------------------------
    }
}