using GameCore.Gameplay.Entities.Player;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.Network
{
    public class NetworkSpawner : NetworkBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private PlayerEntity _playerPrefab;

        // FIELDS: --------------------------------------------------------------------------------

        private static NetworkSpawner _instance;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _instance = this;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SpawnPlayer(ulong clientID) => SpawnPlayerServerRpc(clientID);

        public void SpawnNetworkObject() =>
            NetworkObject.Spawn();

        public bool IsSpawnerReady() => IsSpawned;

        public static NetworkSpawner Get() => _instance;

        [ServerRpc]
        private void SpawnPlayerServerRpc(ulong clientID)
        {
            PlayerEntity playerInstance = Instantiate(_playerPrefab);
            NetworkObject playerNetworkObject = playerInstance.GetNetworkObject();
            playerNetworkObject.SpawnWithOwnership(clientID);
        }
    }
}