using System;
using Sirenix.OdinInspector;
using Sonity;
using UnityEngine;

namespace GameCore.Gameplay.EntitiesSystems.Footsteps
{
    public abstract class FootstepsSystemBase : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(MainSettings)]
        [SerializeField, Min(0f)]
        private float _baseStepSpeed = 0.5f;
        
        [SerializeField, Min(0f)]
        private float _minStepSpeed = 0.1f;
        
        [SerializeField, Min(0f)]
        private float _maxStepSpeed = 10f;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action<string> OnFootstepPerformedEvent = delegate { };
        
        protected event Func<float> GetStepSpeedMultiplierEvent = () => 1f;
        protected event Func<bool> GetGroundedEvent = () => true;
        protected event Func<bool> GetCustomChecksEvent = () => true;

        private const string MainSettings = "Main Settings";

        private SoundEvent _soundEvent;
        private float _footstepTimer;
        private bool _isActive;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Update()
        {
            if (!_isActive)
                return;

            HandleFootsteps();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void ToggleActiveState(bool isActive) =>
            _isActive = isActive;

        // PROTECTED METHODS: ---------------------------------------------------------------------

#warning DELETE AND REPLACE WITH EVENT?
        protected virtual void PlaySound()
        {
            if (_soundEvent == null)
                return;

            _soundEvent.Play(transform);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void HandleFootsteps()
        {
            bool isGrounded = GetGroundedEvent.Invoke();

            if (!isGrounded)
                return;

            bool isCustomCheckValid = GetCustomChecksEvent.Invoke();

            if (!isCustomCheckValid)
                return;

            _footstepTimer -= Time.deltaTime;

            if (_footstepTimer > 0.0f)
                return;

            float footstepTimer = _baseStepSpeed * GetStepSpeedMultiplierEvent.Invoke();
            _footstepTimer = Mathf.Clamp(value: footstepTimer, _minStepSpeed, _maxStepSpeed);

            bool hitSuccessfully =
                Physics.Raycast(origin: transform.position, direction: Vector3.down, out RaycastHit hitInfo);

            if (!hitSuccessfully)
                return;

            string colliderTag = hitInfo.collider.tag;
            OnFootstepPerformedEvent.Invoke(colliderTag);
            
            switch (colliderTag)
            {
                case "Footsteps/WOOD":
                    PlaySound();
                    break;

                case "Footsteps/METAL":
                    PlaySound();
                    break;

                case "Footsteps/DIRT":
                    PlaySound();
                    break;

                case "Footsteps/GRASS":
                    PlaySound();
                    break;

                // Concrete
                default:
                    PlaySound();
                    break;
            }
        }
    }
}