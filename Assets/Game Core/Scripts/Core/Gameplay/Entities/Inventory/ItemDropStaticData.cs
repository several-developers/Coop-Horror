namespace GameCore.Gameplay.Entities.Inventory
{
    public struct ItemDropStaticData
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ItemDropStaticData(int slotIndex, bool randomPosition = false, bool destroy = false)
        {
            SlotIndex = slotIndex;
            RandomPosition = randomPosition;
            Destroy = destroy;
        }
        
        // PROPERTIES: ----------------------------------------------------------------------------
        
        public int SlotIndex { get; }
        public bool RandomPosition { get; }
        public bool Destroy { get; }
    }
}