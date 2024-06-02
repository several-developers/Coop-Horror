namespace GameCore.Gameplay.EntitiesSystems.Inventory
{
    public struct EquippedItemStaticData
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public EquippedItemStaticData(InventoryItemData inventoryItemData, ulong clientID, int slotIndex)
        {
            InventoryItemData = inventoryItemData;
            ClientID = clientID;
            SlotIndex = slotIndex;
        }

        // PROPERTIES: ----------------------------------------------------------------------------
        
        public InventoryItemData InventoryItemData { get; }
        public ulong ClientID { get; }
        public int SlotIndex { get; }
    }
}