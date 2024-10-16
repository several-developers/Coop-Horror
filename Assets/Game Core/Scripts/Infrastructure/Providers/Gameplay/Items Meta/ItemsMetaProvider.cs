﻿using System.Collections.Generic;
using GameCore.Infrastructure.Configs.Global.ItemsList;
using GameCore.Gameplay.Items;
using UnityEngine;

namespace GameCore.Infrastructure.Providers.Global.ItemsMeta
{
    public class ItemsMetaProvider : IItemsMetaProvider
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ItemsMetaProvider(IConfigsProvider gameplayConfigsProvider)
        {
            _itemsDictionary = new Dictionary<int, ItemMeta>();

            SetupItemsDictionary(gameplayConfigsProvider);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly Dictionary<int, ItemMeta> _itemsDictionary;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public bool TryGetItemMeta(int itemID, out ItemMeta itemMeta) =>
            _itemsDictionary.TryGetValue(itemID, out itemMeta);

        public bool IsItemMetaExists(int itemID) =>
            _itemsDictionary.ContainsKey(itemID);

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SetupItemsDictionary(IConfigsProvider configsProvider)
        {
            var itemsListConfig = configsProvider.GetConfig<ItemsListConfigMeta>();
            IEnumerable<ItemsListConfigMeta.ItemReference> allItemsReferences = itemsListConfig.GetAllItemsReferences();

            foreach (ItemsListConfigMeta.ItemReference itemReference in allItemsReferences)
            {
                ItemMeta itemMeta = itemReference.ItemMeta;
                int itemID = itemMeta.ItemID;

                if (IsItemExists(itemID))
                    continue;

                _itemsDictionary.Add(itemID, itemMeta);
            }

            // LOCAL METHODS: -----------------------------

            bool IsItemExists(int itemID)
            {
                if (!IsItemMetaExists(itemID))
                    return false;

                string errorLog = Log.HandleLog($"Item with ID <gb>({itemID})</gb> <rb>already exists</rb>!");
                Debug.LogError(errorLog);

                return true;
            }
        }
    }
}