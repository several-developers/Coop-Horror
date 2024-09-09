using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Configs.Global.ItemsList;
using GameCore.Gameplay.Items;
using GameCore.Infrastructure.Providers.Global;
using UnityEngine.AddressableAssets;

namespace GameCore.Gameplay.AssetsStorages
{
    public class ItemsPreviewAssetsStorage : AssetsStorage<int>, IItemsPreviewAssetsStorage
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ItemsPreviewAssetsStorage(IAssetsProvider assetsProvider, IConfigsProvider configsProvider)
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
                AssetReferenceGameObject itemPreviewPrefabAsset = itemReference.ItemPreviewPrefabAsset;
                ItemMeta itemMeta = itemReference.ItemMeta;
                int itemID = itemMeta.ItemID;

                await LoadAndReleaseAsset<ItemPreviewObject>(itemPreviewPrefabAsset);
                AddAsset(itemID, itemPreviewPrefabAsset);
            }
        }
    }
}