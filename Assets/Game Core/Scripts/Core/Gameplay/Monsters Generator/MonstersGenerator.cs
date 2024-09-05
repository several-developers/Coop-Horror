using System;
using System.Collections.Generic;
using System.Threading;
using GameCore.Configs.Gameplay.Balance;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Configs.Gameplay.MonstersGenerator;
using GameCore.Configs.Global.MonstersList;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Monsters;
using GameCore.Gameplay.Factories.Monsters;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.Level.Locations;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.RoundManagement;
using GameCore.Gameplay.Systems.Spawners;
using GameCore.Gameplay.Utilities;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using GameCore.Infrastructure.Providers.Gameplay.LocationsMeta;
using GameCore.Infrastructure.Providers.Gameplay.MonstersAI;
using GameCore.Infrastructure.Providers.Global;
using GameCore.Observers.Gameplay.Time;
using GameCore.Utilities;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace GameCore.Gameplay.MonstersGeneration
{
    public class MonstersGenerator : IMonstersGenerator, IInitializable, IDisposable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public MonstersGenerator(
            IGameManagerDecorator gameManagerDecorator,
            IConfigsProvider configsProvider,
            IMonstersFactory monstersFactory,
            IRoundManager roundManager,
            ITimeObserver timeObserver,
            ILocationsMetaProvider locationsMetaProvider,
            IMonstersAIConfigsProvider monstersAIConfigsProvider,
            IGameplayConfigsProvider gameplayConfigsProvider
        )
        {
            if (!NetworkHorror.IsTrueServer)
                return;

            var balanceConfig = gameplayConfigsProvider.GetConfig<BalanceConfigMeta>();

            _gameManagerDecorator = gameManagerDecorator;
            _monstersFactory = monstersFactory;
            _roundManager = roundManager;
            _timeObserver = timeObserver;
            _locationsMetaProvider = locationsMetaProvider;
            _monstersAIConfigsProvider = monstersAIConfigsProvider;
            _monstersGeneratorConfig = gameplayConfigsProvider.GetConfig<MonstersGeneratorConfigMeta>();
            _monstersListConfig = configsProvider.GetConfig<MonstersListConfigMeta>();
            _monstersDangerLevelConfig = balanceConfig.MonstersDangerLevelConfig;

            var cts = new CancellationTokenSource();
            _monstersSpawnCycle = new SimpleRoutine(cts, MonstersSpawnTickInterval);

            _validMonstersList = new List<ValidMonster>();
            _monstersSpawnChancesList = new List<MonsterSpawnChance>();
            _monstersSpawnList = new List<MonsterToSpawn>();

            _monstersSpawners = new Dictionary<Floor, List<DungeonMonstersSpawner>>
            {
                { Floor.One, new List<DungeonMonstersSpawner>() },
                { Floor.Two, new List<DungeonMonstersSpawner>() },
                { Floor.Three, new List<DungeonMonstersSpawner>() }
            };
        }

        // FIELDS: --------------------------------------------------------------------------------

        private const float MonstersSpawnTickInterval = 1f;

        private readonly IGameManagerDecorator _gameManagerDecorator;
        private readonly IMonstersFactory _monstersFactory;
        private readonly IRoundManager _roundManager;
        private readonly ITimeObserver _timeObserver;
        private readonly ILocationsMetaProvider _locationsMetaProvider;
        private readonly IMonstersAIConfigsProvider _monstersAIConfigsProvider;
        private readonly MonstersGeneratorConfigMeta _monstersGeneratorConfig;
        private readonly MonstersListConfigMeta _monstersListConfig;
        private readonly MonstersDangerLevelConfig _monstersDangerLevelConfig;

        private readonly SimpleRoutine _monstersSpawnCycle;

        private readonly List<ValidMonster> _validMonstersList;
        private readonly List<MonsterSpawnChance> _monstersSpawnChancesList;
        private readonly List<MonsterToSpawn> _monstersSpawnList;
        private readonly Dictionary<Floor, List<DungeonMonstersSpawner>> _monstersSpawners;

        private bool _isGeneratorEnabled;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Initialize()
        {
            if (!NetworkHorror.IsTrueServer)
                return;

            _monstersSpawnCycle.OnActionEvent += MonstersSpawnTick;

            DungeonMonstersSpawner.OnRegisterMonstersSpawnerEvent += OnRegisterMonstersSpawner;

            _timeObserver.OnHourPassedEvent += OnHourPassed;

            _monstersSpawnCycle.Start();
        }

        public void Dispose()
        {
            if (!NetworkHorror.IsTrueServer)
                return;

            _monstersSpawnCycle.FullStop();

            DungeonMonstersSpawner.OnRegisterMonstersSpawnerEvent -= OnRegisterMonstersSpawner;

            _timeObserver.OnHourPassedEvent -= OnHourPassed;
        }

        public void Start() =>
            _isGeneratorEnabled = true;

        public void Stop()
        {
            _isGeneratorEnabled = false;

            _monstersSpawnList.Clear();
            _monstersSpawners[Floor.One].Clear();
            _monstersSpawners[Floor.Two].Clear();
            _monstersSpawners[Floor.Three].Clear();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        #region Main Methods

        private void MonstersSpawnTick()
        {
            if (!_isGeneratorEnabled)
                return;

            TrySpawnMonster();
        }

        private void TryAddMonsterToSpawnList()
        {
            int monstersAmountToSpawn = GetMonstersAmountToSpawn();

            for (int i = 0; i < monstersAmountToSpawn; i++)
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

            IEnumerable<MonsterReference> allReferences = _monstersListConfig.GetAllReferences();

            foreach (MonsterReference monsterReference in allReferences)
            {
                MonsterAIConfigMeta monsterAIConfig = monsterReference.MonsterAIConfig;
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

                int maxCount = monsterAIConfig.MaxCount;
                int currentAmount = GetMonstersCurrentAmount(monsterAIConfig);
                bool isAmountValid = currentAmount < maxCount;

                if (!isAmountValid)
                {
                    _validMonstersList.RemoveAt(i);
                    continue;
                }

                int currentTimeInMinutes = _timeObserver.GetCurrentTimeInMinutes();
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
                int currentTimeInMinutes = _timeObserver.GetCurrentTimeInMinutes();
                float currentTimeNormalized = Mathf.Clamp01(currentTimeInMinutes / (float)minutesInDay);
                float gameTimeMultiplier = multiplierByGameTime.Evaluate(currentTimeNormalized);

                int maxMonstersCount = monsterAIConfig.MaxCount;
                int currentMonstersCount = GetMonstersCurrentAmount(monsterAIConfig);
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

            var spawnParams = new EntitySpawnParams<MonsterEntityBase>.Builder()
                .SetSpawnPosition(spawnPosition)
                .SetSuccessCallback(entity => { MonsterSpawned(entity, floor); })
                .Build();

            _monstersFactory.CreateMonster(monsterType, spawnParams);
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

            // _monstersFactory.SpawnMonster(monsterType, spawnPosition, Quaternion.identity, out _);
        }

        private static void MonsterSpawned(MonsterEntityBase monsterEntity, Floor floor)
        {
            MonsterType monsterType = monsterEntity.GetMonsterType();
            
            string monsterSpawnLog = Log.HandleLog($"Spawned Monster <gb>{monsterType.GetNiceName()}</gb>");
            Debug.LogWarning(monsterSpawnLog);

            monsterEntity.SetEntityLocation(EntityLocation.Dungeon);
            monsterEntity.SetFloor(floor);
        }

        private void CleanUp()
        {
            _validMonstersList.Clear();
            _monstersSpawnChancesList.Clear();
        }

        #endregion

        #region Helper Methods

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
            float hourDurationInSeconds = _timeObserver.GetHourDurationInSeconds();
            float spawnDelay = Random.Range(0f, hourDurationInSeconds);
            return spawnDelay;
        }

        private int GetMonstersAmountToSpawn()
        {
            int alivePlayersAmount = GetAlivePlayersAmount();
            int monstersAmountToSpawn = _monstersGeneratorConfig.GetRandomMonstersAmount(alivePlayersAmount);
            return monstersAmountToSpawn;
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

        private int GetMonstersCurrentAmount(MonsterAIConfigMeta monsterAIConfig)
        {
            MonsterType monsterType = monsterAIConfig.GetMonsterType();

            int spawnedAmount = GetSpawnedMonstersAmount();
            int relatedAmount = GetRelatedMonstersAmount();
            int amountInSpawnList = GetAmountInSpawnList();

            int currentAmount = spawnedAmount + relatedAmount + amountInSpawnList;
            return currentAmount;

            // LOCAL METHODS: -----------------------------

            int GetSpawnedMonstersAmount()
            {
                int amount = _roundManager.GetMonstersCount(monsterType);
                return amount;
            }

            int GetRelatedMonstersAmount()
            {
                IEnumerable<MonsterType> relatedMonstersToCount = monsterAIConfig.GetRelatedMonstersToCount();
                int amount = 0;

                foreach (MonsterType relatedMonsterType in relatedMonstersToCount)
                {
                    bool isAtLeastOneMonsterExists = _roundManager.IsAtLeastOneMonsterExists(relatedMonsterType);

                    if (!isAtLeastOneMonsterExists)
                        continue;

                    amount += 1;
                }

                return amount;
            }

            int GetAmountInSpawnList()
            {
                int amount = 0;

                foreach (MonsterToSpawn monsterToSpawn in _monstersSpawnList)
                {
                    bool isSameMonster = monsterToSpawn.MonsterType == monsterType;

                    if (!isSameMonster)
                        continue;

                    amount += 1;
                }

                return amount;
            }
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

            TryAddMonsterToSpawnList();
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