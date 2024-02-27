using GameCore.Enums.Global;
using UnityEngine;

namespace GameCore.Gameplay.Network.ConnectionManagement
{
    internal class ClientConnectedState : ConnectionState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        public ClientConnectedState(ConnectionManager connectionManager) : base(connectionManager)
        {
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void Enter()
        {
            //if (m_LobbyServiceFacade.CurrentUnityLobby != null)
                //m_LobbyServiceFacade.BeginTracking();
        }

        public override void Exit() { }

        public override void OnClientDisconnect(ulong _)
        {
            string disconnectReason = ConnectionManager.NetworkManager.DisconnectReason;
            
            if (string.IsNullOrEmpty(disconnectReason))
            {
                //m_ConnectStatusPublisher.Publish(ConnectStatus.Reconnecting);
                ConnectionManager.ChangeState(ConnectionManager.ClientReconnectingState);
            }
            else
            {
                var connectStatus = JsonUtility.FromJson<ConnectStatus>(disconnectReason);
                //m_ConnectStatusPublisher.Publish(connectStatus);
                ConnectionManager.ChangeState(ConnectionManager.OfflineState);
            }
        }

        public override void OnUserRequestedShutdown()
        {
            //m_ConnectStatusPublisher.Publish(ConnectStatus.UserRequestedDisconnect);
            ConnectionManager.ChangeState(ConnectionManager.OfflineState);
        }
    }
}