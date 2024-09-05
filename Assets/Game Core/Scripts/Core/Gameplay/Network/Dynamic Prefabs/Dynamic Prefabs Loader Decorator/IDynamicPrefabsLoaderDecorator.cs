using System;
using Unity.Netcode;

namespace GameCore.Gameplay.Network.DynamicPrefabs
{
    public interface IDynamicPrefabsLoaderDecorator
    {
        event Action<string, Action<NetworkObject>> OnTrySpawnPrefabEvent;
        void LoadAndGetPrefab(string guid, Action<NetworkObject> loadCallback);
    }
}