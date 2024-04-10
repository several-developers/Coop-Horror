using System.Collections.Generic;

namespace GameCore.Gameplay.Quests
{
    public class QuestsStorage
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public QuestsStorage() =>
            _activeQuestsData = new List<QuestRuntimeData>(capacity: 6);

        // FIELDS: --------------------------------------------------------------------------------

        private readonly List<QuestRuntimeData> _activeQuestsData;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void AddQuestData(QuestRuntimeData questRuntimeData) =>
            _activeQuestsData.Add(questRuntimeData);

        public void UpdateQuestsData(IEnumerable<QuestRuntimeDataContainer> questsRuntimeDataContainers)
        {
            ClearData();

            foreach (QuestRuntimeDataContainer dataContainer in questsRuntimeDataContainers)
            {
                QuestRuntimeData runtimeData = new(dataContainer);
                _activeQuestsData.Add(runtimeData);
            }
        }

        public IReadOnlyList<QuestRuntimeData> GetActiveQuestsData() => _activeQuestsData;

        public QuestRuntimeDataContainer[] GetQuestsRuntimeDataContainers()
        {
            int activeQuestsAmount = _activeQuestsData.Count;
            var questsRuntimeDataContainers = new QuestRuntimeDataContainer[activeQuestsAmount];

            for (int i = 0; i < activeQuestsAmount; i++)
            {
                QuestRuntimeData questRuntimeData = _activeQuestsData[i];
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