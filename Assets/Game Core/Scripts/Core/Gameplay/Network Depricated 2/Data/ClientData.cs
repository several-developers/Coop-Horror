using System;
using GameCore.Utilities;
using Unity.Netcode;

namespace GameCore.Gameplay.NetworkDepricated2
{
    public class ClientData
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        public ClientData()
        {
        }

        public ClientData(ulong clientID) =>
            ClientID = clientID;
        
        // FIELDS: --------------------------------------------------------------------------------
        
        public ulong ClientID;
        public int PlayerID;
        public string UserID;
        public string Username;
        public bool IsHost; //Doesn't necessarily mean its the server host, just that its the one that created the game
        public bool DataReceived; //Data was received from this user
        public ClientState state = ClientState.Offline;
        public byte[] extra = Array.Empty<byte>();

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public string GetExtraString() =>
            NetworkUtilities.DeserializeString(extra);

        public T GetExtraData<T>() where T : INetworkSerializable, new() =>
            NetworkUtilities.NetDeserialize<T>(extra);
    }
}