using Cysharp.Threading.Tasks;
using GameCore.Gameplay.Factories;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameCore.Infrastructure.Providers.Global
{
    public interface IAssetsProvider
    {
        void Instantiate();
        UniTask<T> LoadAsset<T>(AssetReference assetReference) where T : class;
        UniTask<T> LoadAsset<T>(string address) where T : class;
        UniTask<GameObject> Instantiate(string address);
        UniTask<GameObject> Instantiate(string address, Vector3 at);
        UniTask<GameObject> Instantiate(string address, Transform parent);
        void Cleanup();
        GameObject GetScenesLoaderPrefab();
        MenuPrefabsListMeta GetMenuPrefabsList();
    }
}