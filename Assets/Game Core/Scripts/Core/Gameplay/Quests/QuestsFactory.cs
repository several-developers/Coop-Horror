using System.Collections.Generic;
using GameCore.Configs.Gameplay.Quests;
using GameCore.Enums.Gameplay;
using Sirenix.Utilities;
using UnityEngine;

namespace GameCore.Gameplay.Quests
{
    public class QuestsFactory
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public QuestsFactory(QuestsStorage questsStorage, QuestsConfigMeta questsConfig)
        {
            _questsStorage = questsStorage;
            _questsConfig = questsConfig;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly QuestsStorage _questsStorage;
        private readonly QuestsConfigMeta _questsConfig;

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

                    _questsStorage.AddQuestData(questRuntimeData);
                }
            }
        }

        private IReadOnlyDictionary<int, int> CreateQuestItems(QuestDifficulty difficulty)
        {
            Dictionary<int, int> questItems = new();
            QuestPresetConfig questPresetConfig = GetQuestPresetConfig(difficulty);
            int itemsListLength = questPresetConfig.GetRandomItemsListLength();

            for (int i = 0; i < itemsListLength; i++)
            {
                int itemID = Random.Range(-9999, 9999);
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