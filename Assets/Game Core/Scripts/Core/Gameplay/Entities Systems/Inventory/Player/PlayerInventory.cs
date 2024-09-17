using System;
using GameCore.Gameplay.Network;
using UnityEngine;

namespace GameCore.Gameplay.Systems.Inventory
{
    public class PlayerInventory
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public PlayerInventory() =>
            _inventory = new Inventory<InventoryItemData>(Constants.PlayerInventorySize);

        // PROPERTIES: ----------------------------------------------------------------------------

        public int Size => _inventory.Size;

        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action<EquippedItemStaticData> OnItemEquippedEvent = delegate { };
        public event Action<DroppedItemStaticData> OnItemDroppedEvent = delegate { };
        public event Action<ChangedSlotStaticData> OnSelectedSlotChangedEvent = delegate { };

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

        public bool AddItem(InventoryItemData inventoryItemData, int slotIndex)
        {
            bool isItemExists = _inventory.IsItemExists(slotIndex);

            //LogPickUpItem(itemData.ItemID);

            if (isItemExists)
                return false;
                
            _inventory.AddItem(inventoryItemData, slotIndex);

            ulong clientID = GetClientID();
            EquippedItemStaticData data = new(inventoryItemData, clientID, slotIndex);
            OnItemEquippedEvent.Invoke(data);

            return true;
        }

        public bool AddItem(InventoryItemData inventoryItemData, out int slotIndex)
        {
            bool isItemInSelectedSlotExists = _inventory.HasItemInSelectedSlot();

            //LogPickUpItem(itemData.ItemID);
            
            if (isItemInSelectedSlotExists)
                slotIndex = _inventory.AddItem(inventoryItemData);
            else
                slotIndex = _inventory.AddItemInSelectedSlot(inventoryItemData);

            // Item wasn't equipped.
            if (slotIndex < 0)
                return false;

            ulong clientID = GetClientID();
            EquippedItemStaticData data = new(inventoryItemData, clientID, slotIndex);
            OnItemEquippedEvent.Invoke(data);

            return true;
        }

        public bool DropItem(bool destroy = false)
        {
            bool hasItemInSelectedSlot = _inventory.HasItemInSelectedSlot();

            if (!hasItemInSelectedSlot)
                return false;

            //LogItemDrop(itemData.ItemID);

            int slotIndex = _inventory.DropSelectedItem();
            const bool randomPosition = false;

            ulong clientID = GetClientID();
            DroppedItemStaticData data = new(clientID, slotIndex, randomPosition, destroy);
            OnItemDroppedEvent.Invoke(data);
            
            return true;
        }
        
        public bool DropItem(int slotIndex, bool destroy = false)
        {
            bool hasItemInSelectedSlot = _inventory.IsItemExists(slotIndex);

            if (!hasItemInSelectedSlot)
                return false;

            _inventory.DropItem(slotIndex);
            return true;
        }
        
        public void DropAllItems()
        {
            int iterations = _inventory.Size;
            ulong clientID = GetClientID();
            const bool randomPosition = true;

            for (int i = 0; i < iterations; i++)
            {
                _inventory.DropItem(i);

                DroppedItemStaticData data = new(clientID, i, randomPosition);
                OnItemDroppedEvent.Invoke(data);
            }
        }

        public int GetSelectedSlotIndex() =>
            _inventory.GetSelectedSlotIndex();

        public bool TryGetSelectedItemData(out InventoryItemData inventoryItemData) =>
            _inventory.TryGetSelectedItemData(out inventoryItemData);

        public bool IsInventoryFull() =>
            _inventory.IsInventoryFull();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SendSelectedSlotChangedEvent(int selectedSlotIndex)
        {
            ulong clientID = GetClientID();
            ChangedSlotStaticData data = new(clientID, selectedSlotIndex);
            OnSelectedSlotChangedEvent?.Invoke(data);
        }
        
        private static ulong GetClientID() =>
            NetworkHorror.ClientID;

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