using System.Collections.Generic;

namespace GameCore.Gameplay.Systems.Quests
{
    public class QuestsController
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public QuestsController(QuestsStorage questsStorage) =>
            _questsStorage = questsStorage;

        // FIELDS: --------------------------------------------------------------------------------

        private readonly QuestsStorage _questsStorage;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SelectQuest(int questID)
        {
            IReadOnlyList<QuestRuntimeData> awaitingQuestsData = _questsStorage.GetAwaitingQuestsData();

            int awaitingQuestsAmount = awaitingQuestsData.Count;
            int questIndex = -1;

            for (int i = awaitingQuestsAmount - 1; i >= 0; i--)
            {
                bool isMatches = awaitingQuestsData[i].QuestID == questID;

                if (!isMatches)
                    continue;

                questIndex = i;
                break;
            }

            bool isAwaitingQuestFound = questIndex != -1;

            if (!isAwaitingQuestFound)
            {
                Log.PrintError(log: $"Awaiting quest <gb>({questID})</gb> <rb>not found</rb>!");
                return;
            }

            QuestRuntimeData questRuntimeData = awaitingQuestsData[questIndex];

            _questsStorage.RemoveAwaitingQuestData(questIndex);
            _questsStorage.AddActiveQuestData(questRuntimeData);
        }

        public void SubmitQuestItem(int itemID)
        {
            IReadOnlyList<QuestRuntimeData> activeQuestsData = GetActiveQuestsData();

            foreach (QuestRuntimeData questRuntimeData in activeQuestsData)
            {
                bool isCompleted = questRuntimeData.IsCompleted();

                if (isCompleted)
                    continue;

                bool containsItem = questRuntimeData.ContainsItem(itemID);

                if (!containsItem)
                    continue;

                questRuntimeData.SubmitQuestItem(itemID);
                break;
            }
        }

        public void DecreaseDays()
        {
            IReadOnlyList<QuestRuntimeData> activeQuestsData = GetActiveQuestsData();

            foreach (QuestRuntimeData questRuntimeData in activeQuestsData)
                questRuntimeData.DecreaseDay();
        }

        public void CompleteQuests()
        {
            IReadOnlyList<QuestRuntimeData> activeQuestsData = GetActiveQuestsData();
            int iterations = activeQuestsData.Count;

            for (int i = iterations - 1; i >= 0; i--)
            {
                QuestRuntimeData questRuntimeData = activeQuestsData[i];
                bool isCompleted = questRuntimeData.IsCompleted();

                if (!isCompleted)
                    continue;

                _questsStorage.AddCompletedQuestData(questRuntimeData);
                _questsStorage.RemoveActiveQuestData(i);
            }
        }

        public int CalculateReward()
        {
            IReadOnlyList<QuestRuntimeData> completedQuestsData = _questsStorage.GetCompletedQuestsData();
            int reward = 0;

            foreach (QuestRuntimeData questRuntimeData in completedQuestsData)
                reward += questRuntimeData.Reward;

            return reward;
        }

        public bool ContainsItemInQuests(int itemID)
        {
            IReadOnlyList<QuestRuntimeData> activeQuestsData = GetActiveQuestsData();

            foreach (QuestRuntimeData questRuntimeData in activeQuestsData)
            {
                bool containsItem = questRuntimeData.ContainsItem(itemID);

                if (containsItem)
                    return true;
            }

            return false;
        }

        public bool ContainsCompletedQuests()
        {
            IReadOnlyList<QuestRuntimeData> activeQuestsData = GetActiveQuestsData();

            foreach (QuestRuntimeData questRuntimeData in activeQuestsData)
            {
                bool isCompleted = questRuntimeData.IsCompleted();

                if (isCompleted)
                    return true;
            }

            return false;
        }

        public bool ContainsExpiredQuests()
        {
            IReadOnlyList<QuestRuntimeData> activeQuestsData = GetActiveQuestsData();

            foreach (QuestRuntimeData questRuntimeData in activeQuestsData)
            {
                bool isExpired = questRuntimeData.IsExpired();

                if (isExpired)
                    return true;
            }

            return false;
        }

        public bool ContainsExpiredAndUncompletedQuests()
        {
            IReadOnlyList<QuestRuntimeData> activeQuestsData = GetActiveQuestsData();

            foreach (QuestRuntimeData questRuntimeData in activeQuestsData)
            {
                bool isExpired = questRuntimeData.IsExpired();
                bool isCompleted = questRuntimeData.IsCompleted();

                if (isExpired && !isCompleted)
                    return true;
            }

            return false;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private IReadOnlyList<QuestRuntimeData> GetActiveQuestsData() =>
            _questsStorage.GetActiveQuestsData();
    }
}