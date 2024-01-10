using GameCore.Gameplay.Entities;
using GameCore.Gameplay.Interactable;
using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.Items
{
    public interface IInteractableItem : IInteractable, INetworkObject
    {
        void PickUp(NetworkObject playerNetworkObject);
        void DropServer(bool randomPosition = false);
        void Drop(bool randomPosition = false);
        void ChangeFollowTarget(Transform followTarget);
        void ShowServer();
        void Show();
        void HideServer();
        void Hide();
        int GetItemID();
    }
}