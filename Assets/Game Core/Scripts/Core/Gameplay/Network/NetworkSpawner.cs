using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Items;
using GameCore.Infrastructure.Providers.Gameplay.ItemsMeta;
using GameCore.Observers.Gameplay.PlayerInteraction;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Network
{
    public class NetworkSpawner : NetworkBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IItemsMetaProvider itemsMetaProvider,
            IPlayerInteractionObserver playerInteractionObserver)
        {
            _itemsMetaProvider = itemsMetaProvider;
            _playerInteractionObserver = playerInteractionObserver;
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private PlayerEntity _playerPrefab;

        // FIELDS: --------------------------------------------------------------------------------

        private static NetworkSpawner _instance;

        private IItemsMetaProvider _itemsMetaProvider;
        private IPlayerInteractionObserver _playerInteractionObserver;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _instance = this;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SpawnPlayer(ulong clientID) => SpawnPlayerServerRpc(clientID);

        public void SpawnItem(int itemID)
        {
            bool isItemExists = _itemsMetaProvider.TryGetItemMeta(itemID, out ItemMeta itemMeta);

            if (!isItemExists)
            {
                string itemNotFoundLog = Log.HandleLog($"Item with ID <gb>({itemID})</gb> <rb>not found</rb>!");
                Debug.Log(itemNotFoundLog);
                return;
            }
        }

        public void Spawn() =>
            NetworkObject.Spawn();

        public bool IsSpawnerReady() => IsSpawned;

        public static NetworkSpawner Get() => _instance;

        [ServerRpc]
        private void SpawnPlayerServerRpc(ulong clientID)
        {
            PlayerEntity playerInstance = Instantiate(_playerPrefab);
            playerInstance.Setup(_playerInteractionObserver); // REWORK, only need for Owner
            
            NetworkObject playerNetworkObject = playerInstance.GetNetworkObject();
            playerNetworkObject.SpawnWithOwnership(clientID);
        }
    }
}