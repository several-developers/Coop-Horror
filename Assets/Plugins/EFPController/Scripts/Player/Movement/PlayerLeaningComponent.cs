using UnityEngine;

namespace EFPController
{
    public class PlayerLeaningComponent
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public PlayerLeaningComponent(Player player, PlayerLeaningConfig leaningConfig)
        {
            _player = player;
            _leaningConfig = leaningConfig;

            _playerMovement = player.controller;
            _inputManager = player.inputManager;
            _transform = player.transform;
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public PlayerLeaningConfig LeaningConfig => _leaningConfig;
        public float LeanAmt { get; private set; }
        public float LeanPos { get; private set; }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly Player _player;
        private readonly PlayerMovement _playerMovement;
        private readonly InputManager _inputManager;
        private readonly Transform _transform;
        private readonly PlayerLeaningConfig _leaningConfig;

        private Vector3 _leanCheckPos;
        private Vector3 _leanCheckPos2;
        private float _leanFactorAmt = 1f;
        private float _leanVel;
        private bool _leanState;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void FixedUpdateLogic(Vector2 move)
        {
            float leanDistance = _leaningConfig.leanDistance;
            float standLeanAmt = _leaningConfig.standLeanAmt;
            float crouchLeanAmt = _leaningConfig.crouchLeanAmt;
            bool allowLeaning = _leaningConfig.allowLeaning;

            if (allowLeaning)
            {
                float crouchCapsuleCheckRadius = _playerMovement.CrouchingComponent.CrouchCapsuleCheckRadius;
                
                if (leanDistance > 0f)
                {
                    if (Time.timeSinceLevelLoad > 0.5f
                        && !_player.dead
                        && !_playerMovement.sprint
                        && _playerMovement.IsGrounded
                        && Mathf.Abs(move.y) < 0.2f)
                    {
                        if (_inputManager.GetActionKey(InputManager.Action.LeanLeft) &&
                            !_inputManager.GetActionKey(InputManager.Action.LeanRight)) // lean left
                        {
                            // set up horizontal capsule cast to check if player would be leaning into an object
                            if (!_playerMovement.CrouchingComponent.IsCrouching)
                            {
                                // move up bottom of lean capsule check when standing to allow player to lean over short objects like boxes 
                                _leanCheckPos = _transform.position + Vector3.up * crouchCapsuleCheckRadius;
                                _leanFactorAmt = standLeanAmt;
                            }
                            else
                            {
                                _leanCheckPos = _transform.position;
                                _leanFactorAmt = crouchLeanAmt;
                            }

                            // define upper point of capsule (1/2 height minus radius)
                            _leanCheckPos2 = _transform.position +
                                            Vector3.up * (_playerMovement.capsule.height / 2f - crouchCapsuleCheckRadius);
                            if (!Physics.CapsuleCast(_leanCheckPos, _leanCheckPos2, crouchCapsuleCheckRadius * 0.8f,
                                    -_transform.right, out _, leanDistance * _leanFactorAmt, _playerMovement.groundMask.value))
                            {
                                LeanAmt = -leanDistance * _leanFactorAmt;
                                _leanState = true;
                            }
                            else
                            {
                                LeanAmt = 0f;
                                _leanState = false;
                            }
                        }
                        else if (_inputManager.GetActionKey(InputManager.Action.LeanRight) &&
                                 !_inputManager.GetActionKey(InputManager.Action.LeanLeft))
                        {
                            // lean right
                            // set up horizontal capsule cast to check if player would be leaning into an object
                            if (!_playerMovement.CrouchingComponent.IsCrouching)
                            {
                                // move up bottom of lean capsule check when standing to allow player to lean over short objects like boxes 
                                _leanCheckPos = _transform.position + Vector3.up * crouchCapsuleCheckRadius;
                                _leanFactorAmt = standLeanAmt;
                            }
                            else
                            {
                                _leanCheckPos = _transform.position;
                                _leanFactorAmt = crouchLeanAmt;
                            }

                            // define upper point of capsule (1/2 height minus radius)
                            _leanCheckPos2 = _transform.position +
                                            Vector3.up * (_playerMovement.capsule.height / 2f - crouchCapsuleCheckRadius);
                            if (!Physics.CapsuleCast(_leanCheckPos, _leanCheckPos2, crouchCapsuleCheckRadius * 0.5f,
                                    _transform.right, out _, leanDistance * _leanFactorAmt, _playerMovement.groundMask.value))
                            {
                                LeanAmt = leanDistance * _leanFactorAmt;
                                _leanState = true;
                            }
                            else
                            {
                                LeanAmt = 0f;
                                _leanState = false;
                            }
                        }
                        else
                        {
                            LeanAmt = 0f;
                            _leanState = false;
                        }
                    }
                    else
                    {
                        LeanAmt = 0f;
                        _leanState = false;
                    }

                    // smooth position between leanAmt values
                    LeanPos = Mathf.SmoothDamp(LeanPos, LeanAmt, ref _leanVel, 0.1f, Mathf.Infinity, Time.deltaTime);
                }
            }
            else
            {
                _leanState = false;
                LeanAmt = 0f;
            }
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------
    }
}