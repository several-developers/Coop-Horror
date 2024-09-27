using System;
using GameCore.Gameplay.Interactable;

namespace GameCore.Observers.Gameplay.PlayerInteraction
{
    public class PlayerInteractionObserver : IPlayerInteractionObserver
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action<IInteractable> OnInteractionStartedEvent;
        public event Action OnInteractionEndedEvent;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void SendCanInteract(IInteractable interactable) =>
            OnInteractionStartedEvent?.Invoke(interactable);

        public void SendInteractionEnded() =>
            OnInteractionEndedEvent?.Invoke();
    }
}