using Cysharp.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
#pragma warning disable CS1998

namespace GameCore.Gameplay.Network.ConnectionManagement
{
    public class ConnectionMethodIP : ConnectionMethodBase
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ConnectionMethodIP(string ip, ushort port, ConnectionManager connectionManager,
            ProfileManager profileManager, string playerName) : base(connectionManager, profileManager, playerName)
        {
            _ipAddress = ip;
            _port = port;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly string _ipAddress;
        private readonly ushort _port;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override async UniTask SetupClientConnectionAsync()
        {
            SetConnectionPayload(GetPlayerId(), PlayerName);
            
            var utp = (UnityTransport)ConnectionManager.NetworkManager.NetworkConfig.NetworkTransport;
            utp.SetConnectionData(_ipAddress, _port);
        }

        public override async UniTask SetupHostConnectionAsync()
        {
            // Need to set connection payload for host as well, as host is a client too
            SetConnectionPayload(GetPlayerId(), PlayerName);
            
            var utp = (UnityTransport)ConnectionManager.NetworkManager.NetworkConfig.NetworkTransport;
            utp.SetConnectionData(_ipAddress, _port);
        }
    }
}