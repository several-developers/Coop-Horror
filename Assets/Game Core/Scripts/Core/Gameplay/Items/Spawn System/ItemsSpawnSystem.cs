using System.Collections.Generic;
using GameCore.Configs.Gameplay.ItemsSpawn;
using GameCore.Enums.Gameplay;
using GameCore.Enums.Global;
using GameCore.Gameplay.Factories.Items;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.Quests;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using GameCore.Utilities;
using UnityEngine;

namespace GameCore.Gameplay.Items.SpawnSystem
{
    public class ItemsSpawnSystem : IItemsSpawnSystem
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
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IQuestsManagerDecorator _questsManagerDecorator;
        private readonly IGameManagerDecorator _gameManagerDecorator;
        private readonly IItemsFactory _itemsFactory;
        private readonly ItemsSpawnConfigMeta _itemsSpawnConfig;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SpawnItems()
        {
            SpawnQuestsItems();
            SpawnQuestsItems();
            SpawnLocationItems();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SpawnQuestsItems()
        {
            int activeQuestsAmount = _questsManagerDecorator.GetActiveQuestsAmount();

            if (activeQuestsAmount <= 0)
                return;

            QuestsStorage questsStorage = _questsManagerDecorator.GetQuestsStorage();
            IReadOnlyList<QuestRuntimeData> activeQuestsData = questsStorage.GetActiveQuestsData();

            foreach (QuestRuntimeData questRuntimeData in activeQuestsData)
            {
                IReadOnlyDictionary<int, QuestItemData> questItemsData = questRuntimeData.GetQuestItems();

                foreach ((int itemID, QuestItemData questItemData) in questItemsData)
                {
                    int itemQuantity = questItemData.TargetItemQuantity;

                    for (int i = 0; i < itemQuantity; i++)
                    {
                        Floor floor = GetRandomFloor();
                        SpawnItem(itemID, floor);
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
        }

        private void SpawnItem(int itemID, Floor floor)
        {
            Vector3 position = Vector3.zero;

            _itemsFactory.CreateItem(itemID, position, out _);
        }

        private Floor GetRandomFloor()
        {
            int firstFloorChance = _itemsSpawnConfig.FirstFloorChance;
            bool isRandomSuccessful = GlobalUtilities.IsRandomSuccessful(firstFloorChance);

            if (isRandomSuccessful)
                return Floor.One;

            int secondFloorChance = _itemsSpawnConfig.SecondFloorChance;
            isRandomSuccessful = GlobalUtilities.IsRandomSuccessful(secondFloorChance);

            if (isRandomSuccessful)
                return Floor.Two;

            return Floor.Three;
        }
    }
}