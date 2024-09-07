using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Utilities;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;

namespace GameCore.Infrastructure.Providers.Global
{
    public sealed class AssetsProvider : AssetsProviderBase, IAssetsProvider, IInitializable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public AssetsProvider()
        {
            _scenesLoaderPrefab = Load<GameObject>(path: AssetsPaths.ScenesLoaderPrefab);
            _networkManager = Load<NetworkManager>(path: AssetsPaths.NetworkManager);
            _completedCache = new Dictionary<string, AsyncOperationHandle>();
            _handles = new Dictionary<string, List<AsyncOperationHandle>>();
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly GameObject _scenesLoaderPrefab;
        private readonly NetworkManager _networkManager;

        private readonly Dictionary<string, AsyncOperationHandle> _completedCache;
        private readonly Dictionary<string, List<AsyncOperationHandle>> _handles;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Initialize() =>
            Addressables.InitializeAsync();

        public async UniTask<T> LoadAsset<T>(AssetReference assetReference) where T : class
        {
            if (_completedCache.TryGetValue(assetReference.AssetGUID, out AsyncOperationHandle completedHandle))
                return completedHandle.Result as T;

            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(assetReference);
            return await RunWitchCacheOnComplete(handle, cacheKey: assetReference.AssetGUID);
        }

        public async UniTask<T> LoadAndForgetAsset<T>(AssetReference assetReference) where T : class
        {
            var asset = await LoadAsset<T>(assetReference);
            ReleaseAsset(assetReference);
            return asset;
        }

        public async UniTask<T> LoadAsset<T>(string address) where T : class
        {
            if (_completedCache.TryGetValue(address, out AsyncOperationHandle completedHandle))
                return completedHandle.Result as T;

            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(address);
            return await RunWitchCacheOnComplete(handle, cacheKey: address);
        }

#warning НЕ УВЕРЕН ЧТО ПРАВИЛЬНО СДЕЛАНО
        public void ReleaseAsset(AssetReference assetReference)
        {
            bool containsCache =
                _completedCache.TryGetValue(assetReference.AssetGUID, out AsyncOperationHandle cachedHandle);
            
            bool containsHandle =
                _handles.TryGetValue(assetReference.AssetGUID, out List<AsyncOperationHandle> handles);
            
            if (containsCache)
            {
                if (cachedHandle.IsValid())
                    Addressables.Release(cachedHandle);
                
                _completedCache.Remove(assetReference.AssetGUID);
            }
            else
                Addressables.Release(assetReference);

            if (!containsHandle)
                return;
            
            foreach (AsyncOperationHandle handle in handles)
            {
                if (!handle.IsValid())
                    continue;
                    
                Addressables.Release(handle);
            }
        }

        public async UniTask<GameObject> Instantiate(string address) =>
            await Addressables.InstantiateAsync(address).Task; // Skips 1 frame before instantiation.

        public async UniTask<GameObject> Instantiate(string address, Vector3 at) =>
            await Addressables.InstantiateAsync(address, at, Quaternion.identity)
                .Task; // Skips 1 frame before instantiation.

        public async UniTask<GameObject> Instantiate(string address, Transform parent) =>
            await Addressables.InstantiateAsync(address, parent).Task; // Skips 1 frame before instantiation.

        public void Cleanup()
        {
            foreach (List<AsyncOperationHandle> resourceHandles in _handles.Values)
            {
                foreach (AsyncOperationHandle handle in resourceHandles)
                {
                    if (!handle.IsValid())
                        continue;
                    
                    Addressables.Release(handle);
                }
            }

            _completedCache.Clear();
            _handles.Clear();
        }

        public GameObject GetScenesLoaderPrefab() => _scenesLoaderPrefab;
        public NetworkManager GetNetworkManager() => _networkManager;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async UniTask<T> RunWitchCacheOnComplete<T>(AsyncOperationHandle<T> handle, string cacheKey)
            where T : class
        {
            handle.Completed += operationHandle => { _completedCache[cacheKey] = operationHandle; };

            AddHandle(cacheKey, handle);

            return await handle.Task;
        }

        private void AddHandle<T>(string key, AsyncOperationHandle<T> handle) where T : class
        {
            if (!_handles.TryGetValue(key, out List<AsyncOperationHandle> resourceHandles))
            {
                resourceHandles = new List<AsyncOperationHandle>();
                _handles[key] = resourceHandles;
            }

            resourceHandles.Add(handle);
        }
    }
}