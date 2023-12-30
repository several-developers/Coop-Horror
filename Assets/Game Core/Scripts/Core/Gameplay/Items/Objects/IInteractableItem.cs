using GameCore.Gameplay.Entities;
using GameCore.Gameplay.Interactable;
using Unity.Netcode;

namespace GameCore.Gameplay.Items
{
    public interface IInteractableItem : IInteractable, INetworkObject
    {
        void PickUp(NetworkObject playerNetworkObject);
        void Drop(bool randomPosition = false);
        int GetItemID();
    }
}