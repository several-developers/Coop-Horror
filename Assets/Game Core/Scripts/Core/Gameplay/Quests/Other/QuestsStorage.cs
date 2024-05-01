using System.Collections.Generic;

namespace GameCore.Gameplay.Quests
{
    public class QuestsStorage
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public QuestsStorage()
        {
            _awaitingQuestsData = new List<QuestRuntimeData>(capacity: 6);
            _activeQuestsData = new List<QuestRuntimeData>(capacity: 6);
            _completedQuestsData = new List<QuestRuntimeData>(capacity: 6);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly List<QuestRuntimeData> _awaitingQuestsData;
        private readonly List<QuestRuntimeData> _activeQuestsData;
        private readonly List<QuestRuntimeData> _completedQuestsData;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void AddAwaitingQuestData(QuestRuntimeData questRuntimeData) =>
            _awaitingQuestsData.Add(questRuntimeData);

        public void UpdateAwaitingQuestsData(IEnumerable<QuestRuntimeDataContainer> questsRuntimeDataContainers)
        {
            ClearData();

            foreach (QuestRuntimeDataContainer dataContainer in questsRuntimeDataContainers)
            {
                QuestRuntimeData runtimeData = new(dataContainer);
                _awaitingQuestsData.Add(runtimeData);
            }
        }

        public void SelectQuest(int questID)
        {
            int awaitingQuestsAmount = _awaitingQuestsData.Count;
            int questIndex = -1;
            
            for (int i = awaitingQuestsAmount - 1; i >= 0; i--)
            {
                bool isMatches = _awaitingQuestsData[i].QuestID == questID;

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

            QuestRuntimeData questRuntimeData = _awaitingQuestsData[questIndex];
            _awaitingQuestsData.RemoveAt(questIndex);
            AddActiveQuestData(questRuntimeData);
        }

        public void SubmitQuestItem(int itemID)
        {
            foreach (QuestRuntimeData questRuntimeData in _activeQuestsData)
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
            foreach (QuestRuntimeData questRuntimeData in _activeQuestsData)
                questRuntimeData.DecreaseDay();
        }

        public void CompleteQuests()
        {
            int iterations = _activeQuestsData.Count;

            for (int i = iterations - 1; i >= 0; i--)
            {
                QuestRuntimeData questRuntimeData = _activeQuestsData[i];
                bool isCompleted = questRuntimeData.IsCompleted();

                if (!isCompleted)
                    continue;
                
                _completedQuestsData.Add(questRuntimeData);
                _activeQuestsData.RemoveAt(i);
            }
        }

        public void ClearCompletedQuests() =>
            _completedQuestsData.Clear();

        public void ClearAll()
        {
            _awaitingQuestsData.Clear();
            _activeQuestsData.Clear();
            _completedQuestsData.Clear();
        }

        public IReadOnlyList<QuestRuntimeData> GetAwaitingQuestsData() => _awaitingQuestsData;
        
        public IReadOnlyList<QuestRuntimeData> GetActiveQuestsData() => _activeQuestsData;

        public QuestRuntimeDataContainer[] GetAwaitingQuestsRuntimeDataContainers()
        {
            int activeQuestsAmount = _awaitingQuestsData.Count;
            var questsRuntimeDataContainers = new QuestRuntimeDataContainer[activeQuestsAmount];

            for (int i = 0; i < activeQuestsAmount; i++)
            {
                QuestRuntimeData questRuntimeData = _awaitingQuestsData[i];
                QuestRuntimeDataContainer questRuntimeDataContainer = new(questRuntimeData);
                questsRuntimeDataContainers[i] = questRuntimeDataContainer;
            }

            return questsRuntimeDataContainers;
        }

        public int GetActiveQuestsAmount() =>
            _activeQuestsData.Count;
        
        public int CalculateReward()
        {
            int reward = 0;

            foreach (QuestRuntimeData questRuntimeData in _completedQuestsData)
                reward += questRuntimeData.Reward;
            
            return reward;
        }

        public bool ContainsItemInQuests(int itemID)
        {
            foreach (QuestRuntimeData questRuntimeData in _activeQuestsData)
            {
                bool containsItem = questRuntimeData.ContainsItem(itemID);

                if (containsItem)
                    return true;
            }

            return false;
        }

        public bool ContainsCompletedQuests()
        {
            foreach (QuestRuntimeData questRuntimeData in _activeQuestsData)
            {
                bool isCompleted = questRuntimeData.IsCompleted();

                if (isCompleted)
                    return true;
            }

            return false;
        }

        public bool ContainsExpiredQuests()
        {
            foreach (QuestRuntimeData questRuntimeData in _activeQuestsData)
            {
                bool isExpired = questRuntimeData.IsExpired();

                if (isExpired)
                    return true;
            }

            return false;
        }

        public bool ContainsExpiredAndUncompletedQuests()
        {
            foreach (QuestRuntimeData questRuntimeData in _activeQuestsData)
            {
                bool isExpired = questRuntimeData.IsExpired();
                bool isCompleted = questRuntimeData.IsCompleted();

                if (isExpired && !isCompleted)
                    return true;
            }

            return false;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------


        private void AddActiveQuestData(QuestRuntimeData questRuntimeData) =>
            _activeQuestsData.Add(questRuntimeData);

        private void ClearData() =>
            _activeQuestsData.Clear();
    }
}