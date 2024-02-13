using System;
using GameCore.Enums.Gameplay;

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