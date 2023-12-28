using System;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Inventory
{
    public class PlayerInventory
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public PlayerInventory() =>
            _inventory = new Inventory<ItemData>(Constants.PlayerInventorySize);

        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action<int> OnSelectedSlotChangedEvent;
        public event Action<int, ItemData> OnItemEquippedEvent;
        public event Action<int> OnItemDroppedEvent;

        private readonly Inventory<ItemData> _inventory;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SwitchToNextSlot()
        {
            int selectedSlotIndex = _inventory.GetSelectedSlotIndex();
            int newSlotIndex = selectedSlotIndex + 1;
            bool resetIndex = newSlotIndex >= _inventory.Size;

            if (resetIndex)
                newSlotIndex = 0;

            _inventory.SetSelectedSlotIndex(newSlotIndex);
            SendSelectedSlotChangedEvent(newSlotIndex);
        }

        public void SwitchToPreviousSlot()
        {
            int selectedSlotIndex = _inventory.GetSelectedSlotIndex();
            int newSlotIndex = selectedSlotIndex - 1;
            bool setLastIndex = newSlotIndex < 0;

            if (setLastIndex)
                newSlotIndex = _inventory.Size - 1;

            _inventory.SetSelectedSlotIndex(newSlotIndex);
            SendSelectedSlotChangedEvent(newSlotIndex);
        }

        public bool AddItem(ItemData itemData, out int slotIndex)
        {
            bool isItemInSelectedSlotExists = _inventory.IsItemInSelectedSlotExists();

            //LogPickUpItem(itemData.ItemID);
            
            if (isItemInSelectedSlotExists)
                slotIndex = _inventory.AddItem(itemData);
            else
                slotIndex = _inventory.AddItemInSelectedSlot(itemData);

            // Item wasn't equipped.
            if (slotIndex < 0)
                return false;
            
            OnItemEquippedEvent?.Invoke(slotIndex, itemData);

            return true;
        }

        public void DropItem()
        {
            bool isItemExists = _inventory.TryGetSelectedItem(out ItemData itemData);

            if (!isItemExists)
                return;

            //LogItemDrop(itemData.ItemID);

            int slotIndex = _inventory.DropSelectedItem();
            
            OnItemDroppedEvent?.Invoke(slotIndex);
        }

        public void MoveItems()
        {
            
        }
        
        public bool IsInventoryFull() =>
            _inventory.IsInventoryFull();

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