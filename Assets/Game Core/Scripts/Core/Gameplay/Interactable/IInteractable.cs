using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Player;

namespace GameCore.Gameplay.Interactable
{
    public interface IInteractable
    {
        event Action OnInteractionStateChangedEvent;
        void Interact(PlayerEntity playerEntity = null);
        void ToggleInteract(bool canInteract);
        InteractionType GetInteractionType();
        bool CanInteract();
    }
}