﻿using System;
using GameCore.Gameplay.Interactable;

namespace GameCore.Observers.Gameplay.PlayerInteraction
{
    public class PlayerInteractionObserver : IPlayerInteractionObserver
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action<IInteractable> OnCanInteractEvent;
        public event Action OnInteractionEndedEvent;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void SendCanInteract(IInteractable interactable) =>
            OnCanInteractEvent?.Invoke(interactable);

        public void SendInteractionEnded() =>
            OnInteractionEndedEvent?.Invoke();
    }
}