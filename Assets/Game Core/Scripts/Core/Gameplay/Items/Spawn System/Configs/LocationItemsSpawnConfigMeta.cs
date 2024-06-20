using System.Collections.Generic;
using CustomEditors;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Items.SpawnSystem
{
    public class LocationItemsSpawnConfigMeta : EditorMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        [ListDrawerSettings(ListElementLabelName = "Label")]
        private List<ItemSpawnConfig> _configs = new();

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void OnEnable()
        {
            base.OnEnable();

            foreach (ItemSpawnConfig itemSpawnConfig in _configs)
                itemSpawnConfig.CalculateThirdFloorChance();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public IReadOnlyList<ItemSpawnConfig> GetAllConfigs() => _configs;

        public override string GetMetaCategory() =>
            EditorConstants.ItemsSpawnCategory;
    }
}