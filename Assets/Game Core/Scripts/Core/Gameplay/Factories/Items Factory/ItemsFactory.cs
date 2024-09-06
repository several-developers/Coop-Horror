using GameCore.Gameplay.Items;
using GameCore.Gameplay.Network;
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
            _serverID = NetworkHorror.ServerID;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IItemsMetaProvider _itemsMetaProvider;
        private readonly NetworkManager _networkManager;
        private readonly ulong _serverID;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public bool CreateItem(int itemID, Vector3 worldPosition, out ItemObjectBase itemObject)
        {
            bool isItemMetaFound = TryGetItemMeta(itemID, out ItemMeta itemMeta);
            itemObject = null;

            if (!isItemMetaFound)
                return false;

            bool isNetworkObjectFound = TryGetPrefabNetworkObject(itemMeta, out NetworkObject prefabNetworkObject);

            if (!isNetworkObjectFound)
                return false;

            NetworkObject networkObject = _networkManager.SpawnManager
                .InstantiateAndSpawn(prefabNetworkObject, _serverID, destroyWithScene: true, position: worldPosition);
            
            itemObject = networkObject.GetComponent<ItemObjectBase>();
            itemObject.Setup(itemID, itemMeta.ScaleMultiplier);

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