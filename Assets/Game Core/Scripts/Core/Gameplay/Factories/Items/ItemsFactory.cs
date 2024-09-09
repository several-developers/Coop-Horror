using Cysharp.Threading.Tasks;
using GameCore.Gameplay.AssetsStorages;
using GameCore.Gameplay.Items;
using GameCore.Gameplay.Network.DynamicPrefabs;
using GameCore.Gameplay.Utilities;
using GameCore.Infrastructure.Providers.Global;
using GameCore.Infrastructure.Providers.Global.ItemsMeta;
using Zenject;

namespace GameCore.Gameplay.Factories.Items
{
    public class ItemsFactory : AddressablesFactory<int>, IItemsFactory
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------
        public ItemsFactory(
            DiContainer diContainer,
            IAssetsProvider assetsProvider,
            IItemsAssetsStorage assetsStorage,
            IDynamicPrefabsLoaderDecorator dynamicPrefabsLoaderDecorator,
            IItemsMetaProvider itemsMetaProvider
        ) : base(diContainer, assetsProvider, assetsStorage, dynamicPrefabsLoaderDecorator)
        {
            _itemsMetaProvider = itemsMetaProvider;
        }


        // FIELDS: --------------------------------------------------------------------------------

        private readonly IItemsMetaProvider _itemsMetaProvider;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public async UniTask CreateItem<TItemObject>(int itemID, SpawnParams<TItemObject> spawnParams)
            where TItemObject : ItemObjectBase
        {
            if (!TrySetupItem(itemID, spawnParams))
                return;

            await LoadAndCreateNetworkObject(itemID, spawnParams);
        }

        public void CreateItemDynamic<TItemObject>(int itemID, SpawnParams<TItemObject> spawnParams)
            where TItemObject : ItemObjectBase
        {
            if (!TrySetupItem(itemID, spawnParams))
                return;

            LoadAndCreateDynamicNetworkObject(itemID, spawnParams);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private bool TrySetupItem<TItemObject>(int itemID, SpawnParams<TItemObject> spawnParams)
            where TItemObject : ItemObjectBase
        {
            if (!TryGetItemMeta(itemID, out ItemMeta itemMeta))
                return false;

            spawnParams.SetupInstanceCallbackEvent += itemObjectInstance =>
            {
                itemObjectInstance.Setup(itemID, itemMeta.ScaleMultiplier);
            };

            return true;
        }

        private bool TryGetItemMeta(int itemID, out ItemMeta itemMeta)
        {
            bool isItemMetaFound = _itemsMetaProvider.TryGetItemMeta(itemID, out itemMeta);

            if (isItemMetaFound)
                return true;

            Log.PrintError(log: $"Item Meta with Item ID <gb>({itemID})</gb> <rb>not found</rb>!");
            return false;
        }
    }
}