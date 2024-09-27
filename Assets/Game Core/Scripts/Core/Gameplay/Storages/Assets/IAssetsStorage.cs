using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace GameCore.Gameplay.Storages.Assets
{
    public interface IAssetsStorage<in TKey> : IWarmUp
    {
        UniTask<T> LoadAndReleaseAsset<T>(AssetReference assetReference) where T : class;
        void AddAsset(TKey key, AssetReference assetReference);
        void AddDynamicAsset(TKey key, AssetReference assetReference);
        bool TryGetAssetReference(TKey key, out AssetReference assetReference);
        bool TryGetDynamicAssetReference(TKey key, out AssetReference assetReference);
    }
}