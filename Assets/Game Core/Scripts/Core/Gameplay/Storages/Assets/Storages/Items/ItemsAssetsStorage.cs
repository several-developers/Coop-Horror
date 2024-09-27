using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Infrastructure.Configs.Global.ItemsList;
using GameCore.Gameplay.Items;
using GameCore.Infrastructure.Providers.Global;
using UnityEngine.AddressableAssets;

namespace GameCore.Gameplay.Storages.Assets
{
    public class ItemsAssetsStorage : AssetsStorage<int>, IItemsAssetsStorage
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        public ItemsAssetsStorage(IAssetsProvider assetsProvider, IConfigsProvider configsProvider)
            : base(assetsProvider)
        {
            _itemsListConfig = configsProvider.GetConfig<ItemsListConfigMeta>();
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly ItemsListConfigMeta _itemsListConfig;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override async UniTask WarmUp() =>
            await SetupAssetsReferences();

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
    }
}