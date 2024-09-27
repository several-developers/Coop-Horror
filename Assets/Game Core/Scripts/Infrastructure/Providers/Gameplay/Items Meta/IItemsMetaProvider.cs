using GameCore.Gameplay.Items;

namespace GameCore.Infrastructure.Providers.Global.ItemsMeta
{
    public interface IItemsMetaProvider
    {
        bool TryGetItemMeta(int itemID, out ItemMeta itemMeta);
        bool IsItemMetaExists(int itemID);
    }
}