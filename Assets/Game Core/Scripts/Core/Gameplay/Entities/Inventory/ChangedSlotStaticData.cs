namespace GameCore.Gameplay.Entities.Inventory
{
    public struct ChangedSlotStaticData
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ChangedSlotStaticData(ulong clientID, int slotIndex)
        {
            ClientID = clientID;
            SlotIndex = slotIndex;
        }

        // PROPERTIES: ----------------------------------------------------------------------------
        
        public ulong ClientID { get; }
        public int SlotIndex { get; }
    }
}