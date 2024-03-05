using GameCore.Enums.Global;
using GameCore.Gameplay.Network.UnityServices.Lobbies;
using GameCore.Gameplay.PubSub;
using UnityEngine;

namespace GameCore.Gameplay.Network.ConnectionManagement
{
    internal class ClientConnectedState : ConnectionState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        public ClientConnectedState(ConnectionManager connectionManager,
            IPublisher<ConnectStatus> connectStatusPublisher, LobbyServiceFacade lobbyServiceFacade)
            : base(connectionManager, connectStatusPublisher)
        {
            _lobbyServiceFacade = lobbyServiceFacade;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly LobbyServiceFacade _lobbyServiceFacade;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void Enter()
        {
            if (_lobbyServiceFacade.CurrentUnityLobby != null)
                _lobbyServiceFacade.BeginTracking();
        }

        public override void Exit() { }

        public override void OnClientDisconnect(ulong _)
        {
            string disconnectReason = ConnectionManager.NetworkManager.DisconnectReason;
            
            if (string.IsNullOrEmpty(disconnectReason))
            {
                ConnectStatusPublisher.Publish(message: ConnectStatus.Reconnecting);
                ConnectionManager.ChangeState(ConnectionManager.ClientReconnectingState);
            }
            else
            {
                var connectStatus = JsonUtility.FromJson<ConnectStatus>(disconnectReason);
                ConnectStatusPublisher.Publish(message: connectStatus);
                ConnectionManager.ChangeState(ConnectionManager.OfflineState);
            }
        }

        public override void OnUserRequestedShutdown()
        {
            ConnectStatusPublisher.Publish(message: ConnectStatus.UserRequestedDisconnect);
            ConnectionManager.ChangeState(ConnectionManager.OfflineState);
        }
    }
}