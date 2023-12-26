using GameCore.Enums;

namespace GameCore.Gameplay.Interactable
{
    public interface IInteractable
    {
        void Interact();
        InteractionType GetInteractionType();
    }
}