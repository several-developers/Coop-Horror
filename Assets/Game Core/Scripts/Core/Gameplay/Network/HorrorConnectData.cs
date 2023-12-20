using Unity.Netcode;
using NetcodePlus.Demo;

namespace GameCore.Gameplay.Network
{
    [System.Serializable]
    public class HorrorConnectData : INetworkSerializable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public HorrorConnectData() { }

        public HorrorConnectData(GameMode gameMode) =>
            _mode = gameMode;

        // PROPERTIES: ----------------------------------------------------------------------------

        public GameMode Mode => _mode;
        public string Character => _character;

        // FIELDS: --------------------------------------------------------------------------------

        private GameMode _mode = GameMode.None;
        private string _character = "";

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetCharacter(string character) =>
            _character = character;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _mode);
            serializer.SerializeValue(ref _character);
        }
    }
}