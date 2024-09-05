using System;
using Unity.Netcode;

namespace GameCore.Gameplay.Network.DynamicPrefabs
{
    public class DynamicPrefabsLoaderDecorator : IDynamicPrefabsLoaderDecorator
    {
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<string, Action<NetworkObject>> OnTrySpawnPrefabEvent = delegate { };

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void LoadAndGetPrefab(string guid, Action<NetworkObject> loadCallback) =>
            OnTrySpawnPrefabEvent.Invoke(guid, loadCallback);
    }
}