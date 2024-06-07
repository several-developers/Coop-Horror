using System.Collections.Generic;
using GameCore.Gameplay.Items;

namespace GameCore.Infrastructure.Providers.Gameplay.Items
{
    public interface IItemsProvider
    {
        void RegisterItem(ItemObjectBase item);
        void RemoveItem(int uniqueItemID);
        IReadOnlyDictionary<int, ItemObjectBase> GetAllItems();
        bool TryGetItem(int uniqueItemID, out ItemObjectBase item);
    }
}