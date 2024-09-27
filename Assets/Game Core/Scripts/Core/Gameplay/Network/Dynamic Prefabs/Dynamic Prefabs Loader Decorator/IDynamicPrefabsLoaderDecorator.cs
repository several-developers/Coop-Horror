using System;
using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.Network.DynamicPrefabs
{
    public interface IDynamicPrefabsLoaderDecorator
    {
        event Action<string, Action<GameObject>> OnTrySpawnGameObjectPrefabEvent;
        event Action<string, Action<NetworkObject>> OnTrySpawnNetworkObjectPrefabEvent;
        void LoadAndGetGameObjectPrefab(string guid, Action<GameObject> loadCallback);
        void LoadAndGetNetworkObjectPrefab(string guid, Action<NetworkObject> loadCallback);
    }
}