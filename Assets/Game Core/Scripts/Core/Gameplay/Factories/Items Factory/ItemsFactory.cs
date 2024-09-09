using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Configs.Global.ItemsList;
using GameCore.Gameplay.Items;
using GameCore.Gameplay.Network.DynamicPrefabs;
using GameCore.Gameplay.Utilities;
using GameCore.Infrastructure.Providers.Global;
using GameCore.Infrastructure.Providers.Global.ItemsMeta;
using GameCore.Utilities;
using UnityEngine.AddressableAssets;
using Zenject;

namespace GameCore.Gameplay.Factories.Items
{
    public class ItemsFactory : AddressablesFactoryBase<int>, IItemsFactory
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------
        public ItemsFactory(
            DiContainer diContainer,
            IAssetsProvider assetsProvider,
            IDynamicPrefabsLoaderDecorator dynamicPrefabsLoaderDecorator,
            IItemsMetaProvider itemsMetaProvider,
            IConfigsProvider configsProvider
        ) : base(diContainer, assetsProvider, dynamicPrefabsLoaderDecorator)
        {
            _itemsMetaProvider = itemsMetaProvider;
            _itemsListConfig = configsProvider.GetConfig<ItemsListConfigMeta>();
        }


        // FIELDS: --------------------------------------------------------------------------------

        private readonly IItemsMetaProvider _itemsMetaProvider;
        private readonly ItemsListConfigMeta _itemsListConfig;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override async UniTask WarmUp() =>
            await SetupAssetsReferences();

        public async UniTask CreateItem<TItemObject>(int itemID, SpawnParams<TItemObject> spawnParams)
            where TItemObject : ItemObjectBase
        {
            if (!TryPrepareItem(itemID, spawnParams))
                return;

            await LoadAndCreateNetworkObject(itemID, spawnParams);
        }

        public void CreateItemDynamic<TItemObject>(int itemID, SpawnParams<TItemObject> spawnParams)
            where TItemObject : ItemObjectBase
        {
            if (!TryPrepareItem(itemID, spawnParams))
                return;

            LoadAndCreateDynamicNetworkObject(itemID, spawnParams);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async UniTask SetupAssetsReferences()
        {
            IEnumerable<ItemsListConfigMeta.ItemReference>
                allItemsReferences = _itemsListConfig.GetAllItemsReferences();

            foreach (ItemsListConfigMeta.ItemReference itemReference in allItemsReferences)
            {
                AssetReferenceGameObject itemPrefabAsset = itemReference.ItemPrefabAsset;
                int itemID = itemReference.ItemMeta.ItemID;

                await LoadAndReleaseAsset<ItemObjectBase>(itemPrefabAsset);
                AddDynamicAsset(itemID, itemPrefabAsset);
            }
        }

        private bool TryPrepareItem<TItemObject>(int itemID, SpawnParams<TItemObject> spawnParams)
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