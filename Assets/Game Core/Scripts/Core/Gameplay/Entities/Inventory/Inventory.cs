using System;
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

        public event Action<int> OnSelectedSlotChangedEvent;
        
        private readonly List<TItem> _items;
        private readonly int _size;

        private int _selectedSlotIndex;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void AddItem(TItem item)
        {
            if (IsInventoryFull())
                return;
            
            _items.Add(item);
        }

        public void RemoveItem(TItem item) =>
            _items.Remove(item);
        
        public void SwitchToNextSlot()
        {
            int newSlotIndex = _selectedSlotIndex + 1;
            bool resetIndex = newSlotIndex >= Constants.PlayerInventorySize;

            if (resetIndex)
                newSlotIndex = 0;

            _selectedSlotIndex = newSlotIndex;
            OnSelectedSlotChanged();
        }

        public void SwitchToLastSlot()
        {
            int newSlotIndex = _selectedSlotIndex - 1;
            bool setLastIndex = newSlotIndex < 0;

            if (setLastIndex)
                newSlotIndex = Constants.PlayerInventorySize - 1;

            _selectedSlotIndex = newSlotIndex;
            OnSelectedSlotChanged();
        }

        public void Clear() =>
            _items.Clear();

        public IReadOnlyList<TItem> GetAllItems() => _items;

        public int GetItemsAmount() =>
            _items.Count;

        public bool IsInventoryFull() =>
            _items.Count >= _size;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnSelectedSlotChanged() =>
            OnSelectedSlotChangedEvent?.Invoke(_selectedSlotIndex);
    }
}