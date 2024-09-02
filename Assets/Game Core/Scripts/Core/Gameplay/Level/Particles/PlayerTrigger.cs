using System;
using GameCore.Gameplay.Entities.Player;
using UnityEngine;

namespace GameCore.Gameplay.Level.Particles
{
    public class PlayerTrigger : MonoBehaviour
    {
        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnPlayerEnterEvent = delegate { };
        public event Action OnPlayerExitEvent = delegate { };

        // GAME ENGINE METHODS: -------------------------------------------------------------------
        
        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out PlayerEntity _))
                return;

            OnPlayerEnterEvent.Invoke();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent(out PlayerEntity _))
                return;

            OnPlayerExitEvent.Invoke();
        }
    }
}