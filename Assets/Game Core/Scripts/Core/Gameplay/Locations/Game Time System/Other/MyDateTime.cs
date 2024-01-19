using Unity.Netcode;

namespace GameCore.Gameplay.Locations.GameTime
{
    public struct MyDateTime : INetworkSerializable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        public MyDateTime(int second, int minute, int hour)
        {
            _second = second;
            _minute = minute;
            _hour = hour;
        }
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public int Second => _second;
        public int Minute => _minute;
        public int Hour => _hour;

        // FIELDS: --------------------------------------------------------------------------------
        
        private int _second;
        private int _minute;
        private int _hour;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _second);
            serializer.SerializeValue(ref _minute);
            serializer.SerializeValue(ref _hour);
        }
    }
}