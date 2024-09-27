using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities;

namespace GameCore.Gameplay.Interactable
{
    public interface IInteractable
    {
        event Action OnInteractionStateChangedEvent;
        void InteractionStarted(IEntity entity = null);
        void InteractionEnded(IEntity entity = null);
        void Interact(IEntity entity = null);
        void ToggleInteract(bool canInteract);
        InteractionType GetInteractionType();
        bool CanInteract();
    }
}