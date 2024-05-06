﻿namespace GameCore.Gameplay.Network
{
    public struct CreateItemPreviewStaticData
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public CreateItemPreviewStaticData(int slotIndex, int itemID)
        {
            SlotIndex = slotIndex;
            ItemID = itemID;
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public int SlotIndex { get; }
        public int ItemID { get; }
    }
}