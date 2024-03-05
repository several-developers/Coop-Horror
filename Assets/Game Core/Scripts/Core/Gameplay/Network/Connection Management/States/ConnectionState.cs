using GameCore.Enums.Global;
using GameCore.Gameplay.PubSub;
using Unity.Netcode;

namespace GameCore.Gameplay.Network.ConnectionManagement
{
    internal abstract class ConnectionState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        protected ConnectionState(ConnectionManager connectionManager, IPublisher<ConnectStatus> connectStatusPublisher)
        {
            ConnectionManager = connectionManager;
            ConnectStatusPublisher = connectStatusPublisher;
        }

        // FIELDS: --------------------------------------------------------------------------------

        protected readonly ConnectionManager ConnectionManager;

        protected readonly IPublisher<ConnectStatus> ConnectStatusPublisher;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public abstract void Enter();

        public abstract void Exit();

        public virtual void OnClientConnected(ulong clientId)
        {
        }

        public virtual void OnClientDisconnect(ulong clientId)
        {
        }

        public virtual void OnServerStarted()
        {
        }

        public virtual void StartClientIP(string playerName, string ipAddress, int port)
        {
        }

        public virtual void StartClientLobby(string playerName)
        {
        }

        public virtual void StartHostIP(string playerName, string ipaddress, int port)
        {
        }

        public virtual void StartHostLobby(string playerName)
        {
        }

        public virtual void OnUserRequestedShutdown()
        {
        }

        public virtual void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request,
            NetworkManager.ConnectionApprovalResponse response)
        {
        }


        public virtual void OnTransportFailure()
        {
        }
    }
}