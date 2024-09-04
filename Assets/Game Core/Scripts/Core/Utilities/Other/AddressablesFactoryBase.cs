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
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IAssetsProvider _assetsProvider;
        private readonly Dictionary<Type, AssetReference> _referencesDictionary;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected async UniTask SetupReferencesDictionary<T>(IEnumerable<AssetReference> assetReferences)
            where T : class
        {
            List<AsyncOperationHandle<GameObject>> handles = new();

            foreach (AssetReference assetReference in assetReferences)
            {
                var handle = Addressables.LoadAssetAsync<GameObject>(assetReference);
                handles.Add(handle);

                GameObject menuPrefab = await handle.Task;

                if (!menuPrefab.TryGetComponent(out T menuView))
                {
                    Log.PrintError(log: $"<gb>Asset Reference '{assetReference.AssetGUID}'</gb> " +
                                        "was <rb>not found</rb>!");
                    continue;
                }

                Type type = menuView.GetType();
                bool success = _referencesDictionary.TryAdd(type, assetReference);

                if (success)
                    continue;

                Log.PrintError(log: $"Key '<gb>{type.Name}</gb>' <rb>already exists</rb>!");
            }

            foreach (var handle in handles)
                Addressables.Release(handle);
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

        protected bool TryGetAssetReference(Type type, out AssetReference assetReference)
        {
            bool isAssetReferenceFound = _referencesDictionary.TryGetValue(type, out assetReference);

            if (!isAssetReferenceFound)
                Log.PrintError(log: $"<gb>Asset Reference '<gb>{type.Name}</gb>' <rb>not found</rb>!");

            return isAssetReferenceFound;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private bool TryGetAssetReference<T>(out AssetReference assetReference) where T : class
        {
            Type key = typeof(T);
            return TryGetAssetReference(key, out assetReference);
        }
    }
}