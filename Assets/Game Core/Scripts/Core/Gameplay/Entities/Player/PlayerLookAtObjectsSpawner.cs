using GameCore.Gameplay.Network;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Player
{
    public class PlayerLookAtObjectsSpawner : NetcodeBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private NetworkObject _prefab;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            PlayerEntity.OnPlayerSpawnedEvent += OnPlayerSpawned;

        public override void OnDestroy()
        {
            base.OnDestroy();

            PlayerEntity.OnPlayerSpawnedEvent -= OnPlayerSpawned;
        }

        // RPC: -----------------------------------------------------------------------------------

        [ServerRpc(RequireOwnership = false)]
        private void SpawnPrefabServerRpc(ulong clientID)
        {
            bool isPlayerFound = PlayerEntity.TryGetPlayer(clientID, out PlayerEntity playerEntity);
            
            if (!isPlayerFound)
                return;

            NetworkObject instance = NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(_prefab);
            instance.TrySetParent(playerEntity.transform);

            //SpawnPrefabClientRpc(clientID, instance);
        }

        [ClientRpc]
        private void SpawnPrefabClientRpc(ulong clientID, NetworkObjectReference prefabInstanceReference)
        {
            bool isPlayerFound = PlayerEntity.TryGetPlayer(clientID, out PlayerEntity playerEntity);

            if (!isPlayerFound)
                return;

            bool isNetworkObjectFound = prefabInstanceReference.TryGet(out NetworkObject prefabInstance);
            
            if (!isNetworkObjectFound)
                return;
            
            Debug.LogWarning("Setup " + clientID);
            playerEntity.SetLookAtObject(prefabInstance.transform);
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            Debug.LogWarning("SPAWNED");
        }

        private void OnPlayerSpawned(PlayerEntity playerEntity)
        {
            if (!IsOwner)
                return;
            
            Debug.LogWarning("PLAYER");
            SpawnPrefabServerRpc(playerEntity.OwnerClientId);
        }
    }
}