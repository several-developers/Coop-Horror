using System.Collections.Generic;
using GameCore.Enums.Gameplay;
using Unity.Netcode;

namespace GameCore.Gameplay.Quests
{
    public struct QuestRuntimeDataContainer : INetworkSerializable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public QuestRuntimeDataContainer(QuestRuntimeData questRuntimeData)
        {
            _difficulty = questRuntimeData.Difficulty;
            _questID = questRuntimeData.QuestID;
            _reward = questRuntimeData.Reward;

            IReadOnlyDictionary<int, int> questItems = questRuntimeData.GetQuestItems();
            int questItemsAmount = questRuntimeData.GetQuestItemsAmount();

            _questItemsID = new int[questItemsAmount];
            _questItemsQuantity = new int[questItemsAmount];

            int i = 0;

            foreach (KeyValuePair<int, int> pair in questItems)
            {
                _questItemsID[i] = pair.Key;
                _questItemsQuantity[i] = pair.Value;
                i++;
            }
        }
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public int[] QuestItemsID => _questItemsID;
        public int[] QuestItemsQuantity => _questItemsQuantity;
        public QuestDifficulty Difficulty => _difficulty;
        public int QuestID => _questID;
        public int Reward => _reward;

        // FIELDS: --------------------------------------------------------------------------------
        
        private int[] _questItemsID;
        private int[] _questItemsQuantity;
        private QuestDifficulty _difficulty;
        private int _questID;
        private int _reward;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _questItemsID);
            serializer.SerializeValue(ref _questItemsQuantity);
            serializer.SerializeValue(ref _difficulty);
            serializer.SerializeValue(ref _questID);
            serializer.SerializeValue(ref _reward);
        }
    }
}