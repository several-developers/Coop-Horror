using System.Collections.Generic;
using GameCore.Enums.Gameplay;
using UnityEngine;

namespace GameCore.Gameplay.Systems.Quests
{
    public class QuestRuntimeData
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public QuestRuntimeData(QuestDifficulty difficulty, int questID, int reward, int daysLeft,
            IReadOnlyDictionary<int, QuestItemData> questItems)
        {
            Difficulty = difficulty;
            QuestID = questID;
            Reward = reward;
            DaysLeft = daysLeft;
            _questItems = new Dictionary<int, QuestItemData>(capacity: questItems.Count);

            foreach (KeyValuePair<int, QuestItemData> pair in questItems)
                _questItems.Add(pair.Key, pair.Value);
        }

        public QuestRuntimeData(QuestRuntimeDataContainer questRuntimeDataContainer)
        {
            Difficulty = questRuntimeDataContainer.Difficulty;
            QuestID = questRuntimeDataContainer.QuestID;
            Reward = questRuntimeDataContainer.Reward;
            DaysLeft = questRuntimeDataContainer.DaysLeft;

            int[] questItemsID = questRuntimeDataContainer.QuestItemsID;
            QuestItemData[] questItemsData = questRuntimeDataContainer.QuestItemsData;
            int itemsAmount = questItemsID.Length;
            bool isAmountEquals = questItemsID.Length == questItemsData.Length;

            if (!isAmountEquals)
            {
                Log.PrintError(log: "Items ID and quantity are <rb>not equals</rb>!");
                return;
            }

            _questItems = new Dictionary<int, QuestItemData>(capacity: itemsAmount);

            for (int i = 0; i < itemsAmount; i++)
            {
                int itemID = questItemsID[i];
                QuestItemData questItemData = questItemsData[i];
                _questItems.Add(itemID, questItemData);
            }
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public QuestDifficulty Difficulty { get; }
        public int QuestID { get; }
        public int Reward { get; }
        public int DaysLeft { get; private set; }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly Dictionary<int, QuestItemData> _questItems; // <item_id, item_quantity>

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SubmitQuestItem(int itemID)
        {
            bool containsItem = _questItems.ContainsKey(itemID);

            if (!containsItem)
            {
                Log.PrintError(log: $"Item with ID <gb>({itemID})</gb> <rb>not found</rb>!");
                return;
            }

            QuestItemData questItemData = _questItems[itemID];
            questItemData.IncreaseItemQuantity();

            _questItems[itemID] = questItemData;
        }

        public void DecreaseDay()
        {
            if (DaysLeft <= 0)
                return;

            DaysLeft--;
        }
        
        public IReadOnlyDictionary<int, QuestItemData> GetQuestItems() => _questItems;

        public float GetQuestProgress()
        {
            float questProgress = 0f;
            float progressSum = 0f;
            int questItemsAmount = _questItems.Count;

            foreach (QuestItemData questItemData in _questItems.Values)
                progressSum += questItemData.GetProgress();

            if (questItemsAmount > 0)
                questProgress = progressSum / questItemsAmount;

            return questProgress;
        }
        
        public int GetQuestItemsAmount() =>
            _questItems.Count;

        public int GetItemsTotalAmount()
        {
            int totalAmount = 0;

            foreach (QuestItemData questItemData in _questItems.Values)
                totalAmount += questItemData.TargetItemQuantity;

            return totalAmount;
        }

        public bool ContainsItem(int itemID) =>
            _questItems.ContainsKey(itemID);

        public bool IsExpired() =>
            DaysLeft <= 0;

        public bool IsCompleted()
        {
            bool isCompleted = true;

            foreach (QuestItemData questItemData in _questItems.Values)
            {
                bool isQuestItemCompleted = questItemData.IsCompleted();

                if (isQuestItemCompleted)
                    continue;

                isCompleted = false;
                break;
            }

            return isCompleted;
        }
    }
}