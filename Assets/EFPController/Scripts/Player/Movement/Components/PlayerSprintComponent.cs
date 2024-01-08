using UnityEngine;

namespace EFPController
{
    public class PlayerSprintComponent
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public PlayerSprintComponent(Player player, PlayerSprintConfig sprintConfig)
        {
            _sprintConfig = sprintConfig;
            _playerMovement = player.controller;
            _inputManager = player.InputManager;
            
            // set sprint mode to toggle, hold, or both, based on inspector setting
            switch (sprintConfig.sprintMode)
            {
                case PlayerMovement.SprintType.Both:
                    _sprintDelay = 0.4f;
                    break;
                case PlayerMovement.SprintType.Hold:
                    _sprintDelay = 0f;
                    break;
                case PlayerMovement.SprintType.Toggle:
                    _sprintDelay = 999f; // time allowed between button down and release to activate toggle
                    break;
            }
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public PlayerSprintConfig SprintConfig => _sprintConfig;
        
        public bool SprintActive { get; set; } = true; // True when player is allowed to sprint.
        public bool Sprint { get; set; } // True when sprint button is ready.

        // FIELDS: --------------------------------------------------------------------------------

        private readonly PlayerMovement _playerMovement;
        private readonly InputManager _inputManager;
        private readonly PlayerSprintConfig _sprintConfig;
        private readonly float _sprintDelay = 0.4f;

        // Track when sprinting stopped for control of item pickup time in Player script.
        private float _sprintStopTime;
        
        private float _sprintStart = -2f;
        private float _sprintEnd;
        private bool _sprintEndState;
        private bool _sprintStartState;
        private bool _sprintStopState = true;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void FixedUpdateLogic(float t, Vector2 move, bool isSwimming)
        {
            bool allowSprinting = _sprintConfig.allowSprinting;

            if (!allowSprinting)
                return;

            float inputX = _playerMovement.InputX;
            float inputY = _playerMovement.InputY;
            bool forwardSprintOnly = _sprintConfig.forwardSprintOnly;
            
            // toggle or hold sprinting state by determining if sprint button is pressed or held
            if ((Mathf.Abs(inputY) > 0f && forwardSprintOnly) ||
                (!forwardSprintOnly && (Mathf.Abs(inputX) > 0f) || Mathf.Abs(inputY) > 0f))
            {
                if (_inputManager.GetActionKey(InputManager.Action.Sprint) ||
                    _inputManager.GetActionKeyDown(InputManager.Action.Sprint))
                {
                    if (!_sprintStartState)
                    {
                        _sprintStart = t; // track time that sprint button was pressed
                        _sprintStartState = true; // perform these actions only once
                        _sprintEndState = false;
                        // if button is tapped, toggle sprint state
                        if (_sprintEnd - _sprintStart < _sprintDelay * Time.timeScale)
                        {
                            if (!Sprint)
                            {
                                // only allow sprint to start or cancel crouch if player is not under obstacle
                                if (!Sprint)
                                {
                                    Sprint = true;
                                }
                                else
                                {
                                    Sprint = false; // pressing sprint button again while sprinting stops sprint
                                }
                            }
                            else
                            {
                                Sprint = false;
                            }
                        }
                    }
                }
                else if (!_sprintEndState)
                {
                    _sprintEnd = t; // track time that sprint button was released
                    _sprintEndState = true;
                    _sprintStartState = false;
                    
                    // if releasing sprint button after holding it down, stop sprinting
                    if (_sprintEnd - _sprintStart > _sprintDelay * Time.timeScale)
                    {
                        Sprint = false;
                    }
                }
            }
            else
            {
                if (!_inputManager.GetActionKey(InputManager.Action.Sprint))
                    Sprint = false;
            }
            
            if (_playerMovement.DashComponent.DashActive)
                Sprint = false;

            // cancel a sprint in certain situations
            if ((inputY <= 0f && Mathf.Abs(inputX) > 0f &&
                 forwardSprintOnly) // cancel sprint if player sprints into a wall and strafes left or right
                || (move.y <= 0f && forwardSprintOnly) // cancel sprint if joystick is released
                || (inputY < 0f && forwardSprintOnly) // cancel sprint if player moves backwards
                || (_playerMovement.JumpingComponent.IsJumping && _playerMovement.JumpingComponent.JumpingConfig.jumpCancelsSprint)
                || isSwimming
                || _playerMovement.ClimbingComponent.IsClimbing // cancel sprint if player runs out of breath
               )
            {
                Sprint = false;
            }

            // determine if player can run
            if (((inputY > 0f && forwardSprintOnly) || (_playerMovement.IsMoving && !forwardSprintOnly))
                && Sprint
                && !_playerMovement.CrouchingComponent.IsCrouching
                && _playerMovement.IsGrounded
               )
            {
                SprintActive = true;
                _sprintStopState = true;
            }
            else
            {
                if (_sprintStopState)
                {
                    _sprintStopTime = t;
                    _sprintStopState = false;
                }

                SprintActive = false;
            }
        }
    }
}