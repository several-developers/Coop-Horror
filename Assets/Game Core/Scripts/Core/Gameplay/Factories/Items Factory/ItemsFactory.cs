using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Configs.Global.ItemsList;
using GameCore.Gameplay.Items;
using GameCore.Gameplay.Network.DynamicPrefabs;
using GameCore.Gameplay.Network.PrefabsRegistrar;
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
            INetworkPrefabsRegistrar networkPrefabsRegistrar,
            IItemsMetaProvider itemsMetaProvider,
            IConfigsProvider configsProvider
        ) : base(diContainer, assetsProvider, dynamicPrefabsLoaderDecorator)
        {
            _networkPrefabsRegistrar = networkPrefabsRegistrar;
            _itemsMetaProvider = itemsMetaProvider;
            _itemsListConfig = configsProvider.GetConfig<ItemsListConfigMeta>();
        }


        // FIELDS: --------------------------------------------------------------------------------

        private readonly INetworkPrefabsRegistrar _networkPrefabsRegistrar;
        private readonly IItemsMetaProvider _itemsMetaProvider;
        private readonly ItemsListConfigMeta _itemsListConfig;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override async UniTask WarmUp() =>
            await SetupReferencesDictionary();

        public async UniTask CreateItem<TItemObject>(int itemID, SpawnParams<TItemObject> spawnParams)
            where TItemObject : ItemObjectBase
        {
            if (!TryGetItemMeta(itemID, out ItemMeta itemMeta))
                return;
            
            spawnParams.SetupInstanceEvent += itemObjectInstance =>
            {
                itemObjectInstance.Setup(itemID, itemMeta.ScaleMultiplier);
            };
            
            await LoadAndCreateNetworkObject(itemID, spawnParams);
        }

        public void CreateItemDynamic<TItemObject>(int itemID, SpawnParams<TItemObject> spawnParams)
            where TItemObject : ItemObjectBase
        {
            if (!TryGetItemMeta(itemID, out ItemMeta itemMeta))
                return;
            
            spawnParams.SetupInstanceEvent += itemObjectInstance =>
            {
                itemObjectInstance.Setup(itemID, itemMeta.ScaleMultiplier);
            };
            
            LoadAndCreateDynamicNetworkObject(itemID, spawnParams);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async UniTask SetupReferencesDictionary()
        {
            IEnumerable<ItemsListConfigMeta.ItemReference>
                allItemsReferences = _itemsListConfig.GetAllItemsReferences();

            foreach (ItemsListConfigMeta.ItemReference itemReference in allItemsReferences)
            {
                AssetReferenceGameObject itemPrefabAsset = itemReference.ItemPrefabAsset;
                int itemID = itemReference.ItemMeta.ItemID;
                
                var itemObject = await LoadAndReleaseAsset<ItemObjectBase>(itemPrefabAsset);

                AddDynamicAsset(itemID, itemPrefabAsset);
                _networkPrefabsRegistrar.Register(itemObject.gameObject);
            }
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