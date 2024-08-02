using System;
using System.Collections.Generic;
using GameCore.Configs.Gameplay.Balance;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Configs.Gameplay.MonstersGenerator;
using GameCore.Configs.Gameplay.MonstersList;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Monsters;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.EntitiesSystems.Spawners;
using GameCore.Gameplay.Factories.Monsters;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.GameTimeManagement;
using GameCore.Gameplay.Level.Locations;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.RoundManagement;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using GameCore.Infrastructure.Providers.Gameplay.LocationsMeta;
using GameCore.Infrastructure.Providers.Gameplay.MonstersAI;
using GameCore.Utilities;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace GameCore.Gameplay.MonstersGeneration
{
    public interface IMonstersGenerator
    {
        void Start();
        void Stop();
    }

    public class MonstersGenerator : IMonstersGenerator, ITickable, IDisposable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public MonstersGenerator(
            IGameManagerDecorator gameManagerDecorator,
            IMonstersFactory monstersFactory,
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
            _monstersFactory = monstersFactory;
            _roundManager = roundManager;
            _timeCycle = timeCycle;
            _locationsMetaProvider = locationsMetaProvider;
            _monstersAIConfigsProvider = monstersAIConfigsProvider;
            _monstersGeneratorConfig = gameplayConfigsProvider.GetMonstersGeneratorConfig();
            _monstersListConfig = gameplayConfigsProvider.GetMonstersListConfig();
            _monstersDangerLevelConfig = balanceConfig.MonstersDangerLevelConfig;
            _validMonstersList = new List<ValidMonster>();
            _monstersSpawnChancesList = new List<MonsterSpawnChance>();
            _monstersSpawnList = new List<MonsterToSpawn>();

            _monstersSpawners = new Dictionary<Floor, List<DungeonMonstersSpawner>>
            {
                { Floor.One, new List<DungeonMonstersSpawner>() },
                { Floor.Two, new List<DungeonMonstersSpawner>() },
                { Floor.Three, new List<DungeonMonstersSpawner>() }
            };

            DungeonMonstersSpawner.OnRegisterMonstersSpawnerEvent += OnRegisterMonstersSpawner;

            _timeCycle.OnHourPassedEvent += OnHourPassed;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameManagerDecorator _gameManagerDecorator;
        private readonly IMonstersFactory _monstersFactory;
        private readonly IRoundManager _roundManager;
        private readonly ITimeCycle _timeCycle;
        private readonly ILocationsMetaProvider _locationsMetaProvider;
        private readonly IMonstersAIConfigsProvider _monstersAIConfigsProvider;
        private readonly MonstersGeneratorConfigMeta _monstersGeneratorConfig;
        private readonly MonstersListConfigMeta _monstersListConfig;
        private readonly MonstersDangerLevelConfig _monstersDangerLevelConfig;
        private readonly List<ValidMonster> _validMonstersList;
        private readonly List<MonsterSpawnChance> _monstersSpawnChancesList;
        private readonly List<MonsterToSpawn> _monstersSpawnList;
        private readonly Dictionary<Floor, List<DungeonMonstersSpawner>> _monstersSpawners;

        private int _monstersAmountToSpawn;
        private bool _isGeneratorEnabled;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Tick()
        {
            if (!_isGeneratorEnabled)
                return;

            TrySpawnMonster();
        }

        public void Dispose()
        {
            DungeonMonstersSpawner.OnRegisterMonstersSpawnerEvent -= OnRegisterMonstersSpawner;

            _timeCycle.OnHourPassedEvent -= OnHourPassed;
        }

        public void Start() =>
            _isGeneratorEnabled = true;

        public void Stop()
        {
            _isGeneratorEnabled = false;
            _monstersSpawnList.Clear();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        #region Main Methods

        private void GeneratorTick()
        {
            GetMonstersAmountToSpawn();

            for (int i = 0; i < _monstersAmountToSpawn; i++)
            {
                CreateValidMonstersList();
                ValidateMonstersSpawnList();
                CreateMonstersSpawnChancesList();
                AddRandomMonsterToSpawnList();
            }

            CleanUp();
        }

        private void CreateValidMonstersList()
        {
            _validMonstersList.Clear();

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

                ValidMonster validMonster = new(monsterAIConfig);
                _validMonstersList.Add(validMonster);
            }
        }

        private void ValidateMonstersSpawnList()
        {
            int monstersAmount = _validMonstersList.Count;

            if (monstersAmount == 0)
                return;

            for (int i = monstersAmount - 1; i >= 0; i--)
            {
                ValidMonster validMonster = _validMonstersList[i];
                MonsterAIConfigMeta monsterAIConfig = validMonster.MonsterAIConfig;
                MonsterType monsterType = validMonster.MonsterType;

                int maxCount = monsterAIConfig.MaxCount;
                int currentAmount = _roundManager.GetMonstersCount(monsterType);
                bool isAmountValid = currentAmount < maxCount;

                if (!isAmountValid)
                {
                    _validMonstersList.RemoveAt(i);
                    continue;
                }

                int currentTimeInMinutes = _timeCycle.GetCurrentTimeInMinutes();
                Vector2Int spawnTime = monsterAIConfig.SpawnTime;
                bool isTimeValid = currentTimeInMinutes >= spawnTime.x && currentTimeInMinutes <= spawnTime.y;

                if (!isTimeValid)
                {
                    _validMonstersList.RemoveAt(i);
                    continue;
                }

                MonsterDangerLevel dangerLevel = monsterAIConfig.DangerLevel;
                MonsterSpawnType spawnType = validMonster.SpawnType;

                int monsterDangerValue = _monstersDangerLevelConfig.GetDangerValue(dangerLevel);
                int locationCurrentDangerLevel = GetCurrentDangerValue(spawnType);
                int possibleLocationDangerValue = monsterDangerValue + locationCurrentDangerLevel;

                int maxDangerValue = GetLocationMaxDangerValue(spawnType);
                bool isLocationDangerValueValid = possibleLocationDangerValue < maxDangerValue;

                if (isLocationDangerValueValid)
                    continue;

                _validMonstersList.RemoveAt(i);
            }
        }

        private void CreateMonstersSpawnChancesList()
        {
            _monstersSpawnChancesList.Clear();

            foreach (ValidMonster monsterToSpawn in _validMonstersList)
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

        private void AddRandomMonsterToSpawnList()
        {
            int monstersAmountToSpawn = _monstersSpawnChancesList.Count;

            if (monstersAmountToSpawn == 0)
                return;

            var chances = new double[monstersAmountToSpawn];

            for (int i = 0; i < monstersAmountToSpawn; i++)
                chances[i] = _monstersSpawnChancesList[i].Chance;

            int randomIndex = GlobalUtilities.GetRandomIndex(chances);
            MonsterType monsterType = _monstersSpawnChancesList[randomIndex].MonsterType;
            float spawnDelay = GetMonsterSpawnDelay();

            MonsterToSpawn monsterToSpawn = new(monsterType, spawnDelay);
            _monstersSpawnList.Add(monsterToSpawn);
        }

        private void TrySpawnMonster()
        {
            int monstersSpawnAmount = _monstersSpawnList.Count;
            float deltaTime = Time.deltaTime;

            for (int i = monstersSpawnAmount - 1; i >= 0; i--)
            {
                MonsterToSpawn monsterToSpawn = _monstersSpawnList[i];
                monsterToSpawn.Tick(deltaTime);

                bool canSpawn = monsterToSpawn.CanSpawn();

                if (!canSpawn)
                    continue;

                MonsterType monsterType = monsterToSpawn.MonsterType;

                _monstersSpawnList.RemoveAt(i);
                SpawnMonsterIndoor(monsterType);
            }
        }

        private void SpawnMonsterIndoor(MonsterType monsterType)
        {
            Floor floor = GetRandomFloor(monsterType);
            bool isSpawnPositionFound = TryGetIndoorMonsterSpawnPosition(floor, out Vector3 spawnPosition);

            if (!isSpawnPositionFound)
            {
                string log = Log.HandleLog("Monster spawn position <rb>not found</rb>!");
                Debug.LogError(log);
                return;
            }

            bool isMonsterSpawned = _monstersFactory.SpawnMonster(monsterType, spawnPosition, Quaternion.identity,
                out MonsterEntityBase monsterEntity);

            if (!isMonsterSpawned)
                return;
            
            monsterEntity.SetEntityLocation(EntityLocation.Dungeon);
            monsterEntity.SetFloor(floor);
        }

        private void SpawnMonsterOutdoor(MonsterType monsterType)
        {
            Floor floor = GetRandomFloor(monsterType);
            bool isSpawnPositionFound = TryGetIndoorMonsterSpawnPosition(floor, out Vector3 spawnPosition);

            if (!isSpawnPositionFound)
            {
                string log = Log.HandleLog("Monster spawn position <rb>not found</rb>!");
                Debug.LogError(log);
                return;
            }

            _monstersFactory.SpawnMonster(monsterType, spawnPosition, Quaternion.identity, out _);
        }

        private void CleanUp()
        {
            _validMonstersList.Clear();
            _monstersSpawnChancesList.Clear();
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
            locationMeta = null;
            
            if (currentLocation == LocationName.Base)
                return false;
            
            bool isLocationMetaFound = _locationsMetaProvider.TryGetLocationMeta(currentLocation, out locationMeta);

            if (isLocationMetaFound)
                return true;

            Log.PrintError(log: $"Location Meta <gb>{currentLocation}</gb> <rb>not found</rb>!");
            return false;
        }

        private bool TryGetIndoorMonsterSpawnPosition(Floor floor, out Vector3 spawnPosition)
        {
            spawnPosition = Vector3.zero;

            bool isMonstersSpawnerFound =
                TryGetRandomMonstersSpawner(floor, out DungeonMonstersSpawner monstersSpawner);

            if (!isMonstersSpawnerFound)
                return false;

            spawnPosition = monstersSpawner.GetRandomSpawnWorldPosition();
            return true;
        }

        private bool TryGetRandomMonstersSpawner(Floor floor, out DungeonMonstersSpawner monstersSpawner)
        {
            List<DungeonMonstersSpawner> spawnersList = _monstersSpawners[floor];
            int spawnersCount = spawnersList.Count;
            monstersSpawner = null;

            if (spawnersCount == 0)
                return false;

            int randomIndex = Random.Range(0, spawnersCount);
            monstersSpawner = spawnersList[randomIndex];

            return true;
        }

        private Floor GetRandomFloor(MonsterType monsterType)
        {
            bool isMonsterConfigFound =
                _monstersAIConfigsProvider.TryGetMonsterAIConfig(monsterType, out MonsterAIConfigMeta monsterAIConfig);

            if (!isMonsterConfigFound)
                return Floor.Three;

            float firstFloorSpawnChance = 0.33f;
            float secondFloorSpawnChance = 0.33f;
            float thirdFloorSpawnChance = 0.33f;

            float firstFloorMultiplier = monsterAIConfig.GetFloorSpawnChanceMultiplier(Floor.One);
            float secondFloorMultiplier = monsterAIConfig.GetFloorSpawnChanceMultiplier(Floor.Two);
            float thirdFloorMultiplier = monsterAIConfig.GetFloorSpawnChanceMultiplier(Floor.Three);

            firstFloorSpawnChance *= firstFloorMultiplier;
            secondFloorSpawnChance *= secondFloorMultiplier;
            thirdFloorSpawnChance *= thirdFloorMultiplier;

            var chances = new double[]
            {
                firstFloorSpawnChance,
                secondFloorSpawnChance,
                thirdFloorSpawnChance
            };

            int randomIndex = GlobalUtilities.GetRandomIndex(chances);

            Floor floor = randomIndex switch
            {
                0 => Floor.One,
                1 => Floor.Two,
                2 => Floor.Three,
                _ => Floor.Three
            };

            return floor;
        }

        private float GetMonsterSpawnDelay()
        {
            float hourDurationInSeconds = _timeCycle.GetHourDurationInSeconds();
            float spawnDelay = Random.Range(0f, hourDurationInSeconds);
            return spawnDelay;
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

        private void OnRegisterMonstersSpawner(DungeonMonstersSpawner dungeonMonstersSpawner)
        {
            Floor floor = dungeonMonstersSpawner.Floor;
            _monstersSpawners[floor].Add(dungeonMonstersSpawner);
        }

        private void OnHourPassed()
        {
            if (!_isGeneratorEnabled)
                return;

            GeneratorTick();
        }

        // INNER CLASSES: -------------------------------------------------------------------------

        private class ValidMonster
        {
            // CONSTRUCTORS: --------------------------------------------------------------------------

            public ValidMonster(MonsterAIConfigMeta monsterAIConfig) =>
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
                MonsterType = monsterType;
                Chance = chance;
            }

            // PROPERTIES: ----------------------------------------------------------------------------

            public MonsterType MonsterType { get; }
            public double Chance { get; }
        }

        private class MonsterToSpawn
        {
            // CONSTRUCTORS: --------------------------------------------------------------------------

            public MonsterToSpawn(MonsterType monsterType, float timeLeft)
            {
                MonsterType = monsterType;
                _timeLeft = timeLeft;
            }

            // PROPERTIES: ----------------------------------------------------------------------------

            public MonsterType MonsterType { get; }

            // FIELDS: --------------------------------------------------------------------------------

            private float _timeLeft;

            // PUBLIC METHODS: ------------------------------------------------------------------------

            public void Tick(float deltaTime) =>
                _timeLeft -= deltaTime;

            public bool CanSpawn() =>
                _timeLeft <= 0.0f;
        }
    }
}