using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Infrastructure.Providers.Global;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GameCore.Utilities
{
    public abstract class AddressablesFactoryBase
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        protected AddressablesFactoryBase(IAssetsProvider assetsProvider)
        {
            _assetsProvider = assetsProvider;
            _referencesDictionary = new Dictionary<Type, AssetReference>();
            _dynamicReferencesDictionary = new Dictionary<Type, AssetReference>();
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IAssetsProvider _assetsProvider;
        private readonly Dictionary<Type, AssetReference> _referencesDictionary;
        private readonly Dictionary<Type, AssetReference> _dynamicReferencesDictionary;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public async UniTask LoadAndSaveAsset<T>(AssetReference assetReference) where T : class
        {
            var handle = Addressables.LoadAssetAsync<GameObject>(assetReference);

            await LoadAndSaveAssetReference<T>(_referencesDictionary, handle, assetReference);
            Addressables.Release(handle);
        }

        public async UniTask LoadAndSaveAssetDynamic<T>(AssetReference assetReference) where T : class
        {
            var handle = Addressables.LoadAssetAsync<GameObject>(assetReference);

            await LoadAndSaveAssetReference<T>(_dynamicReferencesDictionary, handle, assetReference);
            Addressables.Release(handle);
        }

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected async UniTask SetupReferencesDictionary<T>(IEnumerable<AssetReference> assetReferences)
            where T : class
        {
            foreach (AssetReference assetReference in assetReferences)
                await LoadAndSaveAsset<T>(assetReference);
        }

        protected async UniTask SetupDynamicReferencesDictionary<T>(IEnumerable<AssetReference> assetReferences)
            where T : class
        {
            foreach (AssetReference assetReference in assetReferences)
                await LoadAndSaveAssetDynamic<T>(assetReference);
        }

        protected async UniTask<T> LoadAsset<T>() where T : class
        {
            bool isAssetReferenceFound = TryGetAssetReference<T>(out AssetReference assetReference);

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

        protected bool TryGetAssetGUID<T>(out string guid) where T : class
        {
            if (!TryGetAssetReference<T>(out AssetReference assetReference))
            {
                guid = string.Empty;
                return false;
            }

            guid = assetReference.AssetGUID;
            return true;
        }

        protected bool TryGetDynamicAssetGUID<T>(out string guid) where T : class
        {
            if (!TryGetDynamicAssetReference<T>(out AssetReference assetReference))
            {
                guid = string.Empty;
                return false;
            }

            guid = assetReference.AssetGUID;
            return true;
        }

        protected bool TryGetAssetReference(Type type, out AssetReference assetReference)
        {
            bool isAssetReferenceFound = _referencesDictionary.TryGetValue(type, out assetReference);

            if (!isAssetReferenceFound)
                Log.PrintError(log: $"<gb>Asset Reference '<gb>{type.Name}</gb>' <rb>not found</rb>!");

            return isAssetReferenceFound;
        }

        protected bool TryGetDynamicAssetReference(Type type, out AssetReference assetReference)
        {
            bool isAssetReferenceFound = _dynamicReferencesDictionary.TryGetValue(type, out assetReference);

            if (!isAssetReferenceFound)
                Log.PrintError(log: $"<gb>Asset Reference '<gb>{type.Name}</gb>' <rb>not found</rb>!");

            return isAssetReferenceFound;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private static async UniTask LoadAndSaveAssetReference<T>(Dictionary<Type, AssetReference> dictionary,
            AsyncOperationHandle<GameObject> handle, AssetReference assetReference) where T : class
        {
            GameObject menuPrefab = await handle.Task;

            if (!menuPrefab.TryGetComponent(out T menuView))
            {
                Log.PrintError(log: $"<gb>Asset Reference '{assetReference.AssetGUID}'</gb> " +
                                    "was <rb>not found</rb>!");
                return;
            }

            Type type = menuView.GetType();
            bool success = dictionary.TryAdd(type, assetReference);

            if (success)
                return;

            Log.PrintError(log: $"Key '<gb>{type.Name}</gb>' <rb>already exists</rb>!");
        }
        
        private bool TryGetAssetReference<T>(out AssetReference assetReference) where T : class
        {
            Type key = typeof(T);
            return TryGetAssetReference(key, out assetReference);
        }

        private bool TryGetDynamicAssetReference<T>(out AssetReference assetReference) where T : class
        {
            Type key = typeof(T);
            return TryGetDynamicAssetReference(key, out assetReference);
        }
    }
}