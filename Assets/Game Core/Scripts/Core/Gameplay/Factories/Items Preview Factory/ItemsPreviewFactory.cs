using GameCore.Gameplay.Items;
using GameCore.Infrastructure.Providers.Gameplay.ItemsMeta;
using UnityEngine;

namespace GameCore.Gameplay.Factories.ItemsPreview
{
    public class ItemsPreviewFactory : IItemsPreviewFactory
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ItemsPreviewFactory(IItemsMetaProvider itemsMetaProvider) =>
            _itemsMetaProvider = itemsMetaProvider;

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly IItemsMetaProvider _itemsMetaProvider;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public bool Create(int itemID, Transform parent, out ItemPreviewObject itemPreviewObject)
        {
            bool isItemMetaExists = _itemsMetaProvider.TryGetItemMeta(itemID, out ItemMeta itemMeta);

            if (!isItemMetaExists)
            {
                itemPreviewObject = null;
                LogItemNotFound(itemID);
                return false;
            }

            ItemPreviewObject itemPreviewPrefab = itemMeta.ItemPreviewPrefab;

            if (itemPreviewPrefab == null)
            {
                itemPreviewObject = null;
                LogItemPrefabNotFound(itemID);
                return false;
            }
            
            itemPreviewObject = Object.Instantiate(itemPreviewPrefab, parent);
            
            Transform itemTransform = itemPreviewObject.transform;
            itemTransform.localPosition = itemMeta.ItemPreviewPosition;
            itemTransform.localRotation = Quaternion.Euler(itemMeta.ItemPreviewRotation);
            itemTransform.localScale = itemMeta.ItemPreviewScale;
            
            return true;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private static void LogItemNotFound(int itemID)
        {
            string log = Log.HandleLog($"Item with ID <gb>({itemID})</gb> <rb>not found</rb>!");
            Debug.Log(log);
        }
        
        private static void LogItemPrefabNotFound(int itemID)
        {
            string log = Log.HandleLog($"Item Prefab with ID <gb>({itemID})</gb> <rb>not found</rb>!");
            Debug.Log(log);
        }
    }
}