using GameCore.Gameplay.Items;

namespace GameCore.Infrastructure.Providers.Gameplay.Items
{
    public interface IItemsProvider
    {
        void RegisterItem(ItemObjectBase item);
        bool TryGetItem(int uniqueItemID, out ItemObjectBase item);
    }
}