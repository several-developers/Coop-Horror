using System.Collections.Generic;
using UnityEngine;

namespace GameCore.UI.Gameplay.Inventory
{
    public class InventoryFactory
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public InventoryFactory(ItemSlotView itemSlotViewPrefab, Transform container)
        {
            _itemSlotViewPrefab = itemSlotViewPrefab;
            _container = container;
            _itemsSlots = new List<ItemSlotView>();
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly ItemSlotView _itemSlotViewPrefab;
        private readonly Transform _container;
        private readonly List<ItemSlotView> _itemsSlots;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Create()
        {
            const int playerInventorySize = Constants.PlayerInventorySize;

            for (int i = 0; i < playerInventorySize; i++)
            {
                ItemSlotView itemSlotView = Object.Instantiate(_itemSlotViewPrefab, _container);
                _itemsSlots.Add(itemSlotView);
            }
        }

        public bool GetItemSlot(int slotIndex, out ItemSlotView itemSlotView)
        {
            if (slotIndex < Constants.PlayerInventorySize)
            {
                itemSlotView = _itemsSlots[slotIndex];
                return true;
            }

            itemSlotView = null;
            return false;
        }
    }
}