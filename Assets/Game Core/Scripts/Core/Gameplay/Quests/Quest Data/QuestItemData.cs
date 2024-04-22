using Unity.Netcode;

namespace GameCore.Gameplay.Quests
{
    public struct QuestItemData : INetworkSerializable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public QuestItemData(int targetItemQuantity)
        {
            _targetItemQuantity = targetItemQuantity;
            _currentItemQuantity = 0;
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public int TargetItemQuantity => _targetItemQuantity;
        public int CurrentItemQuantity => _currentItemQuantity;
        
        // FIELDS: --------------------------------------------------------------------------------

        private int _targetItemQuantity;
        private int _currentItemQuantity;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _targetItemQuantity);
            serializer.SerializeValue(ref _currentItemQuantity);
        }

        public void IncreaseItemQuantity() =>
            _currentItemQuantity++;

        public float GetProgress()
        {
            if (IsCompleted())
                return 1f;
            
            return _currentItemQuantity / (float)_targetItemQuantity;
        }

        public bool IsCompleted() =>
            _currentItemQuantity == _targetItemQuantity;
    }
}