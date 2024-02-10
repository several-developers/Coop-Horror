using Unity.Netcode;

namespace GameCore.Gameplay.Dungeons
{
    public struct DungeonsSeedData : INetworkSerializable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public DungeonsSeedData(int seedOne, int seedTwo, int seedThree)
        {
            _seedOne = seedOne;
            _seedTwo = seedTwo;
            _seedThree = seedThree;
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public int SeedOne => _seedOne;
        public int SeedTwo => _seedTwo;
        public int SeedThree => _seedThree;

        // FIELDS: --------------------------------------------------------------------------------

        private int _seedOne;
        private int _seedTwo;
        private int _seedThree;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _seedOne);
            serializer.SerializeValue(ref _seedTwo);
            serializer.SerializeValue(ref _seedThree);
        }
    }
}