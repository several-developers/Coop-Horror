using GameCore.Gameplay.Entities;
using GameCore.Gameplay.Interactable;
using Unity.Netcode;

namespace GameCore.Gameplay.Items
{
    public interface IInteractableItem : IInteractable, INetworkObject
    {
        void ChangeOwnership();
        void PickUpServer(NetworkObject playerNetworkObject);
        void PickUpClient(ulong ownerID);
        void DropServer(bool randomPosition = false);
        void DropClient(bool randomPosition = false);
        void ShowServer();
        void ShowClient();
        void HideServer();
        void HideClient();
        int GetItemID();
    }
}