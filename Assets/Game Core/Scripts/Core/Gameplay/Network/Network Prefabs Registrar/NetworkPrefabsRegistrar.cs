using GameCore.Gameplay.Network.Utilities;
using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.Network.PrefabsRegistrar
{
    public class NetworkPrefabsRegistrar : INetworkPrefabsRegistrar
    {
        // PROPERTIES: ----------------------------------------------------------------------------

        private NetworkManager NetworkManager
        {
            get
            {
                if (_isNetworkManagerFound)
                    return _networkManager;

                _networkManager = NetworkManager.Singleton;
                _isNetworkManagerFound = true;

                return _networkManager;
            }
        }

        // FIELDS: --------------------------------------------------------------------------------

        private NetworkManager _networkManager;
        private bool _isNetworkManagerFound;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Register(GameObject prefab)
        {
            if (!IsPrefabValid(prefab))
                return;
            
            NetworkManager.AddNetworkPrefab(prefab);

            NetworkManager.PrefabHandler.AddHandler(
                networkPrefabAsset: prefab,
                instanceHandler: new ZenjectNetCodeFactory(prefab, diContainer: null)
            );
        }

        public void Remove(GameObject prefab)
        {
            if (!IsPrefabValid(prefab))
                return;
            
            NetworkManager.RemoveNetworkPrefab(prefab);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private bool IsPrefabValid(GameObject prefab)
        {
            bool isPrefabValid = prefab.TryGetComponent<NetworkObject>(out _);

            if (!isPrefabValid)
                Log.PrintError(log: $"Prefab <gb>{prefab.name}</gb> <rb>doesn't contains</rb> Network Object!");
            
            return isPrefabValid;
        }
    }
}