﻿using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameCore.Infrastructure.Providers.Global
{
    public interface IAssetsProvider
    {
        UniTask<T> LoadAsset<T>(AssetReference assetReference) where T : class;
        UniTask<T> LoadAndForgetAsset<T>(AssetReference assetReference) where T : class;
        UniTask<T> LoadAsset<T>(string address) where T : class;
        void ReleaseAsset(AssetReference assetReference);
        UniTask<GameObject> Instantiate(string address);
        UniTask<GameObject> Instantiate(string address, Vector3 at);
        UniTask<GameObject> Instantiate(string address, Transform parent);
        void Cleanup();
        GameObject GetScenesLoaderPrefab(); // TEMP
        NetworkManager GetNetworkManager();
    }
}