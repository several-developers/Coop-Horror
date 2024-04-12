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
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly List<QuestRuntimeData> _awaitingQuestsData;
        private readonly List<QuestRuntimeData> _activeQuestsData;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void AddAwaitingQuestData(QuestRuntimeData questRuntimeData) =>
            _awaitingQuestsData.Add(questRuntimeData);
        
        public void AddActiveQuestData(QuestRuntimeData questRuntimeData) =>
            _activeQuestsData.Add(questRuntimeData);

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

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ClearData() =>
            _activeQuestsData.Clear();
    }
}