using System;
using GameCore.Gameplay.Entities;
using UnityEngine;

namespace GameCore.Gameplay.Triggers
{
    public class LocationDoorTrigger : MonoBehaviour
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action OnTriggeredEvent = delegate { };
        
        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void OnTriggerEnter(Collider other) => CheckForEntity(other);
        
        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CheckForEntity(Component other)
        {
            if (!other.TryGetComponent(out IEntity _))
                return;

            OnTriggeredEvent.Invoke();
        }
    }
}