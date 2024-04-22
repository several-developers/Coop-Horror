using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.Factories.Items
{
    public interface IItemsFactory
    {
        bool CreateItem(int itemID, Vector3 position, out NetworkObject networkObject);
    }
}