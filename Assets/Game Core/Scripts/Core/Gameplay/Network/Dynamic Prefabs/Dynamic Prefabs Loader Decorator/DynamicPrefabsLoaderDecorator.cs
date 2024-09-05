using System;
using UnityEngine;

namespace GameCore.Gameplay.Network.DynamicPrefabs
{
    public class DynamicPrefabsLoaderDecorator : IDynamicPrefabsLoaderDecorator
    {
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<string, Action<GameObject>> OnTrySpawnPrefabEvent = delegate { };

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void LoadAndGetPrefab(string guid, Action<GameObject> callback) =>
            OnTrySpawnPrefabEvent.Invoke(guid, callback);
    }
}