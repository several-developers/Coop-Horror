using GameCore.Enums.Global;
using GameCore.Gameplay.Network.UnityServices.Lobbies;
using GameCore.Gameplay.PubSub;
using UnityEngine.SceneManagement;

namespace GameCore.Gameplay.Network.ConnectionManagement
{
    internal class OfflineState : ConnectionState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        public OfflineState(ConnectionManager connectionManager, IPublisher<ConnectStatus> connectStatusPublisher,
            ProfileManager profileManager, LobbyServiceFacade lobbyServiceFacade, LocalLobby localLobby)
            : base(connectionManager, connectStatusPublisher)
        {
            _profileManager = profileManager;
            _lobbyServiceFacade = lobbyServiceFacade;
            _localLobby = localLobby;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly ProfileManager _profileManager;
        private readonly LobbyServiceFacade _lobbyServiceFacade;
        private readonly LocalLobby _localLobby;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void Enter()
        {
            _lobbyServiceFacade.EndTracking();
            ConnectionManager.NetworkManager.Shutdown();
            TryLoadMainMenu();
        }

        public override void Exit()
        {
        }
        
        public override void StartClientIP(string playerName, string ipAddress, int port)
        {
            var connectionMethod = new ConnectionMethodIP(ipAddress, (ushort)port, ConnectionManager,
                _profileManager, playerName);
            
            ConnectionManager.ClientConnectingState.Configure(connectionMethod);
            ConnectionManager.ChangeState(ConnectionManager.ClientConnectingState.Configure(connectionMethod));
        }
        
        public override void StartClientLobby(string playerName)
        {
            var connectionMethod = new ConnectionMethodRelay(ConnectionManager, _profileManager, playerName,
                _lobbyServiceFacade, _localLobby);
            
            ConnectionManager.ClientReconnectingState.Configure(connectionMethod);
            ConnectionManager.ChangeState(ConnectionManager.ClientConnectingState.Configure(connectionMethod));
        }
        
        public override void StartHostIP(string playerName, string ipaddress, int port)
        {
            var connectionMethod = new ConnectionMethodIP(ipaddress, (ushort)port, ConnectionManager,
                _profileManager, playerName);
            
            ConnectionManager.ChangeState(ConnectionManager.StartingHostState.Configure(connectionMethod));
        }

        public override void StartHostLobby(string playerName)
        {
            var connectionMethod = new ConnectionMethodRelay(ConnectionManager,_profileManager, playerName,
                _lobbyServiceFacade, _localLobby);
            
            ConnectionManager.ChangeState(ConnectionManager.StartingHostState.Configure(connectionMethod));
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void TryLoadMainMenu()
        {
            string activeSceneName = SceneManager.GetActiveScene().name;
            bool isCurrentSceneMainMenu = string.Equals(activeSceneName, SceneName.MainMenu.ToString());

            if (isCurrentSceneMainMenu)
                return;
            
            ConnectionManager.EnterLoadMainMenuState();
        }
    }
}