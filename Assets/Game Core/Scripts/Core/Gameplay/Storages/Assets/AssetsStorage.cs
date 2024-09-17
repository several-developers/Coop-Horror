using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Infrastructure.Providers.Global;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameCore.Gameplay.Storages.Assets
{
    public abstract class AssetsStorage<TKey> : IAssetsStorage<TKey>
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        protected AssetsStorage(IAssetsProvider assetsProvider)
        {
            _assetsProvider = assetsProvider;
            _referencesDictionary = new Dictionary<TKey, AssetReference>();
            _dynamicReferencesDictionary = new Dictionary<TKey, AssetReference>();
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly IAssetsProvider _assetsProvider;
        private readonly Dictionary<TKey, AssetReference> _referencesDictionary;
        private readonly Dictionary<TKey, AssetReference> _dynamicReferencesDictionary;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public abstract UniTask WarmUp();
        
        public async UniTask<T> LoadAndReleaseAsset<T>(AssetReference assetReference) where T : class
        {
            var handle = await _assetsProvider.LoadAsset<GameObject>(assetReference);
            
            if (!handle.TryGetComponent(out T t))
                Log.PrintError(log: $"Component of type '<gb>{typeof(T)}</gb>' <rb>not found</rb>!");
            
            _assetsProvider.ReleaseAsset(assetReference);
            
            return t;
        }
        
        public async UniTask<T> LoadAsset<T>(AssetReference assetReference) where T : class
        {
            var handle = await _assetsProvider.LoadAsset<GameObject>(assetReference);

            if (!handle.TryGetComponent(out T t))
                Log.PrintError(log: $"Component of type '<gb>{typeof(T)}</gb>' <rb>not found</rb>!");
            
            return t;
        }
        
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
        
        public bool TryGetAssetReference(TKey key, out AssetReference assetReference)
        {
            bool isAssetReferenceFound = _referencesDictionary.TryGetValue(key, out assetReference);

            if (!isAssetReferenceFound)
                Log.PrintError(log: $"Asset Reference with key '<gb>{key}</gb>' <rb>not found</rb>!");

            return isAssetReferenceFound;
        }
        
        public bool TryGetDynamicAssetReference(TKey key, out AssetReference assetReference)
        {
            bool isAssetReferenceFound = _dynamicReferencesDictionary.TryGetValue(key, out assetReference);

            if (!isAssetReferenceFound)
                Log.PrintError(log: $"Dynamic Asset Reference with key '<gb>{key}</gb>' <rb>not found</rb>!");

            return isAssetReferenceFound;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
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