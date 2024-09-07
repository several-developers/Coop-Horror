using Cysharp.Threading.Tasks;
using GameCore.Gameplay.Items;
using GameCore.Gameplay.Utilities;

namespace GameCore.Gameplay.Factories.Items
{
    public interface IItemsFactory
    {
        UniTask WarmUp();

        UniTask CreateItem<TItemObject>(int itemID, ItemSpawnParams<TItemObject> spawnParams)
            where TItemObject : ItemObjectBase;

        void CreateItemDynamic<TItemObject>(int itemID, ItemSpawnParams<TItemObject> spawnParams)
            where TItemObject : ItemObjectBase;
    }
}