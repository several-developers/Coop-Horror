using GameCore.Gameplay.Interactable;

namespace GameCore.Gameplay.Items
{
    public interface IInteractableItem : IInteractable
    {
        int GetItemID();
    }
}