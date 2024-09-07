using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Configs.Global.ItemsList;
using GameCore.Gameplay.Items;
using GameCore.Gameplay.Network.DynamicPrefabs;
using GameCore.Gameplay.Utilities;
using GameCore.Infrastructure.Providers.Global;
using GameCore.Infrastructure.Providers.Global.ItemsMeta;
using GameCore.Utilities;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameCore.Gameplay.Factories.Items
{
    public class ItemsFactory : AddressablesFactoryBase<int>, IItemsFactory
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------
        public ItemsFactory(
            IAssetsProvider assetsProvider,
            IConfigsProvider configsProvider,
            IItemsMetaProvider itemsMetaProvider,
            IDynamicPrefabsLoaderDecorator dynamicPrefabsLoaderDecorator
        ) : base(assetsProvider)
        {
            _dynamicPrefabsLoaderDecorator = dynamicPrefabsLoaderDecorator;
            _itemsMetaProvider = itemsMetaProvider;
            _itemsListConfig = configsProvider.GetConfig<ItemsListConfigMeta>();
            _itemsPrefabsAssets = new Dictionary<int, AssetReferenceGameObject>();
        }


        // FIELDS: --------------------------------------------------------------------------------

        private readonly IDynamicPrefabsLoaderDecorator _dynamicPrefabsLoaderDecorator;
        private readonly IItemsMetaProvider _itemsMetaProvider;
        private readonly ItemsListConfigMeta _itemsListConfig;
        private readonly Dictionary<int, AssetReferenceGameObject> _itemsPrefabsAssets;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override async UniTask WarmUp() =>
            await SetupReferencesDictionary();

        public async UniTask CreateItem<TItemObject>(int itemID, SpawnParams<TItemObject> spawnParams)
            where TItemObject : ItemObjectBase
        {
            if (!TrySetupItemParams(itemID, spawnParams))
                return;

            await LoadAndCreateItem(spawnParams, itemID);
        }

        public void CreateItemDynamic<TItemObject>(int itemID, SpawnParams<TItemObject> spawnParams)
            where TItemObject : ItemObjectBase
        {
            if (!TrySetupItemParams(itemID, spawnParams))
                return;

            InstantiateItemDynamic(spawnParams, itemID);
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
                bool success = _itemsPrefabsAssets.TryAdd(itemID, itemPrefabAsset);

                if (!success)
                {
                    Log.PrintError(log: $"Dictionary <rb>already contains</rb> item ID '<gb>{itemID}</gb>'!");
                    continue;
                }

                await LoadAndReleaseAsset<ItemObjectBase>(itemPrefabAsset);
                AddDynamicAsset(itemID, itemPrefabAsset);
            }
        }

        private bool TrySetupItemParams<TItemObject>(int itemID, SpawnParams<TItemObject> spawnParams)
            where TItemObject : ItemObjectBase
        {
            if (!TryGetItemAsset(itemID, out AssetReferenceGameObject assetReference))
            {
                spawnParams.FailCallbackEvent += _ => AssetReferenceNotFoundError(itemID);
                return false;
            }

            spawnParams.SetAssetReference(assetReference);
            return true;
        }

        private void InstantiateItemDynamic<TItemObject>(SpawnParams<TItemObject> spawnParams, int itemID)
            where TItemObject : ItemObjectBase
        {
            AssetReference assetReference = spawnParams.AssetReference;
            bool containsAssetReference = assetReference != null;
            string guid;

            if (containsAssetReference)
            {
                guid = assetReference.AssetGUID;
            }
            else if (!TryGetDynamicAssetGUID(itemID, out guid))
            {
                spawnParams.SendFailCallback(reason: $"Asset GUID for '{typeof(TItemObject)}' not found!");
                return;
            }

            _dynamicPrefabsLoaderDecorator.LoadAndGetPrefab(
                guid: guid,
                loadCallback: prefabNetworkObject => CreateItem(prefabNetworkObject, itemID, spawnParams)
            );
        }

        private async UniTask LoadAndCreateItem<TItemObject>(SpawnParams<TItemObject> spawnParams, int itemID)
            where TItemObject : ItemObjectBase
        {
            AssetReference assetReference = spawnParams.AssetReference;
            bool containsAssetReference = assetReference != null;

            TItemObject entityPrefab;

            if (containsAssetReference)
                entityPrefab = await LoadAsset<TItemObject>(assetReference);
            else
                entityPrefab = await LoadAsset<TItemObject>(itemID);

            CreateItem(entityPrefab, itemID, spawnParams);
        }

        private void CreateItem<TItemObject>(TItemObject itemPrefab, int itemID, SpawnParams<TItemObject> spawnParams)
            where TItemObject : ItemObjectBase
        {
            NetworkObject prefabNetworkObject = null;

            if (!TryGetPrefabNetworkObject())
                return;

            CreateItem(prefabNetworkObject, itemID, spawnParams);

            // LOCAL METHODS: -----------------------------

            bool TryGetPrefabNetworkObject()
            {
                bool isPrefabFound = itemPrefab != null;

                if (!isPrefabFound)
                {
                    SendFailCallback(reason: "Item prefab not found!");
                    return false;
                }

                bool isNetworkObjectFound = itemPrefab.TryGetComponent(out prefabNetworkObject);

                if (isNetworkObjectFound)
                    return true;

                SendFailCallback(reason: "Network Object not found!");
                return false;
            }

            void SendFailCallback(string reason) =>
                spawnParams.SendFailCallback(reason);
        }

        private void CreateItem<TItemObject>(NetworkObject prefabNetworkObject, int itemID,
            SpawnParams<TItemObject> spawnParams) where TItemObject : ItemObjectBase
        {
            bool isItemMetaFound = TryGetItemMeta(itemID, out ItemMeta itemMeta);

            if (!isItemMetaFound)
                return;

            if (prefabNetworkObject == null)
            {
                SendFailCallback(reason: "Network Object not found!");
                return;
            }

            NetworkObject instanceNetworkObject = InstantiateEntity();
            var itemInstance = instanceNetworkObject.GetComponent<TItemObject>();

            itemInstance.Setup(itemID, itemMeta.ScaleMultiplier);
            spawnParams.SendSuccessCallback(itemInstance);

            // LOCAL METHODS: -----------------------------

            void SendFailCallback(string reason) =>
                spawnParams.SendFailCallback(reason);

            NetworkObject InstantiateEntity()
            {
                Vector3 worldPosition = spawnParams.WorldPosition;
                Quaternion rotation = spawnParams.Rotation;
                ulong ownerID = spawnParams.OwnerID;

                NetworkSpawnManager spawnManager = NetworkManager.Singleton.SpawnManager;

                NetworkObject networkObject = spawnManager.InstantiateAndSpawn(
                    networkPrefab: prefabNetworkObject,
                    ownerClientId: ownerID,
                    destroyWithScene: true,
                    position: worldPosition,
                    rotation: rotation
                );

                return networkObject;
            }
        }

        private static void AssetReferenceNotFoundError(int itemID) =>
            Debug.LogError(message: $"Asset Reference not found for item ID '{itemID}'!");

        private bool TryGetItemAsset(int itemID, out AssetReferenceGameObject assetReference)
        {
            bool isAssetFound = _itemsPrefabsAssets.TryGetValue(itemID, out assetReference);

            if (isAssetFound)
                return true;

            Log.PrintError(log: $"Item Asset Reference with Item ID <gb>({itemID})</gb> <rb>not found</rb>!");
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