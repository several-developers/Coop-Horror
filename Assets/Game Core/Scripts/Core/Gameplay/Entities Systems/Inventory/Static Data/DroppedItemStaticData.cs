namespace GameCore.Gameplay.Systems.Inventory
{
    public struct DroppedItemStaticData
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public DroppedItemStaticData(ulong clientID, int slotIndex, bool randomPosition = false, bool destroy = false)
        {
            ClientID = clientID;
            SlotIndex = slotIndex;
            RandomPosition = randomPosition;
            Destroy = destroy;
        }
        
        // PROPERTIES: ----------------------------------------------------------------------------
        
        public ulong ClientID { get; }
        public int SlotIndex { get; }
        public bool RandomPosition { get; }
        public bool Destroy { get; }
    }
}