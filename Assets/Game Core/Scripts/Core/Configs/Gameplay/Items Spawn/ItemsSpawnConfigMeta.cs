using System.Collections.Generic;
using CustomEditors;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Items.SpawnSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Configs.Gameplay.ItemsSpawn
{
    public class ItemsSpawnConfigMeta : EditorMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField, Range(0, 100), SuffixLabel("%", overlay: true)]
        [OnValueChanged(nameof(CalculateThirdFloorChance))]
        private int _firstFloorChance = 25;

        [SerializeField, Range(0, 100), SuffixLabel("%", overlay: true)]
        [OnValueChanged(nameof(CalculateThirdFloorChance))]
        private int _secondFloorChance = 30;
        
        [ShowInInspector, Range(0, 100), SuffixLabel("%", overlay: true), ReadOnly]
        private int _thirdFloorChance;

        [SerializeField]
        private AnimationCurve _itemsDistribution;

        [SerializeField, Space(height: 5)]
        private List<LocationItemsSpawnConfigReference> _itemsSpawnConfigs;

        // PROPERTIES: ----------------------------------------------------------------------------

        public int FirstFloorChance => _firstFloorChance;
        public int SecondFloorChance => _secondFloorChance;
        public AnimationCurve ItemsDistribution => _itemsDistribution;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void OnEnable()
        {
            base.OnEnable();
            CalculateThirdFloorChance();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public bool TryGetItemsSpawnConfig(LocationName locationName, out LocationItemsSpawnConfigMeta result)
        {
            foreach (LocationItemsSpawnConfigReference configReference in _itemsSpawnConfigs)
            {
                bool isMatches = configReference.LocationName == locationName;

                if (!isMatches)
                    continue;

                result = configReference.Config;
                return true;
            }

            Log.PrintError(log: $"Items Spawn Config for location <gb>{locationName}</gb> <rb>not found</rb>!");

            result = null;
            return false;
        }

        public override string GetMetaCategory() =>
            EditorConstants.GameplayConfigsCategory;

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void CalculateThirdFloorChance() =>
            _thirdFloorChance = 100 - (_firstFloorChance + _secondFloorChance);
    }
}