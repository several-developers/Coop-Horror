using System;
using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.Network.DynamicPrefabs
{
    public class DynamicPrefabsLoaderDecorator : IDynamicPrefabsLoaderDecorator
    {
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<string, Action<GameObject>> OnTrySpawnGameObjectPrefabEvent = delegate { };
        public event Action<string, Action<NetworkObject>> OnTrySpawnNetworkObjectPrefabEvent = delegate { };

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void LoadAndGetGameObjectPrefab(string guid, Action<GameObject> loadCallback) =>
            OnTrySpawnGameObjectPrefabEvent.Invoke(guid, loadCallback);

        public void LoadAndGetNetworkObjectPrefab(string guid, Action<NetworkObject> loadCallback) =>
            OnTrySpawnNetworkObjectPrefabEvent.Invoke(guid, loadCallback);
    }
}