using System;
using System.Collections.Generic;
using CustomEditors;
using GameCore.Infrastructure.Configs.Gameplay.Lighting;
using GameCore.Enums.Gameplay;
using GameCore.Enums.Global;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameCore.Gameplay.Level.Locations
{
    public class LocationMeta : EditorMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private string _locationNameText = "location_name";

        [SerializeField]
        private LocationName _locationName;

        [SerializeField]
        private SceneName _sceneName;

        [SerializeField, Min(0)]
        private int _maxIndoorDangerValue;

        [SerializeField, Min(0)]
        private int _maxOutdoorDangerValue;

        [SerializeField, Space(height: 5)]
        private MonstersSpawnMultipliersConfig _monstersSpawnMultipliersConfig;

        [Title(Constants.References)]
        [SerializeField, Required]
        private AssetReference _sceneAsset;
        
        [SerializeField, Required]
        private LightingPresetMeta _lightingPreset;

        // PROPERTIES: ----------------------------------------------------------------------------

        public string LocationNameText => _locationNameText;
        public LocationName LocationName => _locationName;
        public SceneName SceneName => _sceneName;
        public int MaxIndoorDangerValue => _maxIndoorDangerValue;
        public int MaxOutdoorDangerValue => _maxOutdoorDangerValue;
        public AssetReference SceneAsset => _sceneAsset;
        public LightingPresetMeta LightingPreset => _lightingPreset;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public bool TryGetMonsterSpawnChanceMultiplier(MonsterType monsterType, out float spawnMultiplier)
        {
            spawnMultiplier = 1f;
            
            var allConfigs = _monstersSpawnMultipliersConfig.GetAllConfigs();

            foreach (var config in allConfigs)
            {
                bool isMatches = config.MonsterType == monsterType;

                if (!isMatches)
                    continue;

                spawnMultiplier = config.Multiplier;
                return true;
            }

            return false;
        }

        public override string GetMetaCategory() =>
            EditorConstants.LocationsCategory;

        // INNER CLASSES: -------------------------------------------------------------------------

        [Serializable]
        public class MonstersSpawnMultipliersConfig
        {
            // CONSTRUCTORS: --------------------------------------------------------------------------

            public MonstersSpawnMultipliersConfig() =>
                _configs = new List<MonsterSpawnMultiplierConfig>();

            // MEMBERS: -------------------------------------------------------------------------------

            [SerializeField]
            private List<MonsterSpawnMultiplierConfig> _configs;

            // PUBLIC METHODS: ------------------------------------------------------------------------

            public IEnumerable<MonsterSpawnMultiplierConfig> GetAllConfigs() => _configs;

            // INNER CLASSES: -------------------------------------------------------------------------

            [Serializable]
            public class MonsterSpawnMultiplierConfig
            {
                // MEMBERS: -------------------------------------------------------------------------------

                [SerializeField]
                private MonsterType _monsterType;

                [SerializeField, Range(0, 5)]
                private float _multiplier = 1f;

                // PROPERTIES: ----------------------------------------------------------------------------

                public MonsterType MonsterType => _monsterType;
                public float Multiplier => _multiplier;
            }
        }
    }
}