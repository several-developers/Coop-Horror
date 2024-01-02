using System;
using UnityEngine;

namespace GameCore.Gameplay.Other
{
    public class AnimationObserver : MonoBehaviour
    {
        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnAttackEvent;
        public event Action OnAttackFinishedEvent;
        public event Action OnEnabledEvent;
        public event Action OnDisabledEvent;
        public event Action OnDoorOpenedEvent;
        public event Action OnDoorClosedEvent;

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        public void OnTriggerAttackEvent() =>
            OnAttackEvent?.Invoke();
        
        public void OnTriggerAttackFinishedEvent() =>
            OnAttackFinishedEvent?.Invoke();
        
        public void OnTriggerEnabledEvent() =>
            OnEnabledEvent?.Invoke();

        public void OnTriggerDisabledEvent() =>
            OnDisabledEvent?.Invoke();
        
        public void OnTriggerDoorOpenedEvent() =>
            OnDoorOpenedEvent?.Invoke();

        public void OnTriggerDoorClosedEvent() =>
            OnDoorClosedEvent?.Invoke();
    }
}