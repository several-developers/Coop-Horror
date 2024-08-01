using System;
using System.Collections.Generic;
using CustomEditors;
using GameCore.Enums.Gameplay;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Configs.Gameplay.Enemies
{
    public abstract class MonsterAIConfigMeta : EditorMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [TitleGroup(title: BaseSettings)]
        [BoxGroup(BaseSettingsGroup, showLabel: false), SerializeField]
        private MonsterDangerLevel _dangerLevel;

        [BoxGroup(BaseSettingsGroup), SerializeField]
        private bool _isDamageable = true;

        [BoxGroup(BaseSettingsGroup), SerializeField, Min(0f)]
        [ShowIf(nameof(_isDamageable))]
        private float _health = 100f;
        
        [TitleGroup(title: SpawnSettings)]
        [BoxGroup(SpawnSettingsGroup, showLabel: false), SerializeField]
        private MonsterSpawnType _spawnType;
        
        [BoxGroup(SpawnSettingsGroup), SerializeField, Min(1)]
        [HideIf(condition: nameof(_spawnType), optionalValue: MonsterSpawnType.NonSpawnable)]
        private int _maxCount = 1;

        [BoxGroup(SpawnSettingsGroup), SerializeField, Range(0, 100)]
        [HideIf(condition: nameof(_spawnType), optionalValue: MonsterSpawnType.NonSpawnable)]
        [SuffixLabel("%", overlay: true)]
        private float _spawnChance = 50;

        [BoxGroup(SpawnSettingsGroup), SerializeField]
        [HideIf(condition: nameof(_spawnType), optionalValue: MonsterSpawnType.NonSpawnable)]
        [LabelText("Multiplier by Game Time")]
        private AnimationCurve _spawnChanceMultiplierByGameTime = new(keys: DefaultCurve);
        
        [BoxGroup(SpawnSettingsGroup), SerializeField]
        [HideIf(condition: nameof(_spawnType), optionalValue: MonsterSpawnType.NonSpawnable)]
        [LabelText("Multiplier by Monsters Count")]
        private AnimationCurve _spawnChanceMultiplierByMonstersCount = new(keys: DefaultCurve);

        [BoxGroup(SpawnSettingsGroup), SerializeField]
        [HideIf(condition: nameof(_spawnType), optionalValue: MonsterSpawnType.NonSpawnable)]
        [MinMaxSlider(minValue: 0, maxValue: 1440, showFields: true)]
        [OnValueChanged(nameof(UpdateSpawnTimeText))]
        private Vector2Int _spawnTime = new(x: 0, y: 1440); // 1440 minutes in a day.

        [BoxGroup(SpawnSettingsGroup), SerializeField, ReadOnly]
        [HideIf(condition: nameof(_spawnType), optionalValue: MonsterSpawnType.NonSpawnable)]
        [LabelText("Converted Time")]
        private string _spawnTimeText;
        
        [BoxGroup(SpawnSettingsGroup), SerializeField, Space(height: 5)]
        [HideIf(condition: nameof(_spawnType), optionalValue: MonsterSpawnType.NonSpawnable)]
        private FloorsChancesMultipliersConfig _floorsChancesMultipliersConfig;

        // PROPERTIES: ----------------------------------------------------------------------------

        // Base Settings
        public MonsterDangerLevel DangerLevel => _dangerLevel;
        public bool IsDamageable => _isDamageable;
        public float Health => _health;
        
        // Spawn Settings
        public MonsterSpawnType SpawnType => _spawnType;
        public int MaxCount => _maxCount;
        public float SpawnChance => _spawnChance;
        public AnimationCurve SpawnChanceMultiplierByGameTime => _spawnChanceMultiplierByGameTime;
        public AnimationCurve SpawnChanceMultiplierByMonstersCount => _spawnChanceMultiplierByMonstersCount;
        public Vector2Int SpawnTime => _spawnTime;
        
        // FIELDS: --------------------------------------------------------------------------------
        
        private const string BaseSettings = "Base Settings";
        private const string SpawnSettings = "Spawn Settings";
        private const string BaseSettingsGroup = BaseSettings + "/In";
        private const string SpawnSettingsGroup = SpawnSettings + "/In";
        
        private static readonly Keyframe[] DefaultCurve = { new(time: 0f, value: 1f), new(time: 1f, value: 1f) };

        // PUBLIC METHODS: ------------------------------------------------------------------------

        protected override void OnEnable()
        {
            base.OnEnable();
            UpdateSpawnTimeText();
        }

        public abstract MonsterType GetMonsterType();
        
        public override string GetMetaCategory() =>
            EditorConstants.MonstersAICategory;

        public float GetFloorSpawnChanceMultiplier(Floor floor) =>
            _floorsChancesMultipliersConfig.GetMultiplier(floor);

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void UpdateSpawnTimeText()
        {
            float minHourF = _spawnTime.x / 60f;
            int minHour = Mathf.FloorToInt(minHourF);
            int minMinute = _spawnTime.x - minHour * 60;
            
            float maxHourF = _spawnTime.y / 60f;
            int maxHour = Mathf.FloorToInt(maxHourF);
            int maxMinute = _spawnTime.y - maxHour * 60;

            _spawnTimeText = $"{minHour:D2}:{minMinute:D2} - {maxHour:D2}:{maxMinute:D2}";
        }

        // INNER CLASSES: -------------------------------------------------------------------------

        [Serializable]
        public class FloorsChancesMultipliersConfig
        {
            // CONSTRUCTORS: --------------------------------------------------------------------------

            public FloorsChancesMultipliersConfig() =>
                _configs = new List<FloorChanceMultiplierConfig>();

            // MEMBERS: -------------------------------------------------------------------------------

            [SerializeField]
            [ListDrawerSettings(ListElementLabelName = "Label")]
            private List<FloorChanceMultiplierConfig> _configs;

            // PUBLIC METHODS: ------------------------------------------------------------------------

            public float GetMultiplier(Floor floor)
            {
                foreach (FloorChanceMultiplierConfig config in _configs)
                {
                    bool isMatches = config.Floor == floor;

                    if (isMatches)
                        return config.Multiplier;
                }

                return 1f;
            }
            
            // INNER CLASSES: -------------------------------------------------------------------------

            [Serializable]
            public class FloorChanceMultiplierConfig
            {
                // MEMBERS: -------------------------------------------------------------------------------

                [SerializeField]
                private Floor _floor = Floor.One;

                [SerializeField, Range(0f, 5f)]
                private float _multiplier = 1f;

                // PROPERTIES: ----------------------------------------------------------------------------

                public Floor Floor => _floor;
                public float Multiplier => _multiplier;
                
                private string Label => $"'Floor: {_floor}',   'Multiplier: {_multiplier}'";
            }
        }
    }
}