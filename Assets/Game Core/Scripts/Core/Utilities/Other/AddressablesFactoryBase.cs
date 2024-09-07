using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Infrastructure.Providers.Global;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameCore.Utilities
{
    public abstract class AddressablesFactoryBase<TKey> : IAddressablesFactory<TKey>
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        protected AddressablesFactoryBase(IAssetsProvider assetsProvider)
        {
            _assetsProvider = assetsProvider;
            _referencesDictionary = new Dictionary<TKey, AssetReference>();
            _dynamicReferencesDictionary = new Dictionary<TKey, AssetReference>();
        }

        // PROPERTIES: ----------------------------------------------------------------------------


        // FIELDS: --------------------------------------------------------------------------------

        private readonly IAssetsProvider _assetsProvider;
        private readonly Dictionary<TKey, AssetReference> _referencesDictionary;
        private readonly Dictionary<TKey, AssetReference> _dynamicReferencesDictionary;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public abstract UniTask WarmUp();

        public void AddAsset(TKey key, AssetReference assetReference)
        {
            bool success = AddAsset(key, assetReference, _referencesDictionary);

            if (!success)
                return;

            string log = Log.HandleLog($"Added Asset with key '<gb>{key}</gb>'");
            Debug.Log(log);
        }

        public void AddDynamicAsset(TKey key, AssetReference assetReference)
        {
            bool success = AddAsset(key, assetReference, _dynamicReferencesDictionary);

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

        private static bool AddAsset(TKey key, AssetReference assetReference,
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