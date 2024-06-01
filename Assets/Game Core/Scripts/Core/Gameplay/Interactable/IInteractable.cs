using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities;

namespace GameCore.Gameplay.Interactable
{
    public interface IInteractable
    {
        event Action OnInteractionStateChangedEvent;
        void InteractionStarted();
        void InteractionEnded();
        void Interact(IEntity entity = null);
        void ToggleInteract(bool canInteract);
        InteractionType GetInteractionType();
        bool CanInteract();
    }
}