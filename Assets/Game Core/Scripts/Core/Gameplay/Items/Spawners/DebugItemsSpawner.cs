using GameCore.Gameplay.Factories.Items;
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
        private void Construct(IItemsFactory itemsFactory) =>
            _itemsFactory = itemsFactory;

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private bool _spawnAtStart = true;
        
        [Title(Constants.References)]
        [SerializeField, Required]
        private ItemMeta _itemMeta;

        // FIELDS: --------------------------------------------------------------------------------

        private IItemsFactory _itemsFactory;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Start()
        {
            if (!_spawnAtStart)
                return;
            
            SpawnItem();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public string GetItemName() =>
            _itemMeta != null ? $"'{_itemMeta.ItemName}'" : "'none'";

        // PRIVATE METHODS: -----------------------------------------------------------------------

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

        // DEBUG BUTTONS: -------------------------------------------------------------------------

        [Title(Constants.DebugButtons)]
        [Button(buttonSize: 35), DisableInEditorMode]
        private void DebugSpawnItem() => SpawnItem();
    }
}