﻿using System;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Other
{
    public class AnimationObserver : MonoBehaviour
    {
        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnAttackEvent;
        public event Action OnAttackFinishedEvent;

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        public void OnTriggerAttackEvent() =>
            OnAttackEvent?.Invoke();
        
        public void OnTriggerAttackFinishedEvent() =>
            OnAttackFinishedEvent?.Invoke();
    }
}