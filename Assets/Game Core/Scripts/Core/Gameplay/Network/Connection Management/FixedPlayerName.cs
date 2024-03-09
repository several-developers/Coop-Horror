using Unity.Collections;
using Unity.Netcode;

namespace GameCore.Gameplay.Network.ConnectionManagement
{
    /// <summary>
    /// Wrapping FixedString so that if we want to change player name max size in the future, we only do it once here
    /// </summary>
    public struct FixedPlayerName : INetworkSerializable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter =>
            serializer.SerializeValue(ref _name);

        // FIELDS: --------------------------------------------------------------------------------
        
        private FixedString32Bytes _name;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public override string ToString() =>
            _name.Value;

        public static implicit operator string(FixedPlayerName s) => s.ToString();
        public static implicit operator FixedPlayerName(string s) => new() { _name = new FixedString32Bytes(s) };
    }
}