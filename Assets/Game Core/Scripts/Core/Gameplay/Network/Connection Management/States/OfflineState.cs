namespace GameCore.Gameplay.Network.ConnectionManagement
{
    internal class OfflineState : ConnectionState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        public OfflineState(ConnectionManager connectionManager) : base(connectionManager)
        {
        }

        // FIELDS: --------------------------------------------------------------------------------
        

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void Enter()
        {
            ConnectionManager.NetworkManager.Shutdown();
        }

        public override void Exit()
        {
        }
        
        public override void StartClientIP(string playerName, string ipaddress, int port)
        {
            var connectionMethod = new ConnectionMethodIP(ipaddress, (ushort)port, ConnectionManager, playerName);
            ConnectionManager.ClientConnectingState.Configure(connectionMethod);
            ConnectionManager.ChangeState(ConnectionManager.ClientConnectingState.Configure(connectionMethod));
        }
        
        public override void StartClientLobby(string playerName)
        {
            var connectionMethod = new ConnectionMethodRelay(ConnectionManager, playerName);
            ConnectionManager.ClientReconnectingState.Configure(connectionMethod);
            ConnectionManager.ChangeState(ConnectionManager.ClientConnectingState.Configure(connectionMethod));
        }
        
        public override void StartHostIP(string playerName, string ipaddress, int port)
        {
            var connectionMethod = new ConnectionMethodIP(ipaddress, (ushort)port, ConnectionManager, playerName);
            ConnectionManager.ChangeState(ConnectionManager.StartingHostState.Configure(connectionMethod));
        }

        public override void StartHostLobby(string playerName)
        {
            var connectionMethod = new ConnectionMethodRelay(ConnectionManager, playerName);
            ConnectionManager.ChangeState(ConnectionManager.StartingHostState.Configure(connectionMethod));
        }
    }
}