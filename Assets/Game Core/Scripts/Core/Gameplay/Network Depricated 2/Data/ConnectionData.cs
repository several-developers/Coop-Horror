using System;
using Unity.Netcode;

namespace GameCore.Gameplay.NetworkDepricated2
{
    [Serializable]
    public class ConnectionData : INetworkSerializable
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public string _userID = "";
        public string _username = "";

        //Client created the game? (Could be true for client that created the game on dedicated server)
        public bool _isHost;

        public byte[] _extra = Array.Empty<byte>();

        //If you add extra data, make sure the total size of ConnectionData doesn't exceed Netcode max unfragmented msg (1400 bytes)
        //Fragmented msg are not possible for connection data, since connection is done in a single request

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _userID);
            serializer.SerializeValue(ref _username);
            serializer.SerializeValue(ref _isHost);
            serializer.SerializeValue(ref _extra);
        }
    }
}