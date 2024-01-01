using GameCore.Gameplay.Entities;
using GameCore.Gameplay.Interactable;
using Unity.Netcode;

namespace GameCore.Gameplay.Items
{
    public interface IInteractableItem : IInteractable, INetworkObject
    {
        void PickUp(NetworkObject playerNetworkObject);
        void DropServer(bool randomPosition = false);
        void Drop(bool randomPosition = false);
        void ShowServer();
        void Show();
        void HideServer();
        void Hide();
        int GetItemID();
    }
}