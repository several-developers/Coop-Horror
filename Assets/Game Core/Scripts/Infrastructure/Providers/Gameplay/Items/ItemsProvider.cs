using System.Collections.Generic;
using GameCore.Gameplay.Items;

namespace GameCore.Infrastructure.Providers.Gameplay.Items
{
    public class ItemsProvider : IItemsProvider
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ItemsProvider() =>
            _allItems = new Dictionary<int, ItemObjectBase>();

        // FIELDS: --------------------------------------------------------------------------------

        private readonly Dictionary<int, ItemObjectBase> _allItems;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void RegisterItem(ItemObjectBase item)
        {
            int uniqueItemID = item.UniqueItemID;
            bool isSuccessfullyAdded = _allItems.TryAdd(uniqueItemID, item);
            
            if (isSuccessfullyAdded)
                return;

            Log.PrintError(log: $"Item with Unique ID <gb>({uniqueItemID})</gb> <rb>already exists</rb>!");
        }

        public void RemoveItem(int uniqueItemID)
        {
            bool containsItem = _allItems.ContainsKey(uniqueItemID);
            
            if (!containsItem)
                return;

            _allItems.Remove(uniqueItemID);
        }

        public bool TryGetItem(int uniqueItemID, out ItemObjectBase item) =>
            _allItems.TryGetValue(uniqueItemID, out item);
    }
}