using GameCore.Gameplay.Items;
using UnityEngine;

namespace GameCore.Gameplay.Factories.Items
{
    public interface IItemsFactory
    {
        bool CreateItem(int itemID, Vector3 worldPosition, out ItemObjectBase itemObject);
    }
}