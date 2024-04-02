using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Gameplay.Factories;
using GameCore.Gameplay.Network;
using GameCore.Utilities;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GameCore.Infrastructure.Providers.Global
{
    public class AssetsProvider : AssetsProviderBase, IAssetsProvider
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public AssetsProvider()
        {
            _scenesLoaderPrefab = Load<GameObject>(path: AssetsPaths.ScenesLoaderPrefab);
            _menuPrefabsList = Load<MenuPrefabsListMeta>(path: AssetsPaths.MenuPrefabsList);
            _networkManager = Load<NetworkManager>(path: AssetsPaths.NetworkManager);
            _networkHorror = Load<NetworkHorror>(path: AssetsPaths.NetworkHorror);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly GameObject _scenesLoaderPrefab;
        private readonly MenuPrefabsListMeta _menuPrefabsList;
        private readonly NetworkManager _networkManager;
        private readonly NetworkHorror _networkHorror;
        
        private readonly Dictionary<string, AsyncOperationHandle> _completedCache = new();
        private readonly Dictionary<string, List<AsyncOperationHandle>> _handles = new();
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Instantiate() =>
            Addressables.InitializeAsync();

        public async UniTask<T> LoadAsset<T>(AssetReference assetReference) where T : class
        {
            if (_completedCache.TryGetValue(assetReference.AssetGUID, out AsyncOperationHandle completedHandle))
                return completedHandle.Result as T;

            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(assetReference);
            return await RunWitchCacheOnComplete(handle, cacheKey: assetReference.AssetGUID);
        }

        public async UniTask<T> LoadAsset<T>(string address) where T : class
        {
            if (_completedCache.TryGetValue(address, out AsyncOperationHandle completedHandle))
                return completedHandle.Result as T;

            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(address);
            return await RunWitchCacheOnComplete(handle, cacheKey: address);
        }

        public async UniTask<GameObject> Instantiate(string address) =>
            await Addressables.InstantiateAsync(address).Task;

        public async UniTask<GameObject> Instantiate(string address, Vector3 at) =>
            await Addressables.InstantiateAsync(address, at, Quaternion.identity).Task;

        public async UniTask<GameObject> Instantiate(string address, Transform parent) =>
            await Addressables.InstantiateAsync(address, parent).Task;
        
        public void Cleanup()
        {
            foreach (List<AsyncOperationHandle> resourceHandles in _handles.Values)
            {
                foreach (AsyncOperationHandle handle in resourceHandles)
                {
                    Addressables.Release(handle);
                }
            }
            
            _completedCache.Clear();
            _handles.Clear();
        }
        
        public GameObject GetScenesLoaderPrefab() => _scenesLoaderPrefab;
        
        public MenuPrefabsListMeta GetMenuPrefabsList() => _menuPrefabsList;

        public NetworkManager GetNetworkManager() => _networkManager;
        
        public NetworkHorror GetNetworkHorror() => _networkHorror;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async UniTask<T> RunWitchCacheOnComplete<T>(AsyncOperationHandle<T> handle, string cacheKey)
            where T : class
        {
            handle.Completed += operationHandle =>
            {
                _completedCache[cacheKey] = operationHandle;
            };

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