using System;
using GameCore.Gameplay.Interactable;

namespace GameCore.Observers.Gameplay.PlayerInteraction
{
    public interface IPlayerInteractionObserver
    {
        event Action<IInteractable> OnInteractionStartedEvent;
        event Action OnInteractionEndedEvent;
        void SendCanInteract(IInteractable interactable);
        void SendInteractionEnded();
    }
}