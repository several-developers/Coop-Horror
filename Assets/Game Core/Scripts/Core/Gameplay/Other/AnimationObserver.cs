using System;
using UnityEngine;

namespace GameCore.Gameplay.Other
{
    public class AnimationObserver : MonoBehaviour
    {
        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnAttackEvent = delegate { };
        public event Action OnAttackFinishedEvent = delegate { };
        public event Action OnEnabledEvent = delegate { };
        public event Action OnDisabledEvent = delegate { };
        public event Action OnDoorOpenedEvent = delegate { };
        public event Action OnDoorClosedEvent = delegate { };
        public event Action OnInteractEvent = delegate { };

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        public void OnTriggerAttackEvent() =>
            OnAttackEvent.Invoke();
        
        public void OnTriggerAttackFinishedEvent() =>
            OnAttackFinishedEvent.Invoke();
        
        public void OnTriggerEnabledEvent() =>
            OnEnabledEvent.Invoke();

        public void OnTriggerDisabledEvent() =>
            OnDisabledEvent.Invoke();
        
        public void OnTriggerDoorOpenedEvent() =>
            OnDoorOpenedEvent.Invoke();

        public void OnTriggerDoorClosedEvent() =>
            OnDoorClosedEvent.Invoke();

        public void OnTriggerInteractEvent() =>
            OnInteractEvent.Invoke();
    }
}