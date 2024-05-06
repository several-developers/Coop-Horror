using GameCore.Gameplay.Factories.Items;
using GameCore.Gameplay.Network;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Items.Spawners
{
    public class DebugItemsSpawner : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(INetcodeHooks netcodeHooks, IItemsFactory itemsFactory)
        {
            _netcodeHooks = netcodeHooks;
            _itemsFactory = itemsFactory;
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private bool _spawnAtStart = true;
        
        [Title(Constants.References)]
        [SerializeField, Required]
        private ItemMeta _itemMeta;

        // FIELDS: --------------------------------------------------------------------------------

        private INetcodeHooks _netcodeHooks;
        private IItemsFactory _itemsFactory;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _netcodeHooks.OnNetworkSpawnHookEvent += OnNetworkSpawnHook;

        private void Start()
        {
            if (_netcodeHooks.IsSpawned)
                TrySpawnItem();
        }

        private void OnDestroy() =>
            _netcodeHooks.OnNetworkSpawnHookEvent -= OnNetworkSpawnHook;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public string GetItemName() =>
            _itemMeta != null ? $"'{_itemMeta.ItemName}'" : "'none'";

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void TrySpawnItem()
        {
            if (!_spawnAtStart)
                return;

            if (!_netcodeHooks.IsOwner)
                return;
            
            SpawnItem();
        }
        
        private void SpawnItem()
        {
            if (!IsItemMetaValid())
                return;

            Vector3 position = transform.position;
            position.y += 0.25f;
            position = position.GetRandomPosition(radius: 0.25f);

            _itemsFactory.CreateItem(_itemMeta.ItemID, position, out _);
        }

        private bool IsItemMetaValid()
        {
            bool isValid = _itemMeta != null;

            if (isValid)
                return true;

            Log.PrintError(log: $"Item Meta <rb>not found</rb>!");
            return false;
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnNetworkSpawnHook() => TrySpawnItem();

        // DEBUG BUTTONS: -------------------------------------------------------------------------

        [Title(Constants.DebugButtons)]
        [Button(buttonSize: 35), DisableInEditorMode]
        private void DebugSpawnItem() => SpawnItem();
    }
}