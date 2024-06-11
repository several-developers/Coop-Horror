using System;
using System.Collections.Generic;
using GameCore.Configs.Gameplay.ItemsSpawn;
using GameCore.Enums.Gameplay;
using GameCore.Enums.Global;
using GameCore.Gameplay.Factories.Items;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.Items.Spawners;
using GameCore.Gameplay.Quests;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using GameCore.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameCore.Gameplay.Items.SpawnSystem
{
    public class ItemsSpawnSystem : IItemsSpawnSystem, IDisposable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ItemsSpawnSystem(IQuestsManagerDecorator questsManagerDecorator,
            IGameManagerDecorator gameManagerDecorator,
            IItemsFactory itemsFactory,
            IGameplayConfigsProvider gameplayConfigsProvider)
        {
            _questsManagerDecorator = questsManagerDecorator;
            _gameManagerDecorator = gameManagerDecorator;
            _itemsFactory = itemsFactory;
            _itemsSpawnConfig = gameplayConfigsProvider.GetItemsSpawnConfig();

            _itemsSpawners = new Dictionary<Floor, List<DungeonItemsSpawner>>
            {
                { Floor.One, new List<DungeonItemsSpawner>() },
                { Floor.Two, new List<DungeonItemsSpawner>() },
                { Floor.Three, new List<DungeonItemsSpawner>() }
            };

            DungeonItemsSpawner.OnRegisterItemsSpawnerEvent += OnRegisterItemsSpawner;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IQuestsManagerDecorator _questsManagerDecorator;
        private readonly IGameManagerDecorator _gameManagerDecorator;
        private readonly IItemsFactory _itemsFactory;
        private readonly ItemsSpawnConfigMeta _itemsSpawnConfig;
        private readonly Dictionary<Floor, List<DungeonItemsSpawner>> _itemsSpawners;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Dispose() =>
            DungeonItemsSpawner.OnRegisterItemsSpawnerEvent -= OnRegisterItemsSpawner;

        public void SpawnItems()
        {
            SpawnQuestsItems();
            //SpawnQuestsItems();
            SpawnLocationItems();
            ClearItemsSpawners();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private bool TrySelectItemsSpawner(Floor floor, out DungeonItemsSpawner result)
        {
            while (_itemsSpawners[floor].Count > 0)
            {
                int randomIndex = Random.Range(0, _itemsSpawners[floor].Count);
                DungeonItemsSpawner itemsSpawner = _itemsSpawners[floor][randomIndex];
                bool canSpawnItem = itemsSpawner.CanSpawnItem();

                if (canSpawnItem)
                {
                    itemsSpawner.DecreaseAvailableItemsAmount();

                    result = itemsSpawner;
                    canSpawnItem = itemsSpawner.CanSpawnItem();

                    if (!canSpawnItem)
                        _itemsSpawners[floor].RemoveAt(randomIndex);

                    return true;
                }

                _itemsSpawners[floor].RemoveAt(randomIndex);
            }

            result = null;
            return false;
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
            {
                IReadOnlyDictionary<int, QuestItemData> questItemsData = questRuntimeData.GetQuestItems();

                foreach ((int itemID, QuestItemData questItemData) in questItemsData)
                {
                    int itemQuantity = questItemData.TargetItemQuantity;

                    for (int i = 0; i < itemQuantity; i++)
                    {
                        Floor floor = GetRandomFloor(firstFloorChance, secondFloorChance);
                        bool success = SpawnItem(itemID, floor);

                        if (success)
                            continue;

                        Log.PrintError(log: $"Item spawn error! Floor: {floor}");
                        return;
                    }
                }
            }
        }

        private void SpawnLocationItems()
        {
            SceneName selectedLocation = _gameManagerDecorator.GetSelectedLocation();

            bool isConfigFound = _itemsSpawnConfig.TryGetItemsSpawnConfig(selectedLocation,
                out LocationItemsSpawnConfigMeta itemsSpawnConfig);

            if (!isConfigFound)
                return;

            IReadOnlyList<ItemSpawnConfig> allConfigs = itemsSpawnConfig.GetAllConfigs();
            AnimationCurve itemsDistribution = _itemsSpawnConfig.ItemsDistribution;
            int configsAmount = allConfigs.Count;

            if (configsAmount == 0)
                return;

            double[] chances = new double[configsAmount];

            for (int i = 0; i < configsAmount; i++)
                chances[i] = allConfigs[i].SpawnChance;

            List<DungeonItemsSpawner> allItemsSpawners = new();

            foreach (List<DungeonItemsSpawner> itemsSpawners in _itemsSpawners.Values)
                allItemsSpawners.AddRange(itemsSpawners);

            int itemsSpawnersAmount = allItemsSpawners.Count;

            // Пробегаемся по всем спавнерам на всех этажах.
            for (int i = itemsSpawnersAmount - 1; i >= 0; i--)
            {
                DungeonItemsSpawner itemsSpawner = allItemsSpawners[i];
                bool canSpawnItem = itemsSpawner.CanSpawnItem();

                if (!canSpawnItem)
                {
                    allItemsSpawners.RemoveAt(i);
                    continue;
                }

                Floor spawnerFloor = itemsSpawner.Floor;
                float depth = itemsSpawner.Depth;
                int availableItemsAmount = itemsSpawner.AvailableItemsAmount;

                // Пробегаемся по кол-ву возможных предметов в комнате.
                for (int j = 0; j < availableItemsAmount; j++)
                {
                    canSpawnItem = itemsSpawner.CanSpawnItem();

                    if (!canSpawnItem)
                    {
                        allItemsSpawners.RemoveAt(i);
                        break;
                    }

                    float distributionValue = itemsDistribution.Evaluate(depth);
                    int spawnChance = Mathf.FloorToInt(f: distributionValue * 100f);
                    bool canItemSpawn = GlobalUtilities.IsRandomSuccessful(spawnChance);

                    // Смог ли выпасть шанс на спавн предмета.
                    if (!canItemSpawn)
                    {
                        itemsSpawner.DecreaseAvailableItemsAmount();
                        continue;
                    }

                    // Пытаемся найти предмет, который может появится на этом этаже.
                    while (true)
                    {
                        int randomIndex = GlobalUtilities.GetRandomIndex();
                        ItemSpawnConfig itemSpawnConfig = allConfigs[randomIndex];
                        int itemID = itemSpawnConfig.GetItemID();

                        int firstFloorChance = itemSpawnConfig.FirstFloorChance;
                        int secondFloorChance = itemSpawnConfig.SecondFloorChance;

                        Floor floor = GetRandomFloor(firstFloorChance, secondFloorChance);
                        bool isFloorMatches = spawnerFloor == floor;

                        if (!isFloorMatches)
                            continue;

                        Debug.Log($"Spawn: '{itemSpawnConfig.ItemMeta.ItemName}' with chance '{spawnChance}%'");
                        
                        SpawnItem(itemsSpawner, itemID);

                        break;
                    }
                }
            }
        }

        private bool SpawnItem(int itemID, Floor floor)
        {
            bool isItemsSpawnerFound = TrySelectItemsSpawner(floor, out DungeonItemsSpawner itemsSpawner);

            if (!isItemsSpawnerFound)
                return false;

            SpawnItem(itemsSpawner, itemID);
            return true;
        }

        private void SpawnItem(DungeonItemsSpawner itemsSpawner, int itemID)
        {
            Vector3 worldPosition = itemsSpawner.GetRandomSpawnWorldPosition();
            _itemsFactory.CreateItem(itemID, worldPosition, out _);
        }

        private void ClearItemsSpawners() =>
            _itemsSpawners.Clear();

        private static Floor GetRandomFloor(int firstFloorChance, int secondFloorChance)
        {
            bool isRandomSuccessful = GlobalUtilities.IsRandomSuccessful(firstFloorChance);

            if (isRandomSuccessful)
                return Floor.One;

            isRandomSuccessful = GlobalUtilities.IsRandomSuccessful(secondFloorChance);

            if (isRandomSuccessful)
                return Floor.Two;

            return Floor.Three;
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnRegisterItemsSpawner(DungeonItemsSpawner dungeonItemsSpawner)
        {
            Floor floor = dungeonItemsSpawner.Floor;
            _itemsSpawners[floor].Add(dungeonItemsSpawner);
        }
    }
}