using System;
using GameCore.Gameplay.Other;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.EntitiesSystems.Footsteps
{
    public class MonsterFootstepsSystem : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField, Min(0f)]
        private float _minTriggerDelay = 0.1f;
        
        [Title(Constants.References)]
        [SerializeField, Required]
        private AnimationObserver _animationObserver;

        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action<string> OnFootstepPerformedEvent = delegate { };

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _animationObserver.OnFootstepEvent += HandleFootsteps;
            _animationObserver.OnFootstepComplexEvent += HandComplexFootsteps;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void HandleFootsteps()
        {
            bool hitSuccessfully =
                Physics.Raycast(origin: transform.position, direction: Vector3.down, out RaycastHit hitInfo);

            if (!hitSuccessfully)
                return;

            string colliderTag = hitInfo.collider.tag;
            OnFootstepPerformedEvent.Invoke(colliderTag);
            
            switch (colliderTag)
            {
                case "Footsteps/WOOD":
                    break;

                case "Footsteps/METAL":
                    break;

                case "Footsteps/DIRT":
                    break;

                case "Footsteps/GRASS":
                    break;

                // Concrete
                default:
                    break;
            }
        }

        private void HandComplexFootsteps(float clipWeight)
        {
            // Prevent duplicating footsteps sounds.
            if (clipWeight < 0.5f)
                return;

            HandleFootsteps();
        }
    }
}