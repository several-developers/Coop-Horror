﻿using GameCore.Gameplay.Items;
using GameCore.Infrastructure.Providers.Gameplay.ItemsMeta;
using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.Factories.Items
{
    public class ItemsFactory : IItemsFactory
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ItemsFactory(IItemsMetaProvider itemsMetaProvider)
        {
            _itemsMetaProvider = itemsMetaProvider;
            _networkManager = NetworkManager.Singleton;
            _clientID = _networkManager.LocalClientId;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IItemsMetaProvider _itemsMetaProvider;
        private readonly NetworkManager _networkManager;
        private readonly ulong _clientID;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public bool CreateItem(int itemID, Vector3 position, out NetworkObject networkObject)
        {
            bool isItemMetaFound = TryGetItemMeta(itemID, out ItemMeta itemMeta);
            networkObject = null;

            if (!isItemMetaFound)
                return false;

            bool isNetworkObjectFound = TryGetPrefabNetworkObject(itemMeta, out NetworkObject prefabNetworkObject);

            if (!isNetworkObjectFound)
                return false;

            networkObject = _networkManager.SpawnManager
                .InstantiateAndSpawn(prefabNetworkObject, _clientID, destroyWithScene: true);

            networkObject.transform.position = position;

            return true;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private static bool TryGetPrefabNetworkObject(ItemMeta itemMeta, out NetworkObject networkObject)
        {
            ItemObjectBase itemPrefab = itemMeta.ItemPrefab;
            bool isNetworkObjectFound = itemPrefab.TryGetComponent(out networkObject);

            if (isNetworkObjectFound)
                return true;

            Log.PrintError(
                log: $"Network Object of item with Item ID <gb>({itemMeta.ItemID})</gb> <rb>not found</rb>!");
            return false;
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