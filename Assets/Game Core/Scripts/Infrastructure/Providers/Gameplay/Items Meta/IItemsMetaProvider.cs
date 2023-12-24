using GameCore.Gameplay.Items;

namespace GameCore.Infrastructure.Providers.Gameplay.ItemsMeta
{
    public interface IItemsMetaProvider
    {
        bool TryGetItemMeta(int itemID, out ItemMeta itemMeta);
        bool IsItemExists(int itemID);
    }
}