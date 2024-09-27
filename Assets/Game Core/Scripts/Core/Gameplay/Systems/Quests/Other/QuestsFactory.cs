using System.Collections.Generic;
using GameCore.Infrastructure.Configs.Gameplay.Quests;
using GameCore.Infrastructure.Configs.Gameplay.QuestsItems;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Items;
using Sirenix.Utilities;
using UnityEngine;

namespace GameCore.Gameplay.Systems.Quests
{
    public class QuestsFactory
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public QuestsFactory(QuestsStorage questsStorage, QuestsConfigMeta questsConfig,
            QuestsItemsConfigMeta questsItemsConfig)
        {
            _questsStorage = questsStorage;
            _questsConfig = questsConfig;
            _questsItemsConfig = questsItemsConfig;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly QuestsStorage _questsStorage;
        private readonly QuestsConfigMeta _questsConfig;
        private readonly QuestsItemsConfigMeta _questsItemsConfig;

        private int _lastQuestID;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Create() => CreationLogic();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CreationLogic()
        {
            List<QuestDifficulty> availableDifficulties = new();

            GetAvailableDifficulties();
            CreateQuestsData();

            // LOCAL METHODS: -----------------------------

            void GetAvailableDifficulties()
            {
                IReadOnlyCollection<QuestDifficulty> questsLookUpList = _questsConfig.GetQuestsLookUpList();
                IReadOnlyList<QuestRuntimeData> awaitingQuestsData = _questsStorage.GetAwaitingQuestsData();

                availableDifficulties.AddRange(questsLookUpList);

                foreach (QuestRuntimeData questRuntimeData in awaitingQuestsData)
                {
                    QuestDifficulty difficulty = questRuntimeData.Difficulty;
                    availableDifficulties.Remove(difficulty);
                }
            }

            void CreateQuestsData()
            {
                foreach (QuestDifficulty difficulty in availableDifficulties)
                {
                    _lastQuestID++;

                    QuestPresetConfig questPresetConfig = GetQuestPresetConfig(difficulty);
                    int daysLeft = questPresetConfig.GetRandomDeadline();
                    int reward = Random.Range(10, 100);

                    IReadOnlyDictionary<int, QuestItemData> questItemsID =
                        CreateQuestItems(questPresetConfig, difficulty);

                    QuestRuntimeData questRuntimeData =
                        new(difficulty, questID: _lastQuestID, reward, daysLeft, questItemsID);

                    _questsStorage.AddAwaitingQuestData(questRuntimeData);
                }
            }
        }

        private IReadOnlyDictionary<int, QuestItemData> CreateQuestItems(QuestPresetConfig questPresetConfig,
            QuestDifficulty difficulty)
        {
            Dictionary<int, QuestItemData> questItems = new();
            int itemsListLength = questPresetConfig.GetRandomItemsListLength();

            ItemsReference itemsReference = _questsItemsConfig.GetItemsReference(difficulty);
            IReadOnlyList<ItemMeta> allItemsMeta = itemsReference.GetAllItemsMeta();
            int allItemsAmount = allItemsMeta.Count;

            const int itemSearchAttempts = 10;

            for (int i = 0; i < itemsListLength; i++)
            {
                int itemID = 0;
                bool uniqueItemFound = false;

                // Search for the Item ID for the selected Quest Difficulty
                for (int j = 0; j < itemSearchAttempts; j++)
                {
                    int itemIndex = Random.Range(0, allItemsAmount);
                    itemID = allItemsMeta[itemIndex].ItemID;

                    bool alreadyContainsItem = questItems.ContainsKey(itemID);

                    if (alreadyContainsItem)
                        continue;

                    uniqueItemFound = true;
                    break;
                }

                if (!uniqueItemFound)
                    continue;

                int itemQuantity = questPresetConfig.GetRandomItemQuantity();
                QuestItemData questItemData = new(itemQuantity);
                questItems.Add(itemID, questItemData);
            }

            return questItems;
        }

        private QuestPresetConfig GetQuestPresetConfig(QuestDifficulty difficulty)
        {
            IEnumerable<QuestPresetConfig> allQuestsPresets = _questsConfig.GetAllQuestsPresets();

            foreach (QuestPresetConfig questPresetConfig in allQuestsPresets)
            {
                bool isMatches = questPresetConfig.Difficulty == difficulty;

                if (isMatches)
                    return questPresetConfig;
            }

            Log.PrintError(log: $"{typeof(QuestPresetConfig).GetNiceName()} <gb>{difficulty}</gb> <rb>not found!</rb>");
            return null;
        }
    }
}