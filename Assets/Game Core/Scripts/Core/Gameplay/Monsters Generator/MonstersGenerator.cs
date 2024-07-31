using System;
using System.Collections.Generic;
using GameCore.Configs.Gameplay.Balance;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Configs.Gameplay.MonstersGenerator;
using GameCore.Configs.Gameplay.MonstersList;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.GameTimeManagement;
using GameCore.Gameplay.Level.Locations;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.RoundManagement;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using GameCore.Infrastructure.Providers.Gameplay.LocationsMeta;
using GameCore.Infrastructure.Providers.Gameplay.MonstersAI;
using UnityEngine;

namespace GameCore.Gameplay.MonstersGeneration
{
    public interface IMonstersGenerator
    {
        void Start();
        void Stop();
    }

    public class MonstersGenerator : IMonstersGenerator, IDisposable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public MonstersGenerator(
            IGameManagerDecorator gameManagerDecorator,
            IRoundManager roundManager,
            ITimeCycle timeCycle,
            ILocationsMetaProvider locationsMetaProvider,
            IMonstersAIConfigsProvider monstersAIConfigsProvider,
            IGameplayConfigsProvider gameplayConfigsProvider
        )
        {
            if (!NetworkHorror.IsTrueServer)
                return;

            BalanceConfigMeta balanceConfig = gameplayConfigsProvider.GetBalanceConfig();

            _gameManagerDecorator = gameManagerDecorator;
            _roundManager = roundManager;
            _timeCycle = timeCycle;
            _locationsMetaProvider = locationsMetaProvider;
            _monstersAIConfigsProvider = monstersAIConfigsProvider;
            _monstersGeneratorConfig = gameplayConfigsProvider.GetMonstersGeneratorConfig();
            _monstersListConfig = gameplayConfigsProvider.GetMonstersListConfig();
            _monstersDangerLevelConfig = balanceConfig.MonstersDangerLevelConfig;
            _monstersSpawnList = new List<MonsterToSpawn>();
            _monstersSpawnChancesList = new List<MonsterSpawnChance>();

            _timeCycle.OnHourPassedEvent += OnHourPassed;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameManagerDecorator _gameManagerDecorator;
        private readonly IRoundManager _roundManager;
        private readonly ITimeCycle _timeCycle;
        private readonly ILocationsMetaProvider _locationsMetaProvider;
        private readonly IMonstersAIConfigsProvider _monstersAIConfigsProvider;
        private readonly MonstersGeneratorConfigMeta _monstersGeneratorConfig;
        private readonly MonstersListConfigMeta _monstersListConfig;
        private readonly MonstersDangerLevelConfig _monstersDangerLevelConfig;
        private readonly List<MonsterToSpawn> _monstersSpawnList;
        private readonly List<MonsterSpawnChance> _monstersSpawnChancesList;

        private int _monstersAmountToSpawn;
        private bool _isGeneratorEnabled;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Dispose() =>
            _timeCycle.OnHourPassedEvent -= OnHourPassed;

        public void Start() =>
            _isGeneratorEnabled = true;

        // TO DO: Cancel all spawn delays
        public void Stop() =>
            _isGeneratorEnabled = false;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        #region Main Methods

        private void GeneratorTick()
        {
            GetMonstersAmountToSpawn();
            CreateMonstersSpawnList();
            ValidateMonstersSpawnList();
            CreateMonstersSpawnChancesList();

            //LogMonstersSpawnList();

            return;

            TrySpawnIndoorMonsters();
            TrySpawnOutdoorMonsters();

            void LogMonstersSpawnList()
            {
                foreach (MonsterToSpawn monsterToSpawn in _monstersSpawnList)
                {
                    MonsterType monsterType = monsterToSpawn.MonsterType;

                    string log = Log.HandleLog($"Monster to spawn: <gb>{monsterType}</gb>");
                    Debug.Log(log);
                }
            }
        }

        private void CreateMonstersSpawnList()
        {
            _monstersSpawnList.Clear();

            IReadOnlyList<MonsterReference> allReferences = _monstersListConfig.GetAllReferences();

            foreach (MonsterReference monsterReference in allReferences)
            {
                MonsterType monsterType = monsterReference.MonsterType;

                bool isMonsterAIConfigFound = _monstersAIConfigsProvider
                    .TryGetMonsterAIConfig(monsterType, out MonsterAIConfigMeta monsterAIConfig);

                if (!isMonsterAIConfigFound)
                    continue;

                MonsterSpawnType spawnType = monsterAIConfig.SpawnType;

                if (spawnType == MonsterSpawnType.NonSpawnable)
                    continue;

                MonsterToSpawn monsterToSpawn = new(monsterAIConfig);
                _monstersSpawnList.Add(monsterToSpawn);
            }
        }

        private void ValidateMonstersSpawnList()
        {
            int monstersAmount = _monstersSpawnList.Count;

            if (monstersAmount == 0)
                return;

            for (int i = monstersAmount - 1; i >= 0; i--)
            {
                MonsterToSpawn monsterToSpawn = _monstersSpawnList[i];
                MonsterAIConfigMeta monsterAIConfig = monsterToSpawn.MonsterAIConfig;
                MonsterType monsterType = monsterToSpawn.MonsterType;

                int maxCount = monsterAIConfig.MaxCount;
                int currentAmount = _roundManager.GetMonstersCount(monsterType);
                bool isAmountValid = currentAmount < maxCount;

                if (!isAmountValid)
                {
                    _monstersSpawnList.RemoveAt(i);
                    continue;
                }

                int currentTimeInMinutes = _timeCycle.GetCurrentTimeInMinutes();
                Vector2Int spawnTime = monsterAIConfig.SpawnTime;
                bool isTimeValid = currentTimeInMinutes >= spawnTime.x && currentTimeInMinutes <= spawnTime.y;

                if (!isTimeValid)
                {
                    _monstersSpawnList.RemoveAt(i);
                    continue;
                }

                MonsterDangerLevel dangerLevel = monsterAIConfig.DangerLevel;
                MonsterSpawnType spawnType = monsterToSpawn.SpawnType;

                int monsterDangerValue = _monstersDangerLevelConfig.GetDangerValue(dangerLevel);
                int locationCurrentDangerLevel = GetCurrentDangerValue(spawnType);
                int possibleLocationDangerValue = monsterDangerValue + locationCurrentDangerLevel;

                int maxDangerValue = GetLocationMaxDangerValue(spawnType);
                bool isLocationDangerValueValid = possibleLocationDangerValue < maxDangerValue;

                if (isLocationDangerValueValid)
                    continue;

                _monstersSpawnList.RemoveAt(i);
            }
        }

        private void CreateMonstersSpawnChancesList()
        {
            _monstersSpawnChancesList.Clear();

            foreach (MonsterToSpawn monsterToSpawn in _monstersSpawnList)
            {
                MonsterAIConfigMeta monsterAIConfig = monsterToSpawn.MonsterAIConfig;
                MonsterType monsterType = monsterToSpawn.MonsterType;

                double spawnChance = monsterAIConfig.SpawnChance;
                AnimationCurve multiplierByGameTime = monsterAIConfig.SpawnChanceMultiplierByGameTime;
                AnimationCurve multiplierByMonstersCount = monsterAIConfig.SpawnChanceMultiplierByMonstersCount;

                const int minutesInDay = Constants.MinutesInDay;
                int currentTimeInMinutes = _timeCycle.GetCurrentTimeInMinutes();
                float currentTimeNormalized = Mathf.Clamp01(currentTimeInMinutes / (float)minutesInDay);
                float gameTimeMultiplier = multiplierByGameTime.Evaluate(currentTimeNormalized);

                int maxMonstersCount = monsterAIConfig.MaxCount;
                int currentMonstersCount = _roundManager.GetMonstersCount(monsterType);
                float currentMonstersCountNormalized = Mathf.Clamp01(currentMonstersCount / (float)maxMonstersCount);
                float monstersCountMultiplier = multiplierByMonstersCount.Evaluate(currentMonstersCountNormalized);

                float locationMultiplier = 1f;
                bool isLocationMetaFound = TryGetCurrentLocationMeta(out LocationMeta locationMeta);

                if (isLocationMetaFound)
                    locationMeta.TryGetMonsterSpawnChanceMultiplier(monsterType, out locationMultiplier);

                double finalSpawnChance = spawnChance *
                                          gameTimeMultiplier *
                                          monstersCountMultiplier *
                                          locationMultiplier;

                // string log = Log.HandleLog($"Spawn chance of <gb>{monsterType}</gb> = <gb>{finalSpawnChance:F1}%</gb>");
                // Debug.Log(log);

                MonsterSpawnChance monsterSpawnChance = new(monsterType, finalSpawnChance);
                _monstersSpawnChancesList.Add(monsterSpawnChance);
            }
        }

        private void TrySpawnIndoorMonsters() => TrySpawnMonster(MonsterSpawnType.Indoor);

        private void TrySpawnOutdoorMonsters() => TrySpawnMonster(MonsterSpawnType.Outdoor);

        private void TrySpawnMonster(MonsterSpawnType spawnType)
        {
            if (_monstersAmountToSpawn <= 0)
                return;

            int maxDangerLevel = GetLocationMaxDangerValue(spawnType);

            if (maxDangerLevel == 0)
                return;
        }

        #endregion

        #region Helper Methods

        private void GetMonstersAmountToSpawn()
        {
            MonstersSpawnAmountConfig monstersSpawnAmountConfig = _monstersGeneratorConfig.MonstersSpawnAmountConfig;
            int alivePlayersAmount = GetAlivePlayersAmount();
            _monstersAmountToSpawn = monstersSpawnAmountConfig.GetRandomMonstersAmount(alivePlayersAmount);
        }

        private bool TryGetCurrentLocationMeta(out LocationMeta locationMeta)
        {
            LocationName currentLocation = _gameManagerDecorator.GetCurrentLocation();
            bool isLocationMetaFound = _locationsMetaProvider.TryGetLocationMeta(currentLocation, out locationMeta);

            if (isLocationMetaFound)
                return true;

            Log.PrintError(log: $"Location Meta <gb>{currentLocation}</gb> <rb>not found</rb>!");
            return false;
        }

        private int GetAlivePlayersAmount() =>
            _roundManager.GetAlivePlayersAmount();

        private int GetLocationMaxDangerValue(MonsterSpawnType spawnType)
        {
            bool isLocationMetaFound = TryGetCurrentLocationMeta(out LocationMeta locationMeta);

            if (!isLocationMetaFound)
                return 0;

            int maxDangerValue = spawnType switch
            {
                MonsterSpawnType.Indoor => locationMeta.MaxIndoorDangerValue,
                MonsterSpawnType.Outdoor => locationMeta.MaxOutdoorDangerValue,
                _ => 0
            };

            if (maxDangerValue == 0)
                return 0;

            float locationDangerValueMultiplier = _roundManager.GetLocationDangerValueMultiplier();
            float newMaxDangerLevel = maxDangerValue * locationDangerValueMultiplier;
            int finalMaxDangerLevel = Mathf.RoundToInt(newMaxDangerLevel);

            return finalMaxDangerLevel;
        }

        private int GetCurrentDangerValue(MonsterSpawnType spawnType)
        {
            int currentDangerValue = 0;

            switch (spawnType)
            {
                case MonsterSpawnType.Indoor:
                    currentDangerValue = _roundManager.GetCurrentIndoorDangerValue();
                    break;

                case MonsterSpawnType.Outdoor:
                    currentDangerValue = _roundManager.GetCurrentOutdoorDangerValue();
                    break;

                case MonsterSpawnType.Both:
                    currentDangerValue = Mathf.Max(
                        a: GetCurrentDangerValue(MonsterSpawnType.Indoor),
                        b: GetCurrentDangerValue(MonsterSpawnType.Outdoor)
                    );
                    break;
            }

            return currentDangerValue;
        }

        #endregion

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnHourPassed()
        {
            if (!_isGeneratorEnabled)
                return;

            Debug.Log("Hour passed");
            GeneratorTick();
        }

        // INNER CLASSES: -------------------------------------------------------------------------

        private class MonsterToSpawn
        {
            // CONSTRUCTORS: --------------------------------------------------------------------------

            public MonsterToSpawn(MonsterAIConfigMeta monsterAIConfig) =>
                MonsterAIConfig = monsterAIConfig;

            // PROPERTIES: ----------------------------------------------------------------------------

            public MonsterAIConfigMeta MonsterAIConfig { get; }
            public MonsterType MonsterType => MonsterAIConfig.GetMonsterType();
            public MonsterSpawnType SpawnType => MonsterAIConfig.SpawnType;
        }

        private class MonsterSpawnChance
        {
            // CONSTRUCTORS: --------------------------------------------------------------------------

            public MonsterSpawnChance(MonsterType monsterType, double chance)
            {
                _monsterType = monsterType;
                _chance = chance;
            }

            // FIELDS: --------------------------------------------------------------------------------

            private readonly MonsterType _monsterType;
            private readonly double _chance;
        }
    }
}