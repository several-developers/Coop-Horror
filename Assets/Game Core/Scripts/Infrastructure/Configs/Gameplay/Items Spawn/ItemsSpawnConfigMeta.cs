using System;
using System.Collections.Generic;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Items.Generators.Dungeon;
using GameCore.Gameplay.Items.Generators.OutdoorChest;
using GameCore.InfrastructureTools.Configs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Infrastructure.Configs.Gameplay.ItemsSpawn
{
    public class ItemsSpawnConfigMeta : ConfigMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [TitleGroup(DungeonConfigTitle)]
        [BoxGroup(DungeonConfigGroup, showLabel: false), SerializeField, Range(0, 100)]
        [SuffixLabel("%", overlay: true)]
        [OnValueChanged(nameof(CalculateThirdFloorChance))]
        private int _firstFloorChance = 25;

        [BoxGroup(DungeonConfigGroup), SerializeField, Range(0, 100)]
        [SuffixLabel("%", overlay: true)]
        [OnValueChanged(nameof(CalculateThirdFloorChance))]
        private int _secondFloorChance = 30;
        
        [BoxGroup(DungeonConfigGroup), ShowInInspector, Range(0, 100), ReadOnly]
        [SuffixLabel("%", overlay: true)]
        private int _thirdFloorChance;

        [BoxGroup(DungeonConfigGroup), SerializeField]
        private AnimationCurve _itemsDistribution;

        [BoxGroup(DungeonConfigGroup), SerializeField, Space(height: 5)]
        private List<LocationItemsSpawnConfigReference> _spawnConfigsReferences;

        [TitleGroup(OutdoorChestsConfigTitle)]
        [BoxGroup(OutdoorChestsConfigGroup, showLabel: false), SerializeField, LabelText(ConfigLabel)]
        private OutdoorChestsItemsSpawnConfig _outdoorChestsItemsSpawnConfig;

        // PROPERTIES: ----------------------------------------------------------------------------

        public int FirstFloorChance => _firstFloorChance;
        public int SecondFloorChance => _secondFloorChance;
        public AnimationCurve ItemsDistribution => _itemsDistribution;

        // FIELDS: --------------------------------------------------------------------------------

        private const string DungeonConfigTitle = "Dungeon Spawn Settings";
        private const string OutdoorChestsConfigTitle = "Outdoor Chests Spawn Settings";
        
        private const string DungeonConfigGroup = DungeonConfigTitle + "/In";
        private const string OutdoorChestsConfigGroup = OutdoorChestsConfigTitle + "/In";

        private const string ConfigLabel = "Config";

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void OnEnable()
        {
            base.OnEnable();
            CalculateThirdFloorChance();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public OutdoorChestsItemsSpawnConfig GetOutdoorChestsItemsSpawnConfig() => _outdoorChestsItemsSpawnConfig;

        public override string GetMetaCategory() =>
            EditorConstants.GameplayConfigsCategory;
        
        public override ConfigScope GetConfigScope() =>
            ConfigScope.GameplayScene;

        public bool TryGetItemsSpawnConfig(LocationName locationName, out LocationItemsSpawnConfigMeta result)
        {
            foreach (LocationItemsSpawnConfigReference configReference in _spawnConfigsReferences)
            {
                bool isMatches = configReference.LocationName == locationName;

                if (!isMatches)
                    continue;

                result = configReference.Config;
                return true;
            }

            string log = Log.HandleLog($"Items Spawn Config for location <gb>{locationName}</gb> <rb>not found</rb>!");
            Debug.LogWarning(log);

            result = null;
            return false;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void CalculateThirdFloorChance() =>
            _thirdFloorChance = 100 - (_firstFloorChance + _secondFloorChance);

        // INNER CLASSES: -------------------------------------------------------------------------
        
        [Serializable]
        public class LocationItemsSpawnConfigReference
        {
            // MEMBERS: -------------------------------------------------------------------------------

            [SerializeField]
            private LocationName _locationName;

            [SerializeField, Required]
            private LocationItemsSpawnConfigMeta _config;

            // PROPERTIES: ----------------------------------------------------------------------------

            public LocationName LocationName => _locationName;
            public LocationItemsSpawnConfigMeta Config => _config;
        }
    }
}