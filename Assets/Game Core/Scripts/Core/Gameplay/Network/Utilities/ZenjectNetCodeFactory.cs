using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Network.Utilities
{
    public class ZenjectNetCodeFactory : INetworkPrefabInstanceHandler
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ZenjectNetCodeFactory(GameObject prefab, DiContainer container)
        {
            _prefab = prefab;
            _container = container;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly GameObject _prefab;
        private readonly DiContainer _container;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
        {
            GameObjectCreationParameters parameters = new()
            {
                Name = $"{_prefab.name} | Owner: {ownerClientId}",
                Position = position,
                Rotation = rotation
            };

            return _container.InstantiateNetworkPrefab(_prefab, parameters);
        }

        public void Destroy(NetworkObject networkObject) =>
            Object.Destroy(networkObject.gameObject);
    }

    public static class ContainerExtension
    {
        // FIELDS: --------------------------------------------------------------------------------

        private static readonly GameObjectCreationParameters DefaultParameters = GameObjectCreationParameters.Default;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public static NetworkObject InstantiateNetworkPrefab(this DiContainer container, GameObject prefab,
            GameObjectCreationParameters creationParameters = null)
        {
            //bool state = prefab.activeSelf;
            prefab.SetActive(false);
            
            GameObject instance = container.InstantiatePrefab(prefab, creationParameters ?? DefaultParameters);
            
            instance.SetActive(true);
            prefab.SetActive(true);
            
            var networkObject = instance.GetComponent<NetworkObject>();
            return networkObject;
        }
    }
}