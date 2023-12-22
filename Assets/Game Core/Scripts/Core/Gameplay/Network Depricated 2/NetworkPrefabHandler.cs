using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.NetworkDepricated2
{
    public class NetworkPrefabHandler : INetworkPrefabInstanceHandler
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        public NetworkPrefabHandler(GameObject prefab) =>
            _prefab = prefab;

        // FIELDS: --------------------------------------------------------------------------------

        private readonly GameObject _prefab;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
        {
            GameObject obj = Object.Instantiate(_prefab, position, rotation);
            return obj.GetComponent<NetworkObject>();
        }

        public void Destroy(NetworkObject networkObject) =>
            Object.Destroy(networkObject.gameObject);
    }
}