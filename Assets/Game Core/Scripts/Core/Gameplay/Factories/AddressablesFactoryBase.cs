using Cysharp.Threading.Tasks;
using GameCore.Gameplay.AssetsStorages;
using GameCore.Gameplay.Network.DynamicPrefabs;
using GameCore.Gameplay.Utilities;
using GameCore.Infrastructure.Providers.Global;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

namespace GameCore.Gameplay.Factories
{
    public abstract class AddressablesFactoryBase<TKey>
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        protected AddressablesFactoryBase(
            DiContainer diContainer,
            IAssetsProvider assetsProvider,
            IAssetsStorage<TKey> assetsStorage,
            IDynamicPrefabsLoaderDecorator dynamicPrefabsLoaderDecorator
        )
        {
            _diContainer = diContainer;
            _assetsProvider = assetsProvider;
            _assetsStorage = assetsStorage;
            _dynamicPrefabsLoaderDecorator = dynamicPrefabsLoaderDecorator;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly DiContainer _diContainer;
        private readonly IAssetsProvider _assetsProvider;
        private readonly IAssetsStorage<TKey> _assetsStorage;
        private readonly IDynamicPrefabsLoaderDecorator _dynamicPrefabsLoaderDecorator;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected async UniTask<GameObject> LoadAndCreateGameObject<TObject>(TKey key, SpawnParams<TObject> spawnParams)
            where TObject : class
        {
            return await LoadAndCreateGameObject(key, spawnParams, _diContainer);
        }

        protected async UniTask<GameObject> LoadAndCreateGameObject<TObject>(TKey key, SpawnParams<TObject> spawnParams,
            DiContainer diContainer) where TObject : class
        {
            AssetReference assetReference = spawnParams.AssetReference;
            bool containsAssetReference = assetReference != null;

            GameObject prefab;

            if (containsAssetReference)
                prefab = await LoadAsset<GameObject>(assetReference);
            else
                prefab = await LoadAsset<GameObject>(key);

            if (prefab == null)
            {
                Log.PrintError(log: $"Prefab with key '<gb>{key}</gb>' <rb>not found</rb>!");
                return null;
            }

            bool isUI = spawnParams.IsUI;
            Vector3 worldPosition = spawnParams.WorldPosition;
            Quaternion rotation = spawnParams.Rotation;
            Transform parent = spawnParams.Parent;

            GameObject objectInstance = isUI 
                ? diContainer.InstantiatePrefab(prefab, parent)
                : diContainer.InstantiatePrefab(prefab, worldPosition, rotation, parent);

            if (objectInstance.TryGetComponent(out TObject componentInstance))
            {
                spawnParams.SendSetupInstance(componentInstance);
                spawnParams.SendSuccessCallback(componentInstance);
                return objectInstance;
            }

            Log.PrintError(log: "Component of type '<gb>NetworkObject</gb>' <rb>not found</rb>!");
            return null;
        }

        protected async UniTask<NetworkObject> LoadAndCreateNetworkObject<TObject>(TKey key,
            SpawnParams<TObject> spawnParams) where TObject : class
        {
            AssetReference assetReference = spawnParams.AssetReference;
            bool containsAssetReference = assetReference != null;

            GameObject prefab;

            if (containsAssetReference)
                prefab = await LoadAsset<GameObject>(assetReference);
            else
                prefab = await LoadAsset<GameObject>(key);

            if (prefab == null)
            {
                Log.PrintError(log: $"Prefab with key '<gb>{key}</gb>' <rb>not found</rb>!");
                return null;
            }

            if (!prefab.TryGetComponent(out NetworkObject prefabNetworkObject))
            {
                Log.PrintError(log: "Component of type '<gb>NetworkObject</gb>' <rb>not found</rb>!");
                return null;
            }

            Vector3 worldPosition = spawnParams.WorldPosition;
            Quaternion rotation = spawnParams.Rotation;
            ulong ownerID = spawnParams.OwnerID;

            NetworkSpawnManager spawnManager = GetNetworkSpawnManager();

            NetworkObject instanceNetworkObject = spawnManager.InstantiateAndSpawn(
                networkPrefab: prefabNetworkObject,
                ownerClientId: ownerID,
                destroyWithScene: true,
                position: worldPosition,
                rotation: rotation
            );

            if (instanceNetworkObject.TryGetComponent(out TObject objectInstance))
            {
                spawnParams.SendSetupInstance(objectInstance);
                spawnParams.SendSuccessCallback(objectInstance);
                return instanceNetworkObject;
            }

            Log.PrintError(log: $"Component of type '<gb>{typeof(TObject)}</gb>' <rb>not found</rb>!");
            return null;
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

        protected void ReleaseAsset(AssetReference assetReference) =>
            Addressables.Release(assetReference);

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

            if (instance.TryGetComponent(out TObject component))
            {
                spawnParams.SendSetupInstance(component);
                spawnParams.SendSuccessCallback(component);
            }
            else
                Log.PrintError(log: "Wrong prefab type!");
        }

        private static NetworkSpawnManager GetNetworkSpawnManager() =>
            NetworkManager.Singleton.SpawnManager;
        
        private async UniTask<T> LoadAsset<T>(TKey key) where T : class
        {
            bool isAssetReferenceFound = TryGetAssetReference(key, out AssetReference assetReference);

            if (!isAssetReferenceFound)
                return null;

            var gameObjectPrefab = await _assetsProvider.LoadAsset<GameObject>(assetReference);

            if (gameObjectPrefab.TryGetComponent(out T result))
                return result;

            if (gameObjectPrefab is T component)
                return component;

            Log.PrintError(log: $"Component of type '<gb>{typeof(T)}</gb>' for key '<gb>{key}</gb>' " +
                                "<rb>not found</rb>!");

            return null;
        }

        private async UniTask<T> LoadAsset<T>(AssetReference assetReference) where T : class
        {
            var gameObjectPrefab = await _assetsProvider.LoadAsset<GameObject>(assetReference);

            if (gameObjectPrefab.TryGetComponent(out T result))
                return result;

            if (gameObjectPrefab is T component)
                return component;

            Log.PrintError(log: $"Component of type '<gb>{typeof(T)}</gb>; <rb>not found</rb>!");
            return null;
        }

        private bool TryGetDynamicAssetGUID(TKey key, out string guid)
        {
            if (!TryGetDynamicAssetReference(key, out AssetReference assetReference))
            {
                guid = string.Empty;
                return false;
            }

            guid = assetReference.AssetGUID;
            return true;
        }
        
        private bool TryGetAssetReference(TKey key, out AssetReference assetReference) =>
            _assetsStorage.TryGetAssetReference(key, out assetReference);

        private bool TryGetDynamicAssetReference(TKey key, out AssetReference assetReference) =>
            _assetsStorage.TryGetDynamicAssetReference(key, out assetReference);
    }
}