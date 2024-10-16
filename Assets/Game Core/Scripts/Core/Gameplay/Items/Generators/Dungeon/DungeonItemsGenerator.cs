﻿using System;
using System.Collections.Generic;
using GameCore.Infrastructure.Configs.Gameplay.ItemsSpawn;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Factories.Items;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.Items.Spawners;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Systems.Quests;
using GameCore.Gameplay.Utilities;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using GameCore.Observers.Gameplay.Dungeons;
using GameCore.Utilities;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace GameCore.Gameplay.Items.Generators.Dungeon
{
    public class DungeonItemsGenerator : IInitializable, IDisposable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public DungeonItemsGenerator(
            IQuestsManagerDecorator questsManagerDecorator,
            IGameManagerDecorator gameManagerDecorator,
            IItemsFactory itemsFactory,
            IDungeonsObserver dungeonsObserver,
            IGameplayConfigsProvider gameplayConfigsProvider
        )
        {
            if (!IsServer)
                return;

            _questsManagerDecorator = questsManagerDecorator;
            _gameManagerDecorator = gameManagerDecorator;
            _itemsFactory = itemsFactory;
            _dungeonsObserver = dungeonsObserver;
            _itemsSpawnConfig = gameplayConfigsProvider.GetConfig<ItemsSpawnConfigMeta>();
            _rawItemsSpawners = new List<DungeonItemsSpawner>();

            _itemsSpawners = new Dictionary<Floor, List<DungeonItemsSpawner>>
            {
                { Floor.One, new List<DungeonItemsSpawner>() },
                { Floor.Two, new List<DungeonItemsSpawner>() },
                { Floor.Three, new List<DungeonItemsSpawner>() }
            };
        }

        // PROPERTIES: ----------------------------------------------------------------------------
        
        private static bool IsServer => NetworkHorror.IsTrueServer;

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IQuestsManagerDecorator _questsManagerDecorator;
        private readonly IGameManagerDecorator _gameManagerDecorator;
        private readonly IItemsFactory _itemsFactory;
        private readonly IDungeonsObserver _dungeonsObserver;
        private readonly ItemsSpawnConfigMeta _itemsSpawnConfig;
        private readonly List<DungeonItemsSpawner> _rawItemsSpawners;
        private readonly Dictionary<Floor, List<DungeonItemsSpawner>> _itemsSpawners;

        private int _itemsToSpawn;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Initialize()
        {
            if (!IsServer)
                return;

            DungeonItemsSpawner.OnRegisterItemsSpawnerEvent += OnRegisterItemsSpawner;

            _dungeonsObserver.OnDungeonsGenerationCompletedEvent += OnDungeonsGenerationCompleted;
        }
        
        public void Dispose()
        {
            if (!IsServer)
                return;

            DungeonItemsSpawner.OnRegisterItemsSpawnerEvent -= OnRegisterItemsSpawner;

            _dungeonsObserver.OnDungeonsGenerationCompletedEvent -= OnDungeonsGenerationCompleted;
        }

        public void SpawnItems()
        {
            PrepareItemsSpawners();
            SpawnQuestsItems();
            SpawnLocationItems();
            ClearItemsSpawners();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void PrepareItemsSpawners()
        {
            AnimationCurve itemsDistribution = _itemsSpawnConfig.ItemsDistribution;
            _itemsToSpawn = 0;

            foreach (DungeonItemsSpawner itemsSpawner in _rawItemsSpawners)
            {
                bool canSpawnItem = itemsSpawner.CanSpawnItem();

                if (!canSpawnItem)
                    continue;

                float depth = itemsSpawner.Depth;
                int itemsSlotsAmount = itemsSpawner.ItemsSlotsAmount;
                bool addToDictionary = false;

                for (int i = 0; i < itemsSlotsAmount; i++)
                {
                    float distributionValue = itemsDistribution.Evaluate(depth);
                    int spawnChance = Mathf.FloorToInt(f: distributionValue * 100f);
                    bool spawnItem = GlobalUtilities.IsRandomSuccessful(spawnChance);

                    if (!spawnItem)
                        continue;

                    _itemsToSpawn += 1;
                    addToDictionary = true;

                    itemsSpawner.IncreaseItemsAmountToSpawn();
                }

                if (addToDictionary)
                {
                    Floor floor = itemsSpawner.Floor;
                    _itemsSpawners[floor].Add(itemsSpawner);
                }

                itemsSpawner.ClearItemsSlotsAmount();
            }
        }

        private void SpawnQuestsItems()
        {
            int activeQuestsAmount = _questsManagerDecorator.GetActiveQuestsAmount();

            if (activeQuestsAmount <= 0)
                return;

            QuestsStorage questsStorage = _questsManagerDecorator.GetQuestsStorage();
            IReadOnlyList<QuestRuntimeData> activeQuestsData = questsStorage.GetActiveQuestsData();

            int firstFloorChance = _itemsSpawnConfig.FirstFloorChance;
            int secondFloorChance = _itemsSpawnConfig.SecondFloorChance;

            foreach (QuestRuntimeData questRuntimeData in activeQuestsData)
                CheckQuestRuntimeData(questRuntimeData);

            // LOCAL METHODS: -----------------------------

            void CheckQuestRuntimeData(QuestRuntimeData questRuntimeData)
            {
                IReadOnlyDictionary<int, QuestItemData> questItemsData = questRuntimeData.GetQuestItems();

                foreach ((int itemID, QuestItemData questItemData) in questItemsData)
                {
                    int itemQuantity = questItemData.TargetItemQuantity;

                    for (int i = 0; i < itemQuantity; i++)
                    {
                        int firstFloorSpawnersAmount = _itemsSpawners[Floor.One].Count;
                        int secondFloorSpawnersAmount = _itemsSpawners[Floor.Two].Count;
                        int thirdFloorSpawnersAmount = _itemsSpawners[Floor.Three].Count;

                        bool isSpawnerFound = GetRandomFloor(
                            firstFloorChance,
                            secondFloorChance,
                            firstFloorSpawnersAmount,
                            secondFloorSpawnersAmount,
                            thirdFloorSpawnersAmount,
                            out Floor floor
                        );

                        if (!isSpawnerFound)
                            return;

                        DungeonItemsSpawner itemsSpawner = GetRandomItemsSpawner(floor);
                        SpawnItem(itemsSpawner, itemID);

                        _itemsToSpawn = Mathf.Max(a: _itemsToSpawn - 1, b: 0);
                    }
                }
            }
        }

        private void SpawnLocationItems()
        {
            LocationName selectedLocation = _gameManagerDecorator.GetCurrentLocation();

            bool isConfigFound = _itemsSpawnConfig.TryGetItemsSpawnConfig(selectedLocation,
                out LocationItemsSpawnConfigMeta itemsSpawnConfig);

            if (!isConfigFound)
                return;

            IReadOnlyList<ItemSpawnConfig> allConfigs = itemsSpawnConfig.GetAllConfigs();
            int itemsTotalAmount = allConfigs.Count;

            for (int i = 0; i < _itemsToSpawn; i++)
            {
#warning ПЕРЕДЕЛАТЬ
                // ПЕРЕДЕЛАТЬ, ВРЕМЕННО
                int randomItemIndex = Random.Range(0, itemsTotalAmount); // REPLACE FOR PERCENTAGE
                ItemSpawnConfig itemSpawnConfig = allConfigs[randomItemIndex];

                int firstFloorChance = itemSpawnConfig.FirstFloorChance;
                int secondFloorChance = itemSpawnConfig.SecondFloorChance;

                int firstFloorSpawnersAmount = _itemsSpawners[Floor.One].Count;
                int secondFloorSpawnersAmount = _itemsSpawners[Floor.Two].Count;
                int thirdFloorSpawnersAmount = _itemsSpawners[Floor.Three].Count;

                bool isSpawnerFound = GetRandomFloor(firstFloorChance, secondFloorChance, firstFloorSpawnersAmount,
                    secondFloorSpawnersAmount, thirdFloorSpawnersAmount, out Floor floor);

                if (!isSpawnerFound)
                    break;

                DungeonItemsSpawner itemsSpawner = GetRandomItemsSpawner(floor);
                int itemID = itemSpawnConfig.GetItemID();

                SpawnItem(itemsSpawner, itemID);
            }
        }

        private void SpawnItem(DungeonItemsSpawner itemsSpawner, int itemID)
        {
            Vector3 worldPosition = itemsSpawner.GetRandomSpawnWorldPosition();
            worldPosition.y += 1f;
            
            var spawnParams = new SpawnParams<ItemObjectBase>.Builder()
                .SetSpawnPosition(worldPosition)
                .Build();

            _itemsFactory.CreateItemDynamic(itemID, spawnParams);
        }

        private void ClearItemsSpawners()
        {
            _rawItemsSpawners.Clear();
            _itemsSpawners[Floor.One].Clear();
            _itemsSpawners[Floor.Two].Clear();
            _itemsSpawners[Floor.Three].Clear();
        }

        private DungeonItemsSpawner GetRandomItemsSpawner(Floor floor)
        {
            int spawnersAmount = _itemsSpawners[floor].Count;
            int randomIndex = Random.Range(0, spawnersAmount);
            DungeonItemsSpawner itemsSpawner = _itemsSpawners[floor][randomIndex];

            itemsSpawner.DecreaseItemsAmountToSpawn();
            int itemsAmountToSpawn = itemsSpawner.ItemsAmountToSpawn;
            bool canSpawnItems = itemsAmountToSpawn > 0;

            // Удаляем спавнер в котором уже нельзя создавать предметы.
            if (!canSpawnItems)
                _itemsSpawners[floor].RemoveAt(randomIndex);

            return itemsSpawner;
        }

        private static bool GetRandomFloor(int firstFloorChance, int secondFloorChance, int firstFloorSpawnersAmount,
            int secondFloorSpawnersAmount, int thirdFloorSpawnersAmount, out Floor floor)
        {
            if (thirdFloorSpawnersAmount == 0)
            {
                if (firstFloorSpawnersAmount > 0 && secondFloorSpawnersAmount > 0)
                {
                    if (firstFloorChance > 0 && secondFloorChance > 0)
                        floor = IsRandomSuccessful(secondFloorChance) ? Floor.Two : Floor.One;
                    else if (secondFloorChance > 0)
                        floor = Floor.Two;
                    else if (firstFloorChance > 0)
                        floor = Floor.One;
                    else
                        floor = Random.Range(0, 2) == 0 ? Floor.Two : Floor.One;
                }
                else if (secondFloorSpawnersAmount > 0)
                    floor = Floor.Two;
                else if (firstFloorSpawnersAmount > 0)
                    floor = Floor.One;
                else
                {
                    Log.PrintError(log: $"No more spawners left!");

                    floor = Floor.Three;
                    return false;
                }
            }
            else
            {
                bool isRandomSuccessful = firstFloorSpawnersAmount > 0 && IsRandomSuccessful(firstFloorChance);

                if (isRandomSuccessful)
                {
                    floor = Floor.One;
                    return true;
                }

                isRandomSuccessful = secondFloorSpawnersAmount > 0 && IsRandomSuccessful(secondFloorChance);

                if (isRandomSuccessful)
                {
                    floor = Floor.Two;
                    return true;
                }

                floor = Floor.Three;
            }

            return true;
        }

        private static bool IsRandomSuccessful(int chance) =>
            GlobalUtilities.IsRandomSuccessful(chance);

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnRegisterItemsSpawner(DungeonItemsSpawner dungeonItemsSpawner) =>
            _rawItemsSpawners.Add(dungeonItemsSpawner);

        private void OnDungeonsGenerationCompleted() => SpawnItems();
    }
}