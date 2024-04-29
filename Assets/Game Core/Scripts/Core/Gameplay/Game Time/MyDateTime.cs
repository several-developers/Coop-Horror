using Unity.Netcode;

namespace GameCore.Gameplay.GameTimeManagement
{
    public struct MyDateTime : INetworkSerializable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        public MyDateTime(int second, int minute, int hour, int day)
        {
            _second = second;
            _minute = minute;
            _hour = hour;
            _day = day;
        }
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public int Second => _second;
        public int Minute => _minute;
        public int Hour => _hour;
        public int Day => _day;

        // FIELDS: --------------------------------------------------------------------------------
        
        private int _second;
        private int _minute;
        private int _hour;
        private int _day;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _second);
            serializer.SerializeValue(ref _minute);
            serializer.SerializeValue(ref _hour);
            serializer.SerializeValue(ref _day);
        }

        public void ResetDay() =>
            _day = 0;
    }
}