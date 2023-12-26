namespace GameCore.Gameplay.Entities.Inventory
{
    public class ItemData
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ItemData(int itemID) =>
            ItemID = itemID;

        // PROPERTIES: ----------------------------------------------------------------------------

        public int ItemID { get; }
    }
}