using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Configs.Gameplay.ItemsSpawn;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Interactable.Outdoor_Chest;
using GameCore.Gameplay.Factories.Entities;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.Items.Generators.Dungeon;
using GameCore.Gameplay.Level.Locations;
using GameCore.Gameplay.Level.LocationsMechanics;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Systems.Quests;
using GameCore.Gameplay.Utilities;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using GameCore.Observers.Gameplay.Dungeons;
using GameCore.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameCore.Gameplay.Items.Generators.OutdoorChest
{
    public class OutdoorChestItemsGenerator : IDisposable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public OutdoorChestItemsGenerator(
            IQuestsManagerDecorator questsManagerDecorator,
            IGameManagerDecorator gameManagerDecorator,
            IEntitiesFactory entitiesFactory,
            IDungeonsObserver dungeonsObserver,
            IGameplayConfigsProvider gameplayConfigsProvider
        )
        {
            if (!NetworkHorror.IsTrueServer)
                return;

            _questsManagerDecorator = questsManagerDecorator;
            _gameManagerDecorator = gameManagerDecorator;
            _entitiesFactory = entitiesFactory;
            _dungeonsObserver = dungeonsObserver;
            _itemsSpawnConfig = gameplayConfigsProvider.GetConfig<ItemsSpawnConfigMeta>();
            _outdoorChestsItemsSpawnConfig = _itemsSpawnConfig.GetOutdoorChestsItemsSpawnConfig();

            _dungeonsObserver.OnDungeonsGenerationCompletedEvent += OnDungeonsGenerationCompleted;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private const float SpawnOffsetY = 1f;

        private readonly IQuestsManagerDecorator _questsManagerDecorator;
        private readonly IGameManagerDecorator _gameManagerDecorator;
        private readonly IEntitiesFactory _entitiesFactory;
        private readonly IDungeonsObserver _dungeonsObserver;
        private readonly ItemsSpawnConfigMeta _itemsSpawnConfig;
        private readonly OutdoorChestsItemsSpawnConfig _outdoorChestsItemsSpawnConfig;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Dispose()
        {
            if (!NetworkHorror.IsTrueServer)
                return;

            _dungeonsObserver.OnDungeonsGenerationCompletedEvent -= OnDungeonsGenerationCompleted;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async void SpawnChests()
        {
            int chestsAmount = GetRandomChestsAmount();

            if (chestsAmount == 0)
                return;

            bool wait = true;

            for (int i = 0; i < chestsAmount; i++)
            {
                bool isSpawnPointFound = TryGetChestSpawnPoint(out Vector3 worldPosition, out float radius);

                if (!isSpawnPointFound)
                {
                    string log = Log.HandleLog("<gb>Outdoor Chest Spawn Point</gb> <rb>not found</rb>!");
                    Debug.LogWarning(log);
                    return;
                }

                CastRayAndTrySpawnChest(worldPosition, radius);

                if (!wait)
                    continue;

                wait = false;
#warning DELETE
                await UniTask.Delay(millisecondsDelay: 500);
            }
        }

        private void CastRayAndTrySpawnChest(Vector3 worldPosition, float radius)
        {
            Vector2 randomInsideCircle = Random.insideUnitCircle * radius;
            Vector3 origin = worldPosition + new Vector3(x: randomInsideCircle.x, y: 0f, z: randomInsideCircle.y);

            bool rayHit = Physics.Raycast(origin, direction: Vector3.down, out RaycastHit hitInfo,
                maxDistance: Mathf.Infinity);

            if (!rayHit)
            {
                string log = Log.HandleLog("Ray <rb>doesn't hit the terrain</rb>!");
                Debug.LogWarning(log);
                return;
            }

            Vector3 spawnPoint = hitInfo.point;
            spawnPoint.y += SpawnOffsetY;

            TrySpawnChest(spawnPoint);
        }

        private void TrySpawnChest(Vector3 worldPosition)
        {
            int itemsAmount = GetItemsRandomAmount();

            if (itemsAmount == 0)
                return;

            bool isItemsListCreated = TryCreateItemsSpawnList(itemsAmount, out List<int> itemsList);

            if (!isItemsListCreated)
                return;

            var spawnParams = new EntitySpawnParams<OutdoorChestEntity>.Builder()
                .SetSpawnPosition(worldPosition)
                .SetSuccessCallback(entity => { ChestSpawned(entity, itemsList); })
                .Build();

            _entitiesFactory.CreateEntity(spawnParams);
        }

        private static void ChestSpawned(OutdoorChestEntity outdoorChestEntity, List<int> itemsList) =>
            outdoorChestEntity.SetupItemsList(itemsList);

        private int GetRandomChestsAmount() =>
            _outdoorChestsItemsSpawnConfig.GetRandomChestsAmount();

        private int GetItemsRandomAmount() =>
            _outdoorChestsItemsSpawnConfig.GetItemsRandomAmount();

        private static bool TryGetChestSpawnPoint(out Vector3 spawnPoint, out float radius)
        {
            spawnPoint = Vector3.zero;
            radius = 0f;

            LocationManager locationManager = LocationManager.Get();

            if (locationManager == null)
                return false;

            IEnumerable<LocationMechanic> allLocationMechanics = locationManager.GetAllLocationMechanics();

            foreach (LocationMechanic locationMechanic in allLocationMechanics)
            {
                if (locationMechanic is not ChestsSpawnPointsStorage chestsSpawnPointsStorage)
                    continue;

                bool isSpawnPointFound = chestsSpawnPointsStorage.TryGetRandomSpawnPoint(out spawnPoint);

                if (!isSpawnPointFound)
                    continue;

                radius = chestsSpawnPointsStorage.SpawnPointRadius;
                return true;
            }

            return false;
        }

        private bool TryCreateItemsSpawnList(int itemsAmount, out List<int> itemsList)
        {
            itemsList = null;

            if (itemsAmount == 0)
                return false;

            LocationName selectedLocation = _gameManagerDecorator.GetCurrentLocation();

            bool isItemsSpawnConfigFound = _itemsSpawnConfig.TryGetItemsSpawnConfig(selectedLocation,
                out LocationItemsSpawnConfigMeta itemsSpawnConfig);

            if (!isItemsSpawnConfigFound)
                return false;

            IReadOnlyList<ItemSpawnConfig> allConfigs = itemsSpawnConfig.GetAllConfigs();
            int itemsTotalAmount = allConfigs.Count;
            int questsItemsSpawnChance = _outdoorChestsItemsSpawnConfig.QuestsItemsSpawnChance;

            itemsList = new List<int>();

            for (int i = 0; i < itemsAmount; i++)
            {
                if (!TryGetQuestItemID(out int itemID))
                    itemID = GetRandomLocationItemID();

                itemsList.Add(itemID);
            }

            return itemsList.Count != 0;

            // LOCAL METHODS: -----------------------------

            bool TryGetQuestItemID(out int result)
            {
                result = -1;

                bool canGetQuestItem = GlobalUtilities.IsRandomSuccessful(questsItemsSpawnChance);

                if (!canGetQuestItem)
                    return false;

                return TryGetRandomQuestItemID(out result);
            }

            int GetRandomLocationItemID()
            {
#warning ПЕРЕДЕЛАТЬ
                // ПЕРЕДЕЛАТЬ, ВРЕМЕННО
                int randomItemIndex = Random.Range(0, itemsTotalAmount); // REPLACE FOR PERCENTAGE
                ItemSpawnConfig itemSpawnConfig = allConfigs[randomItemIndex];

                return itemSpawnConfig.GetItemID();
            }
        }

        private bool TryGetRandomQuestItemID(out int itemID)
        {
            itemID = -1;

            List<int> allQuestsItemsList = new();

            QuestsStorage questsStorage = _questsManagerDecorator.GetQuestsStorage();
            IReadOnlyList<QuestRuntimeData> activeQuestsData = questsStorage.GetActiveQuestsData();

            foreach (QuestRuntimeData questRuntimeData in activeQuestsData)
            {
                IEnumerable<int> questsItemsID = questRuntimeData.GetQuestsItemsID();
                allQuestsItemsList.AddRange(questsItemsID);
            }

            int itemsAmount = allQuestsItemsList.Count;

            if (itemsAmount == 0)
                return false;

            int randomIndex = Random.Range(0, itemsAmount);
            itemID = allQuestsItemsList[randomIndex];

            return true;
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnDungeonsGenerationCompleted() => SpawnChests();
    }
}