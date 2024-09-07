using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace GameCore.Utilities
{
#warning НУЖЕН РЕФАКТОРИНГ TKey ??
    public interface IAddressablesFactory<in TKey>
    {
        UniTask WarmUp();
        void AddAsset(TKey key, AssetReference assetReference);
        void AddDynamicAsset(TKey key, AssetReference assetReference);
        UniTask<T> LoadAndReleaseAsset<T>(AssetReference assetReference) where T : class;
    }
}