using GameCore.Gameplay.Interactable;
using UnityEngine;

namespace GameCore.Gameplay.Items
{
    public interface IInteractableItem : IInteractable
    {
        int ItemID { get; }
        int UniqueItemID { get; }
        void PickUp();
        void Drop(Vector3 position, Quaternion rotation, bool randomPosition = false, bool destroy = false);
        void ShowServer();
        void ShowClient();
        void HideServer();
        void HideClient();
    }
}