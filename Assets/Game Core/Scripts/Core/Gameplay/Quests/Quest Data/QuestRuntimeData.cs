using System.Collections.Generic;
using GameCore.Enums.Gameplay;

namespace GameCore.Gameplay.Quests
{
    public class QuestRuntimeData
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public QuestRuntimeData(QuestDifficulty difficulty, int questID, int reward,
            IReadOnlyDictionary<int, int> questItems)
        {
            Difficulty = difficulty;
            QuestID = questID;
            Reward = reward;
            _questItems = new Dictionary<int, int>(capacity: questItems.Count);

            foreach (KeyValuePair<int, int> pair in questItems)
                _questItems.Add(pair.Key, pair.Value);
        }

        public QuestRuntimeData(QuestRuntimeDataContainer questRuntimeDataContainer)
        {
            Difficulty = questRuntimeDataContainer.Difficulty;
            QuestID = questRuntimeDataContainer.QuestID;
            Reward = questRuntimeDataContainer.Reward;

            int[] questItemsID = questRuntimeDataContainer.QuestItemsID;
            int[] questItemsQuantity = questRuntimeDataContainer.QuestItemsQuantity;
            int itemsAmount = questItemsID.Length;
            bool isAmountEquals = questItemsID.Length == questItemsQuantity.Length;

            if (!isAmountEquals)
            {
                Log.PrintError(log: "Items ID and quantity are <rb>not equals</rb>!");
                return;
            }

            _questItems = new Dictionary<int, int>(capacity: itemsAmount);

            for (int i = 0; i < itemsAmount; i++)
            {
                int itemID = questItemsID[i];
                int itemQuantity = questItemsQuantity[i];
                _questItems.Add(itemID, itemQuantity);
            }
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public QuestDifficulty Difficulty { get; }
        public int QuestID { get; }
        public int Reward { get; }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly Dictionary<int, int> _questItems; // <item_id, item_quantity>

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public IReadOnlyDictionary<int, int> GetQuestItems() => _questItems;

        public int GetQuestItemsAmount() =>
            _questItems.Count;

        public int GetItemsTotalAmount()
        {
            int totalAmount = 0;

            foreach (int itemQuantity in _questItems.Values)
                totalAmount += itemQuantity;

            return totalAmount;
        }
    }
}