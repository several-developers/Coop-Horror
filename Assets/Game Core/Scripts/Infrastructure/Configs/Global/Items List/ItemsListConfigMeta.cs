using System;
using System.Collections.Generic;
using GameCore.Gameplay.Items;
using GameCore.InfrastructureTools.Configs;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameCore.Infrastructure.Configs.Global.ItemsList
{
    public class ItemsListConfigMeta : ConfigMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField]
        [ListDrawerSettings(ListElementLabelName = "Label")]
        private List<ItemReference> _itemsReferences;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public IEnumerable<ItemReference> GetAllItemsReferences() => _itemsReferences;

        public override string GetMetaCategory() =>
            EditorConstants.GlobalConfigsListsCategory;

        public override ConfigScope GetConfigScope() =>
            ConfigScope.Global;

        // INNER CLASSES: -------------------------------------------------------------------------

        [Serializable]
        public class ItemReference
        {
            // MEMBERS: -------------------------------------------------------------------------------

            [SerializeField, Required]
            private ItemMeta _itemMeta;

            [SerializeField]
            private AssetReferenceGameObject _itemPrefabAsset;

            [SerializeField]
            private AssetReferenceGameObject _itemPreviewPrefabAsset;

            // PROPERTIES: ----------------------------------------------------------------------------

            public ItemMeta ItemMeta => _itemMeta;
            public AssetReferenceGameObject ItemPrefabAsset => _itemPrefabAsset;
            public AssetReferenceGameObject ItemPreviewPrefabAsset => _itemPreviewPrefabAsset;
            
            private string Label => $"'Item: {(_itemMeta == null ? "none" : _itemMeta.ItemName)}'";
        }
    }
}