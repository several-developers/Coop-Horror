using UnityEngine;
using UnityEngine.Rendering;

namespace EFPController
{
    public class PlayerCrouchingComponent
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public PlayerCrouchingComponent(Player player, InputManager inputManager, PlayerMovement playerMovement,
            PlayerCrouchingConfig crouchingConfig)
        {
            _player = player;
            _inputManager = inputManager;
            _playerMovement = playerMovement;
            _crouchingConfig = crouchingConfig;

            _transform = player.transform;
            
            // initialize rayCast and capsule cast heights
            CrouchCapsuleCastHeight = player.capsule.height * 0.45f;
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public PlayerCrouchingConfig CrouchingConfig => _crouchingConfig;
        
        // Vlad, was a field
        public float CrouchCapsuleCheckRadius { get; set; } = 0.5f;
        
        // Amount to add to player height (proportionately increases player height, radius, and capsule cast/raycast heights).
        public float PlayerHeightMod { get; set; } // Vlad, set was private
        
        public float CrouchCapsuleCastHeight { get; }

        // Vlad, created property
        public float LastCrouchTime
        {
            get => _lastCrouchTime;
            set => _lastCrouchTime = value;
        }

        // True when player is crouching.
        public bool IsCrouching { get; set; }
        
        // FIELDS: --------------------------------------------------------------------------------

        private readonly Player _player;
        private readonly InputManager _inputManager;
        private readonly PlayerMovement _playerMovement;
        private readonly PlayerCrouchingConfig _crouchingConfig;
        private readonly Transform _transform;

        private float _lastCrouchTime; // Vlad
        private bool _crouchState;
        private bool _crouchRisen; // player has risen from crouch position.

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void CrouchUpdateLogic(float t, bool canStandUpOrJump, Vector3 currentPosition, Vector3 p2)
        {
            bool allowCrouch = _crouchingConfig.allowCrouch;
            float crouchCooldown = _crouchingConfig.crouchCooldown;
            float crouchingCamHeight = _crouchingConfig.crouchingCamHeight;
            
            // set crouched variable that other scripts will access to check for crouching
            // do this in Update() instead of FixedUpdate to prevent missed button presses between fixed updates
            if (allowCrouch)
            {
                if (IsCrouching && _inputManager.GetActionKeyDown(InputManager.Action.Jump))
                {
                    // only uncrouch if the player has room above them to stand up
                    if (canStandUpOrJump)
                    {
                        IsCrouching = false;
                        _lastCrouchTime = t;
                        _playerMovement.SendOnCrouchEvent(isCrouching: false);
                    }
                }

                if (_inputManager.GetActionKeyDown(InputManager.Action.Crouch) && !_playerMovement.IsSwimming && !_playerMovement.climbing)
                {
                    if (!_crouchState)
                    {
                        if (_lastCrouchTime + crouchCooldown < t)
                        {
                            if (!IsCrouching && _crouchRisen)
                            {
                                // faster moving down to crouch (not moving up against gravity)
                                _playerMovement.camDampSpeed = 0.1f;
                                
                                IsCrouching = true;
                                _lastCrouchTime = t;
                                _playerMovement.sprint = false; // cancel sprint if crouch button is pressed
                                _playerMovement.SendOnCrouchEvent(isCrouching: true);
                            }
                            else
                            {
                                // only uncrouch if the player has room above them to stand up
                                if (canStandUpOrJump)
                                {
                                    _playerMovement.camDampSpeed = 0.2f;
                                    IsCrouching = false;
                                    _lastCrouchTime = t;
                                    _playerMovement.SendOnCrouchEvent(isCrouching: false);
                                }
                            }

                            _crouchState = true;
                        }
                    }
                }
                else
                {
                    _crouchState = false;
                    
                    if ((_playerMovement.sprint || _playerMovement.climbing || _playerMovement.IsSwimming || _playerMovement.dashActive)
                        && !Physics.CheckCapsule(_transform.position + _transform.up * 0.75f, p2,
                            CrouchCapsuleCheckRadius * 0.9f, _playerMovement.groundMask.value, QueryTriggerInteraction.Ignore)
                       )
                    {
                        _playerMovement.camDampSpeed = 0.2f;
                        
                        // cancel crouch if sprint button is pressed and there is room above the player to stand up
                        IsCrouching = false;
                    }
                }
            }

            // determine if player has risen from crouch state
            if (_playerMovement.mainCameraTransform.position.y - currentPosition.y > crouchingCamHeight * 0.5f)
            {
                if (!_crouchRisen)
                {
                    _playerMovement.camDampSpeed = 0.1f;
                    _crouchRisen = true;
                }
            }
        }
    }
}