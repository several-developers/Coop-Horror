using System.Collections.Generic;
using System.Linq;
using EFPController.Utils;
using UnityEngine;

namespace EFPController
{
    public class PlayerSwimmingComponent
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public PlayerSwimmingComponent(Player player, PlayerSwimmingConfig swimmingConfig)
        {
            _player = player;
            _swimmingConfig = swimmingConfig;

            _playerMovement = player.controller;
            _inputManager = player.inputManager;
            _transform = player.transform;
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public PlayerSwimmingConfig SwimmingConfig => _swimmingConfig;

        // FIELDS: --------------------------------------------------------------------------------

        private readonly Player _player;
        private readonly PlayerMovement _playerMovement;
        private readonly InputManager _inputManager;
        private readonly Transform _transform;
        private readonly PlayerSwimmingConfig _swimmingConfig;

        // True when player is touching water collider/trigger.
        private bool _inWater;

        public bool InWater
        {
            get => _inWater;
            private set
            {
                _inWater = value;
                _playerMovement.SendOnWaterEvent(_inWater);
            }
        }

        // true when player view/camera is under the waterline
        public bool holdingBreath { get; set; }

        // true when player is below water movement threshold and is not treading water (camera/view slightly above waterline)
        private bool _isBelowWater;

        public bool IsBelowWater
        {
            get => _isBelowWater;
            private set
            {
                _isBelowWater = value;

                if (_playerMovement.swimmingAudioSource != null)
                    _playerMovement.swimmingAudioSource.clip = _isBelowWater
                        ? _playerMovement.swimUnderSurfaceSound
                        : _playerMovement.swimOnSurfaceSound;
            }
        }

        private bool _isSwimming;

        // Vlad, set was private
        public bool IsSwimming
        {
            get => _isSwimming;
            set
            {
                _isSwimming = value;

                if (_isSwimming)
                {
                    _playerMovement.Rigidbody.useGravity = false;
                    _playerMovement.sprint = _playerMovement.sprintActive = false;
                }
                else
                {
                    if (_playerMovement.swimmingAudioSource != null)
                        _playerMovement.SmoothVolume(_playerMovement.swimmingAudioSource, 0f, 0.25f,
                            () => _playerMovement.swimmingAudioSource.Pause());
                }
            }
        }

        // To make player release and press jump button again to jump if surfacing from water by holding jump button.
        public bool CanWaterJump { get; set; } = true;

        public float swimStartTime { get; set; }
        public float diveStartTime { get; set; }
        public bool drowning { get; set; } // true when player has stayed under water for longer than holdBreathDuration
        public Vector3 playerWaterInitVelocity { get; set; }

        private float drownStartTime = 0f;
        private float swimmingVerticalSpeed;

        private Collider currentWaterCollider;
        private List<Collider> allWaterColliders = new();
        private float enterWaterTime;
        private bool swimTimeState = false;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void InWaterUpdateLogic()
        {
            if (!InWater)
                return;

            // check if player is at wading depth in water (water line at chest) after wading into water
            if (_player.capsule.bounds.center.y <= currentWaterCollider.bounds.max.y)
            {
                IsSwimming = true;

                if (!swimTimeState)
                {
                    swimStartTime = Time.time; // track time that swimming started
                    swimTimeState = true;
                }

                // check if player is at treading water depth (water line at shoulders/neck) after surfacing from dive
                IsBelowWater = _playerMovement.camPos.y <= currentWaterCollider.bounds.max.y;
            }
            else
            {
                IsSwimming = false;
            }

            // check if view height is under water line
            if (_playerMovement.camPos.y <= currentWaterCollider.bounds.max.y)
            {
                if (!holdingBreath)
                {
                    diveStartTime = Time.time;
                    holdingBreath = true;

                    if (enterWaterTime + 0.3f > Time.time && _playerMovement.fallInDeepWaterSound != null)
                        _player.effectsAudioSource.PlayOneShot(_playerMovement.fallInDeepWaterSound);

                    if (_playerMovement.underwaterAudioSource != null)
                    {
                        if (!_playerMovement.underwaterAudioSource.isPlaying)
                            _playerMovement.underwaterAudioSource.Play();

                        _playerMovement.underwaterAudioSource.UnPause();
                    }

                    if (_playerMovement.divingInSounds.Length > 0 && enterWaterTime > 1f)
                        AudioManager.CreateSFX(_playerMovement.divingInSounds.Random(), _playerMovement.camPos);
                }
            }
            else
            {
                if (holdingBreath && _playerMovement.divingOutSounds.Length > 0 && diveStartTime + 0.2f < Time.time)
                    AudioManager.CreateSFX(_playerMovement.divingOutSounds.Random(), _playerMovement.camPos);

                if (_playerMovement.underwaterAudioSource != null)
                    _playerMovement.underwaterAudioSource.Pause();

                holdingBreath = false;
            }

            if (!IsSwimming && !_playerMovement.ClimbingComponent.IsClimbing && _playerMovement.IsGrounded)
                _playerMovement.Rigidbody.useGravity = true;
        }

        public void FixedUpdateLogic(float t, bool debug)
        {
            float drownDamage = _swimmingConfig.drownDamage;
            float holdBreathDuration = _swimmingConfig.holdBreathDuration;

            if (holdingBreath)
            {
                // determine if player will gasp for air when surfacing
                if (t - diveStartTime > holdBreathDuration / 1.5f)
                {
                    drowning = true;
                }

                // determine if player is drowning
                if (t - diveStartTime > holdBreathDuration)
                {
                    if (drownStartTime < t)
                    {
                        if (debug) Debug.Log("ApplyDamage: " + drownDamage);
                        drownStartTime = t + 1.75f;
                    }
                }
            }
            else
            {
                // play gasping sound if player needed air when surfacing
                if (drowning)
                {
                    // Play Audio gasp
                    drowning = false;
                }
            }
        }

        public void FixedUpdateLogic2(float t, float dt)
        {
            if (!IsSwimming)
                return;
            
            // make player sink under surface for a short time if they jumped in deep water
            if (enterWaterTime + 0.2f > t)
            {
                // dont make player sink if they are close to bottom
                // make sure that player doesn't try to sink into the ground if wading into water
                if (_playerMovement.JumpingComponent.LandStartTime + 0.3f > t)
                {
                    if (!Physics.CapsuleCast(_playerMovement.playerMiddlePos, _playerMovement.playerTopPos,
                            _playerMovement.CrouchingComponent.CrouchCapsuleCheckRadius * 0.9f,
                            -_transform.up, out _, _playerMovement.CrouchingComponent.CrouchCapsuleCastHeight, _playerMovement.groundMask.value))
                    {
                        // make player sink into water after jump
                        _playerMovement.Rigidbody.AddForce(new Vector3(0f, swimmingVerticalSpeed, 0f),
                            ForceMode.VelocityChange);
                    }
                }
            }
            else
            {
                // make player rise to water surface if they hold the jump button
                if (_inputManager.GetActionKey(InputManager.Action.Jump))
                {
                    if (IsBelowWater)
                    {
                        swimmingVerticalSpeed = Mathf.MoveTowards(swimmingVerticalSpeed, 3f, dt * 4f);
                        if (holdingBreath)
                            CanWaterJump =
                                false; // don't also jump if player just surfaced by holding jump button
                    }
                    else
                    {
                        swimmingVerticalSpeed = 0f;
                    }
                    // make player dive downwards if they hold the crouch button
                }
                else if (_inputManager.GetActionKey(InputManager.Action.Crouch))
                {
                    swimmingVerticalSpeed = Mathf.MoveTowards(swimmingVerticalSpeed, -3f, dt * 4f);
                }
                else
                {
                    // make player sink slowly when underwater due to the weight of their gear
                    if (IsBelowWater)
                    {
                        swimmingVerticalSpeed = Mathf.MoveTowards(swimmingVerticalSpeed, -0.1f, dt * 4f);
                    }
                    else
                    {
                        swimmingVerticalSpeed = 0f;
                    }
                }

                // allow jumping when treading water if player has released the jump button after surfacing 
                // by holding jump button down to prevent player from surfacing and immediately jumping
                if (!IsBelowWater && !_inputManager.GetActionKey(InputManager.Action.Jump))
                {
                    CanWaterJump = true;
                }

                if (_playerMovement.swimmingAudioSource != null)
                {
                    if (_playerMovement.IsMoving)
                    {
                        if (_playerMovement.swimmingAudioSource != null)
                        {
                            if (!_playerMovement.swimmingAudioSource.isPlaying)
                                _playerMovement.swimmingAudioSource.Play();
                                
                            _playerMovement.swimmingAudioSource.UnPause();
                            _playerMovement.SmoothVolume(_playerMovement.swimmingAudioSource, 1f, 0.25f);
                        }
                    }
                    else
                    {
                        if (_playerMovement.swimmingAudioSource != null)
                        {
                            _playerMovement.SmoothVolume(_playerMovement.swimmingAudioSource, 0f, 0.25f,
                                () => _playerMovement.swimmingAudioSource.Pause());
                        }
                    }
                }

                // apply the vertical swimming speed to the player rigidbody
                _playerMovement.Rigidbody.AddForce(new Vector3(0f, swimmingVerticalSpeed, 0f),
                    ForceMode.VelocityChange);
            }
        }

        public bool TriggerEnterLogic(Collider other)
        {
            bool inWater = other.gameObject.IsLayer(Game.Layer.Water);

            if (!inWater)
                return false;

            if (!allWaterColliders.Contains(other))
                allWaterColliders.Add(other);

            if (_playerMovement.falling && _playerMovement.fallingDistance > 1f && !InWater)
            {
                if (_playerMovement.fallInWaterSounds.Length > 0)
                    AudioManager.CreateSFX(_playerMovement.fallInWaterSounds.Random(), _transform.position);
            }

            enterWaterTime = Time.time;
            currentWaterCollider = other;
            InWater = true;
            _playerMovement.falling = false;

            return true;
        }

        public bool TriggerExitLogic(Collider other)
        {
            bool inWater = other.gameObject.IsLayer(Game.Layer.Water);

            if (!inWater)
                return false;

            if (allWaterColliders.Contains(other))
                allWaterColliders.Remove(other);

            if (other == currentWaterCollider)
            {
                if (allWaterColliders.Count > 0)
                {
                    currentWaterCollider = allWaterColliders.First();
                }
                else
                {
                    swimTimeState = false;
                    InWater = false;
                    IsSwimming = false;
                    IsBelowWater = false;
                    CanWaterJump = true;
                    holdingBreath = false;

                    if (_playerMovement.underwaterAudioSource != null)
                        _playerMovement.underwaterAudioSource.Pause();
                }
            }

            return true;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------
    }
}