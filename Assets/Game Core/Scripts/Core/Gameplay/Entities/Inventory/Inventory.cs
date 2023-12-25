using System.Collections.Generic;

namespace GameCore.Gameplay.Entities.Inventory
{
    public class Inventory<TItem>
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public Inventory(int size)
        {
            _items = new List<TItem>();
            _size = size;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly List<TItem> _items;
        private readonly int _size;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void AddItem(TItem item)
        {
            if (IsInventoryFull())
                return;
            
            _items.Add(item);
        }

        public void RemoveItem(TItem item) =>
            _items.Remove(item);

        public void Clear() =>
            _items.Clear();

        public IReadOnlyList<TItem> GetAllItems() => _items;

        public int GetItemsAmount() =>
            _items.Count;

        public bool IsInventoryFull() =>
            _items.Count >= _size;
    }
}