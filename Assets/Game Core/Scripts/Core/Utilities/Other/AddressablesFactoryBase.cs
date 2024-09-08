using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Gameplay.Network.DynamicPrefabs;
using GameCore.Gameplay.Utilities;
using GameCore.Infrastructure.Providers.Global;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace GameCore.Utilities
{
    public abstract class AddressablesFactoryBase<TKey> : IAddressablesFactory<TKey>
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        protected AddressablesFactoryBase(
            IAssetsProvider assetsProvider,
            IDynamicPrefabsLoaderDecorator dynamicPrefabsLoaderDecorator
        )
        {
            _assetsProvider = assetsProvider;
            _dynamicPrefabsLoaderDecorator = dynamicPrefabsLoaderDecorator;
            _referencesDictionary = new Dictionary<TKey, AssetReference>();
            _dynamicReferencesDictionary = new Dictionary<TKey, AssetReference>();
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IAssetsProvider _assetsProvider;
        private readonly IDynamicPrefabsLoaderDecorator _dynamicPrefabsLoaderDecorator;
        private readonly Dictionary<TKey, AssetReference> _referencesDictionary;
        private readonly Dictionary<TKey, AssetReference> _dynamicReferencesDictionary;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public abstract UniTask WarmUp();

        public void AddAsset(TKey key, AssetReference assetReference)
        {
            bool success = TryAddAsset(key, assetReference, _referencesDictionary);

            if (!success)
                return;

            string log = Log.HandleLog($"Added Asset with key '<gb>{key}</gb>'");
            Debug.Log(log);
        }

        public void AddDynamicAsset(TKey key, AssetReference assetReference)
        {
            bool success = TryAddAsset(key, assetReference, _dynamicReferencesDictionary);

            if (!success)
                return;

            string log = Log.HandleLog($"Added Dynamic Asset with key '<gb>{key}</gb>'");
            Debug.Log(log);
        }

        public async UniTask<T> LoadAndReleaseAsset<T>(AssetReference assetReference) where T : class
        {
            var handle = await _assetsProvider.LoadAsset<GameObject>(assetReference);
            _assetsProvider.ReleaseAsset(assetReference);

            if (handle.TryGetComponent(out T result))
                return result;

            Log.PrintError(log: $"Component of type '<gb>{typeof(T)}</gb>' <rb>not found</rb>!");
            return null;
        }

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected async UniTask<GameObject> LoadAndCreateGameObject<TObject>(TKey key, SpawnParams<TObject> spawnParams)
            where TObject : class
        {
            AssetReference assetReference = spawnParams.AssetReference;
            bool containsAssetReference = assetReference != null;

            TObject prefab;

            if (containsAssetReference)
                prefab = await LoadAsset<TObject>(assetReference);
            else
                prefab = await LoadAsset<TObject>(key);

            if (prefab == null)
            {
                Log.PrintError(log: $"Prefab with key '<gb>{key}</gb>' <rb>not found</rb>!");
                return null;
            }

            Type prefabType = prefab.GetType();

            if (prefab is not GameObject prefabGameObject)
            {
                Log.PrintError(log: $"Prefab of type '<gb>{prefabType}</gb>' <rb>is not</rb> GameObject!");
                return null;
            }

            Vector3 worldPosition = spawnParams.WorldPosition;
            Quaternion rotation = spawnParams.Rotation;

            GameObject instance = Object.Instantiate(prefabGameObject, worldPosition, rotation);
            
            spawnParams.SendSetupInstance(prefab);
            spawnParams.SendSuccessCallback(prefab);

            return instance;
        }

        protected async UniTask<NetworkObject> LoadAndCreateNetworkObject<TObject>(TKey key,
            SpawnParams<TObject> spawnParams) where TObject : class
        {
            AssetReference assetReference = spawnParams.AssetReference;
            bool containsAssetReference = assetReference != null;

            TObject prefab;

            if (containsAssetReference)
                prefab = await LoadAsset<TObject>(assetReference);
            else
                prefab = await LoadAsset<TObject>(key);

            if (prefab == null)
            {
                Log.PrintError(log: $"Prefab with key '<gb>{key}</gb>' <rb>not found</rb>!");
                return null;
            }

            Type prefabType = prefab.GetType();

            if (prefab is not NetworkObject prefabNetworkObject)
            {
                Log.PrintError(log: $"Prefab of type '<gb>{prefabType}</gb>' <rb>is not</rb> NetworkObject!");
                return null;
            }

            Vector3 worldPosition = spawnParams.WorldPosition;
            Quaternion rotation = spawnParams.Rotation;
            ulong ownerID = spawnParams.OwnerID;

            NetworkSpawnManager spawnManager = NetworkManager.Singleton.SpawnManager;

            NetworkObject instance = spawnManager.InstantiateAndSpawn(
                networkPrefab: prefabNetworkObject,
                ownerClientId: ownerID,
                destroyWithScene: true,
                position: worldPosition,
                rotation: rotation
            );
            
            spawnParams.SendSetupInstance(prefab);
            spawnParams.SendSuccessCallback(prefab);

            return instance;
        }

        protected void LoadAndCreateDynamicGameObject<TObject>(TKey key, SpawnParams<TObject> spawnParams)
            where TObject : class
        {
            AssetReference assetReference = spawnParams.AssetReference;
            bool containsAssetReference = assetReference != null;
            string guid;

            if (containsAssetReference)
            {
                guid = assetReference.AssetGUID;
            }
            else if (!TryGetDynamicAssetGUID(key, out guid))
            {
                spawnParams.SendFailCallback(reason: $"Asset GUID for key '{key}' not found!");
                return;
            }

            _dynamicPrefabsLoaderDecorator.LoadAndGetGameObjectPrefab(
                guid: guid,
                loadCallback: prefabNetworkObject => DynamicGameObjectLoaded(prefabNetworkObject, spawnParams)
            );
        }

        protected void LoadAndCreateDynamicNetworkObject<TObject>(TKey key, SpawnParams<TObject> spawnParams)
            where TObject : class
        {
            AssetReference assetReference = spawnParams.AssetReference;
            bool containsAssetReference = assetReference != null;
            string guid;

            if (containsAssetReference)
            {
                guid = assetReference.AssetGUID;
            }
            else if (!TryGetDynamicAssetGUID(key, out guid))
            {
                spawnParams.SendFailCallback(reason: $"Asset GUID for key '{key}' not found!");
                return;
            }

            _dynamicPrefabsLoaderDecorator.LoadAndGetNetworkObjectPrefab(
                guid: guid,
                loadCallback: prefabNetworkObject => DynamicNetworkObjectLoaded(prefabNetworkObject, spawnParams)
            );
        }

        protected async UniTask<T> LoadAsset<T>(TKey key) where T : class
        {
            bool isAssetReferenceFound = TryGetAssetReference(key, out AssetReference assetReference);

            if (!isAssetReferenceFound)
                return null;

            var prefab = await _assetsProvider.LoadAsset<GameObject>(assetReference);
            bool isComponentFound = prefab.TryGetComponent(out T instance);

            if (!isComponentFound)
                Log.PrintError(log: $"Component '<gb>{typeof(T)}</gb> <rb>not found</rb>!'");

            return instance;
        }

        protected async UniTask<T> LoadAsset<T>(AssetReference assetReference) where T : class
        {
            var prefab = await _assetsProvider.LoadAsset<GameObject>(assetReference);
            bool isComponentFound = prefab.TryGetComponent(out T instance);

            if (!isComponentFound)
                Log.PrintError(log: $"Component '<gb>{typeof(T)}</gb> <rb>not found</rb>!'");

            return instance;
        }

        protected void ReleaseAsset(AssetReference assetReference) =>
            Addressables.Release(assetReference);

        protected bool TryGetAssetGUID<T>(TKey key, out string guid) where T : class
        {
            if (!TryGetAssetReference(key, out AssetReference assetReference))
            {
                guid = string.Empty;
                return false;
            }

            guid = assetReference.AssetGUID;
            return true;
        }

        protected bool TryGetDynamicAssetGUID(TKey key, out string guid)
        {
            if (!TryGetDynamicAssetReference(key, out AssetReference assetReference))
            {
                guid = string.Empty;
                return false;
            }

            guid = assetReference.AssetGUID;
            return true;
        }

        protected bool TryGetAssetReference(TKey key, out AssetReference assetReference)
        {
            bool isAssetReferenceFound = _referencesDictionary.TryGetValue(key, out assetReference);

            if (!isAssetReferenceFound)
                Log.PrintError(log: $"Asset Reference with key '<gb>{key}</gb>' <rb>not found</rb>!");

            return isAssetReferenceFound;
        }

        protected bool TryGetDynamicAssetReference(TKey key, out AssetReference assetReference)
        {
            bool isAssetReferenceFound = _dynamicReferencesDictionary.TryGetValue(key, out assetReference);

            if (!isAssetReferenceFound)
                Log.PrintError(log: $"<gb>Asset Reference with key '<gb>{key}</gb>' <rb>not found</rb>!");

            return isAssetReferenceFound;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private static void DynamicGameObjectLoaded<TObject>(GameObject prefab, SpawnParams<TObject> spawnParams)
            where TObject : class
        {
            Vector3 worldPosition = spawnParams.WorldPosition;
            Quaternion rotation = spawnParams.Rotation;

            GameObject instance = Object.Instantiate(prefab, worldPosition, rotation);

            if (instance is TObject result)
                spawnParams.SendSuccessCallback(result);
            else
                Log.PrintError(log: "Wrong prefab type!");
        }

        private static void DynamicNetworkObjectLoaded<TObject>(NetworkObject prefab, SpawnParams<TObject> spawnParams)
            where TObject : class
        {
            Vector3 worldPosition = spawnParams.WorldPosition;
            Quaternion rotation = spawnParams.Rotation;
            ulong ownerID = spawnParams.OwnerID;

            NetworkSpawnManager spawnManager = GetNetworkSpawnManager();

            NetworkObject instance = spawnManager.InstantiateAndSpawn(
                networkPrefab: prefab,
                ownerClientId: ownerID,
                destroyWithScene: true,
                position: worldPosition,
                rotation: rotation
            );

            if (instance is TObject result)
            {
                spawnParams.SendSetupInstance(result);
                spawnParams.SendSuccessCallback(result);
            }
            else
                Log.PrintError(log: "Wrong prefab type!");
        }

        private static NetworkSpawnManager GetNetworkSpawnManager() =>
            NetworkManager.Singleton.SpawnManager;

        private static bool TryAddAsset(TKey key, AssetReference assetReference,
            Dictionary<TKey, AssetReference> dictionary)
        {
            bool success = dictionary.TryAdd(key, assetReference);

            if (success)
                return true;

            Log.PrintError(log: $"Dictionary <rb>already contains</rb> key '<gb>{key}</gb>'");
            return false;
        }
    }
}