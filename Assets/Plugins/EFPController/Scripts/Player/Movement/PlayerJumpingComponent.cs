using UnityEngine;

namespace EFPController
{
    public class PlayerJumpingComponent
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public PlayerJumpingComponent(Player player, InputManager inputManager, PlayerMovement playerMovement,
            PlayerJumpingConfig jumpingConfig)
        {
            _player = player;
            _inputManager = inputManager;
            _playerMovement = playerMovement;
            JumpingConfig = jumpingConfig;

            _rigidbody = player.rigidbody;
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public PlayerJumpingConfig JumpingConfig { get; }

        public bool IsJumping
        {
            get => _isJumping;
            set
            {
                _isJumping = value;
                
                if (IsJumping)
                    _playerMovement.SendOnJumpEvent();
            }
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly Player _player;
        private readonly InputManager _inputManager;
        private readonly PlayerMovement _playerMovement;
        private readonly Rigidbody _rigidbody;

        private float jumpTimer; // track the time player began jump
        private float landStartTime = -999f; // time that player landed from jump
        
        private bool _jumpfxstate = true;
        private bool _jumpButton = true; // to control jump button behavior
        private bool _isJumping; // true when player is jumping

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void JumpingUpdateLogic(float t, bool canStandUpOrJump)
        {
            bool allowJumping = JumpingConfig.allowJumping;
            float antiBunnyHopFactor = JumpingConfig.antiBunnyHopFactor;
            float jumpSpeed = JumpingConfig.jumpSpeed;
            
            if (_playerMovement.IsGrounded)
            {
                if (IsJumping)
                {
                    // play landing sound effect after landing from jump and reset jumpfxstate
                    if (jumpTimer + 0.1f < t)
                    {
                        _player.cameraAnimator.SetTrigger(CameraAnimNames.Land);
                        _playerMovement.SendOnLandEvent();

                        _jumpfxstate = true;
                        _jumpButton = false;
                        IsJumping =
                            false; // reset jumping var (this check must be before jumping var is set to true below)
                    }
                }
                else
                {
                    if (allowJumping)
                    {
                        // determine if player is jumping and set jumping variable
                        if (_inputManager.GetActionKeyDown(InputManager.Action.Jump)
                            && _jumpButton // check that jump button is not being held
                            && !_playerMovement.IsBelowWater
                            && _playerMovement.CanWaterJump
                            && canStandUpOrJump
                            && landStartTime + antiBunnyHopFactor < t // check for bunnyhop delay before jumping
                            && (_playerMovement.slopeAngle < _playerMovement.slopeLimit || _playerMovement.InWater)
                           )
                        {
                            // do not jump if ground normal is greater than slopeLimit and not in water
                            if (_playerMovement.IsCrouching)
                            {
                                _playerMovement.IsCrouching = false;
                                _playerMovement.lastCrouchTime = t;
                            }

                            jumpTimer = t;

                            _player.cameraAnimator.SetTrigger(CameraAnimNames.Jump);

                            if (_jumpfxstate)
                            {
                                // Play Audio jump sfx
                                _jumpfxstate = false;
                            }
                            
                            _playerMovement.yVelocity = jumpSpeed;
                            
                            // apply the jump velocity to the player rigidbody
                            _rigidbody.AddForce(Vector3.up * Mathf.Sqrt(2f * jumpSpeed * -Physics.gravity.y),
                                ForceMode.VelocityChange);
                            IsJumping = true;
                        }

                        // reset jumpBtn to prevent continuous jumping while holding jump button.
                        if (!_inputManager.GetActionKey(InputManager.Action.Jump) &&
                            landStartTime + antiBunnyHopFactor < t)
                        {
                            _jumpButton = true;
                        }
                    }
                }
            }
            else
            {
                if (allowJumping && _playerMovement.IsSwimming && _inputManager.GetActionKeyDown(InputManager.Action.Jump) &&
                    !_playerMovement.IsBelowWater && _playerMovement.CanWaterJump)
                {
                    jumpTimer = t;
                    _player.cameraAnimator.SetTrigger(CameraAnimNames.Jump);
                    if (_jumpfxstate)
                    {
                        // Play Audio jump sfx
                        _jumpfxstate = false;
                    }

                    _playerMovement.yVelocity = jumpSpeed;

                    // apply the jump velocity to the player rigidbody
                    _rigidbody.AddForce(Vector3.up * Mathf.Sqrt(2f * jumpSpeed * -Physics.gravity.y),
                        ForceMode.VelocityChange);
                    
                    IsJumping = true;
                }
            }
        }
    }
}