namespace GameCore.Gameplay.Entities.Inventory
{
    public class Inventory<TItem> where TItem : class
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public Inventory(int size)
        {
            _items = new TItem[size];
            Size = size;
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public int Size { get; }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly TItem[] _items;

        private int _selectedSlotIndex;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public int AddItem(TItem item)
        {
            int slotIndex = -1;

            if (IsInventoryFull())
                return slotIndex;

            for (int i = 0; i < Size; i++)
            {
                bool isEmpty = _items[i] == null;

                if (!isEmpty)
                    continue;

                _items[i] = item;
                slotIndex = i;

                break;
            }

            return slotIndex;
        }

        public int AddItemInSelectedSlot(TItem item)
        {
            _items[_selectedSlotIndex] = item;
            return _selectedSlotIndex;
        }

        public void RemoveItem(TItem item)
        {
            for (int i = 0; i < Size; i++)
            {
                bool isMatches = _items[i] == item;

                if (isMatches)
                    _items[i] = null;
            }
        }

        public void RemoveItem(int slotIndex)
        {
            if (!IsIndexValid(slotIndex))
                return;

            _items[slotIndex] = null;
        }

        public void SetSelectedSlotIndex(int index) =>
            _selectedSlotIndex = index;

        public int DropSelectedItem()
        {
            RemoveItem(_selectedSlotIndex);
            return _selectedSlotIndex;
        }

        public void Clear()
        {
            for (int i = 0; i < Size; i++)
                _items[i] = null;
        }

        public int GetSelectedSlotIndex() => _selectedSlotIndex;

        public int GetItemsAmount()
        {
            int amount = 0;

            foreach (TItem item in _items)
            {
                bool isItemExists = item != null;

                if (isItemExists)
                    amount++;
            }

            return amount;
        }

        public bool TryGetSelectedItem(out TItem item)
        {
            item = _items[_selectedSlotIndex];
            bool isItemExists = IsItemExists(_selectedSlotIndex);
            return isItemExists;
        }

        public bool IsInventoryFull()
        {
            int itemsAmount = GetItemsAmount();
            return itemsAmount >= Size;
        }

        public bool IsItemInSelectedSlotExists() =>
            _items[_selectedSlotIndex] != null;

        public bool IsItemExists(int slotIndex)
        {
            if (!IsIndexValid(slotIndex))
                return false;

            return _items[slotIndex] != null;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private bool IsIndexValid(int slotIndex) =>
            slotIndex >= 0 && slotIndex < Size;
    }
}