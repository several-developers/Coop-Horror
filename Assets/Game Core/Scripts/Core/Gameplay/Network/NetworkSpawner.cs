using GameCore.Gameplay.Items;
using GameCore.Infrastructure.Providers.Gameplay.ItemsMeta;
using GameCore.Observers.Gameplay.PlayerInteraction;
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
        
        // FIELDS: --------------------------------------------------------------------------------

        private static NetworkSpawner _instance;

        private IItemsMetaProvider _itemsMetaProvider;
        private IPlayerInteractionObserver _playerInteractionObserver;

        public IPlayerInteractionObserver PlayerInteractionObserver => _playerInteractionObserver; // TEMP

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            Debug.Log("Loading network spawner");
            _instance = this;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

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

        public void DestroyObject(NetworkObject networkObject) =>
            DestroyObjectServerRpc(networkObject);

        public void Spawn() =>
            NetworkObject.Spawn();

        public bool IsSpawnerReady() => IsSpawned;

        public static NetworkSpawner Get() => _instance;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        [ServerRpc(RequireOwnership = false)]
        private void DestroyObjectServerRpc(NetworkObjectReference networkObjectReference)
        {
            bool isNetworkObject = networkObjectReference.TryGet(out NetworkObject networkObject);

            if (!isNetworkObject)
                return;

            networkObject.Despawn();
        }
    }
}