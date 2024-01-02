using System;
using GameCore.Enums;

namespace GameCore.Gameplay.Interactable
{
    public interface IInteractable
    {
        event Action OnInteractionStateChangedEvent;
        void Interact();
        void ToggleInteract(bool canInteract);
        InteractionType GetInteractionType();
        bool CanInteract();
    }
}