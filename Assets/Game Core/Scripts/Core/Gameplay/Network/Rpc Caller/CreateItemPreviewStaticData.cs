namespace GameCore.Gameplay.Network
{
    public struct CreateItemPreviewStaticData
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public CreateItemPreviewStaticData(ulong clientID, int slotIndex, int itemID)
        {
            ClientID = clientID;
            SlotIndex = slotIndex;
            ItemID = itemID;
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public ulong ClientID { get; }
        public int SlotIndex { get; }
        public int ItemID { get; }
    }
}