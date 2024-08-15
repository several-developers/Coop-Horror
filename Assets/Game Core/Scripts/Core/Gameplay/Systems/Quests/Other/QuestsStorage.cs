using System.Collections.Generic;

namespace GameCore.Gameplay.Systems.Quests
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

        public void AddActiveQuestData(QuestRuntimeData questRuntimeData) =>
            _activeQuestsData.Add(questRuntimeData);

        public void AddCompletedQuestData(QuestRuntimeData questRuntimeData) =>
            _completedQuestsData.Add(questRuntimeData);

        public void UpdateAwaitingQuestsData(IEnumerable<QuestRuntimeDataContainer> questsRuntimeDataContainers)
        {
            ClearActiveQuests();

            foreach (QuestRuntimeDataContainer dataContainer in questsRuntimeDataContainers)
            {
                QuestRuntimeData runtimeData = new(dataContainer);
                _awaitingQuestsData.Add(runtimeData);
            }
        }

        public void RemoveAwaitingQuestData(int index) =>
            _awaitingQuestsData.RemoveAt(index);

        public void RemoveActiveQuestData(int index) =>
            _activeQuestsData.RemoveAt(index);

        public void ClearCompletedQuests() =>
            _completedQuestsData.Clear();

        public void ClearAll()
        {
            ClearAwaitingQuests();
            ClearActiveQuests();
            ClearCompletedQuests();
        }

        public IReadOnlyList<QuestRuntimeData> GetAwaitingQuestsData() => _awaitingQuestsData;

        public IReadOnlyList<QuestRuntimeData> GetActiveQuestsData() => _activeQuestsData;

        public IReadOnlyList<QuestRuntimeData> GetCompletedQuestsData() => _completedQuestsData;

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

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ClearAwaitingQuests() =>
            _awaitingQuestsData.Clear();
        
        private void ClearActiveQuests() =>
            _activeQuestsData.Clear();
    }
}