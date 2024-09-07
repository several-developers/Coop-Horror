using Cysharp.Threading.Tasks;
using GameCore.Gameplay.Items;
using GameCore.Gameplay.Utilities;
using GameCore.Utilities;

namespace GameCore.Gameplay.Factories.Items
{
    public interface IItemsFactory : IAddressablesFactory<int>
    {
        UniTask WarmUp();

        UniTask CreateItem<TItemObject>(int itemID, ItemSpawnParams<TItemObject> spawnParams)
            where TItemObject : ItemObjectBase;

        void CreateItemDynamic<TItemObject>(int itemID, ItemSpawnParams<TItemObject> spawnParams)
            where TItemObject : ItemObjectBase;
    }
}