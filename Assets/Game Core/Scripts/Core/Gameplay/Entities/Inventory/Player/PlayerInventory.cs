using System;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Inventory
{
    public class PlayerInventory
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public PlayerInventory() =>
            _inventory = new Inventory<InventoryItemData>(Constants.PlayerInventorySize);

        // PROPERTIES: ----------------------------------------------------------------------------

        public int Size => _inventory.Size;

        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action<int> OnSelectedSlotChangedEvent;
        public event Action<int, InventoryItemData> OnItemEquippedEvent;
        public event Action<int, bool> OnItemDroppedEvent;

        private readonly Inventory<InventoryItemData> _inventory;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetSelectedSlotIndex(int slotIndex) =>
            _inventory.SetSelectedSlotIndex(slotIndex);

        public void SwitchToNextSlot()
        {
            int selectedSlotIndex = _inventory.GetSelectedSlotIndex();
            int newSlotIndex = selectedSlotIndex + 1;
            bool resetIndex = newSlotIndex >= _inventory.Size;

            if (resetIndex)
                newSlotIndex = 0;

            SetSelectedSlotIndex(newSlotIndex);
            SendSelectedSlotChangedEvent(newSlotIndex);
        }

        public void SwitchToPreviousSlot()
        {
            int selectedSlotIndex = _inventory.GetSelectedSlotIndex();
            int newSlotIndex = selectedSlotIndex - 1;
            bool setLastIndex = newSlotIndex < 0;

            if (setLastIndex)
                newSlotIndex = _inventory.Size - 1;

            SetSelectedSlotIndex(newSlotIndex);
            SendSelectedSlotChangedEvent(newSlotIndex);
        }

        public bool AddItem(InventoryItemData inventoryItemData, out int slotIndex)
        {
            bool isItemInSelectedSlotExists = _inventory.IsItemInSelectedSlotExists();

            //LogPickUpItem(itemData.ItemID);
            
            if (isItemInSelectedSlotExists)
                slotIndex = _inventory.AddItem(inventoryItemData);
            else
                slotIndex = _inventory.AddItemInSelectedSlot(inventoryItemData);

            // Item wasn't equipped.
            if (slotIndex < 0)
                return false;
            
            OnItemEquippedEvent?.Invoke(slotIndex, inventoryItemData);

            return true;
        }

        public void DropItem()
        {
            bool isItemExists = _inventory.TryGetSelectedItemData(out InventoryItemData itemData);

            if (!isItemExists)
                return;

            //LogItemDrop(itemData.ItemID);

            int slotIndex = _inventory.DropSelectedItem();
            const bool randomPosition = false;

            OnItemDroppedEvent?.Invoke(slotIndex, randomPosition);
        }
        
        public void DropAllItems()
        {
            int iterations = _inventory.Size;
            const bool randomPosition = true;

            for (int i = 0; i < iterations; i++)
            {
                _inventory.DropItem(i);
                OnItemDroppedEvent?.Invoke(i, randomPosition);
            }
        }

        public void MoveItems()
        {
            
        }

        public bool TryGetSelectedItemData(out InventoryItemData inventoryItemData) =>
            _inventory.TryGetSelectedItemData(out inventoryItemData);

        public int GetSelectedSlotIndex() =>
            _inventory.GetSelectedSlotIndex();
        
        public bool IsInventoryFull() =>
            _inventory.IsInventoryFull();

        public bool IsItemInSelectedSlotExists() =>
            _inventory.IsItemInSelectedSlotExists();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CreateItemInHand()
        {
            
        }
        
        private void SendSelectedSlotChangedEvent(int selectedSlotIndex) =>
            OnSelectedSlotChangedEvent?.Invoke(selectedSlotIndex);

        private static void LogPickUpItem(int itemID)
        {
            string dropLog = Log.HandleLog($"Item with ID <gb>({itemID})</gb> was dropped.");
            Debug.Log(dropLog);
        }
        
        private static void LogItemDrop(int itemID)
        {
            string dropLog = Log.HandleLog($"Item with ID <gb>({itemID})</gb> was dropped.");
            Debug.Log(dropLog);
        }
    }
}