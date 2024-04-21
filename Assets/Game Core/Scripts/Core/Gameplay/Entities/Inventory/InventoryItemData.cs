namespace GameCore.Gameplay.Entities.Inventory
{
    public class InventoryItemData
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public InventoryItemData(int itemID, int uniqueItemID)
        {
            ItemID = itemID;
            UniqueItemID = uniqueItemID;
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public int ItemID { get; }
        public int UniqueItemID { get; }
    }
}