using System;
using UnityEngine;

namespace GameCore.Gameplay.Network.DynamicPrefabs
{
    public interface IDynamicPrefabsLoaderDecorator
    {
        event Action<string, Action<GameObject>> OnTrySpawnPrefabEvent;
        void LoadAndGetPrefab(string guid, Action<GameObject> loadCallback);
    }
}