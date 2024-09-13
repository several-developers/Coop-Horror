using System;
using System.Collections.Generic;
using GameCore.Enums.Gameplay;
using GameCore.Infrastructure.Configs;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Configs.Gameplay.Enemies
{
    public abstract class MonsterAIConfigMeta : ConfigMeta
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

        [BoxGroup(SpawnSettingsGroup), SerializeField, Space(height: 5)]
        [HideIf(condition: nameof(_spawnType), optionalValue: MonsterSpawnType.NonSpawnable)]
        [Tooltip("Другие монстры, которых стоит учитывать при подсчёте кол-ва этого монстра в игре.")]
        private List<MonsterType> _relatedMonstersToCount = new();

        [BoxGroup(SpawnSettingsGroup), SerializeField]
        [HideIf(condition: nameof(_spawnType), optionalValue: MonsterSpawnType.NonSpawnable)]
        [ListDrawerSettings(ListElementLabelName = "Label")]
        private List<FloorChanceMultiplierConfig> _floorChanceMultiplierConfigs = new();
        
        [BoxGroup(SpawnSettingsGroup + "/Spawn Time", showLabel: false), SerializeField]
        [HideIf(condition: nameof(_spawnType), optionalValue: MonsterSpawnType.NonSpawnable)]
        private TimePeriods _spawnTimePeriods;

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
        public TimePeriods SpawnTimePeriods => _spawnTimePeriods;

        // FIELDS: --------------------------------------------------------------------------------

        protected const string ConfigTitle = "Config";
        
        private const string BaseSettings = "Base Settings";
        private const string SpawnSettings = "Spawn Settings";
        private const string BaseSettingsGroup = BaseSettings + "/In";
        private const string SpawnSettingsGroup = SpawnSettings + "/In";
        
        private static readonly Keyframe[] DefaultCurve = { new(time: 0f, value: 1f), new(time: 1f, value: 1f) };

        // PUBLIC METHODS: ------------------------------------------------------------------------

        protected override void OnEnable()
        {
            base.OnEnable();
            UpdateSpawnTimePeriodTexts();
        }

        public IEnumerable<MonsterType> GetRelatedMonstersToCount() => _relatedMonstersToCount;

        public override string GetMetaCategory() =>
            EditorConstants.MonstersAICategory;

        public abstract MonsterType GetMonsterType();

        public override ConfigScope GetConfigScope() =>
            ConfigScope.GameplayScene;

        public float GetFloorSpawnChanceMultiplier(Floor floor)
        {
            foreach (FloorChanceMultiplierConfig config in _floorChanceMultiplierConfigs)
            {
                bool isMatches = config.Floor == floor;

                if (isMatches)
                    return config.Multiplier;
            }

            return 1f;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void UpdateSpawnTimePeriodTexts() =>
            _spawnTimePeriods.UpdateTimeTexts();

        // INNER CLASSES: -------------------------------------------------------------------------

        [Serializable]
        private class FloorChanceMultiplierConfig
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