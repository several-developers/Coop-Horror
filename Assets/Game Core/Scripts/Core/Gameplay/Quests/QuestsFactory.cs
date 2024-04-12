using System.Collections.Generic;
using GameCore.Configs.Gameplay.Quests;
using GameCore.Configs.Gameplay.QuestsItems;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Items;
using Sirenix.Utilities;
using UnityEngine;

namespace GameCore.Gameplay.Quests
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

        private int _activeQuestsAmount;
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
                IReadOnlyList<QuestRuntimeData> activeQuestsData = _questsStorage.GetActiveQuestsData();

                availableDifficulties.AddRange(questsLookUpList);

                foreach (QuestRuntimeData questRuntimeData in activeQuestsData)
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

                    IReadOnlyDictionary<int, int> questItemsID = CreateQuestItems(difficulty);
                    QuestRuntimeData questRuntimeData = new(difficulty, questID: _lastQuestID, reward: 0, questItemsID);

                    _questsStorage.AddAwaitingQuestData(questRuntimeData);
                }
            }
        }

        private IReadOnlyDictionary<int, int> CreateQuestItems(QuestDifficulty difficulty)
        {
            Dictionary<int, int> questItems = new();
            QuestPresetConfig questPresetConfig = GetQuestPresetConfig(difficulty);
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
                questItems.Add(itemID, itemQuantity);
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