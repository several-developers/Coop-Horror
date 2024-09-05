using System;
using Cysharp.Threading.Tasks;
using GameCore.Enums.Global;
using GameCore.Gameplay.Network.UnityServices.Lobbies;
using UnityEngine;

namespace GameCore.Gameplay.Network.ConnectionManagement
{
    internal class ClientConnectingState : OnlineState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ClientConnectingState(ConnectionManager connectionManager, LocalLobby localLobby)
            : base(connectionManager)
        {
            LobbyServiceFacade = connectionManager.GetLobbyServiceFacade();
            LocalLobby = localLobby;
        }

        // FIELDS: --------------------------------------------------------------------------------

        protected readonly LobbyServiceFacade LobbyServiceFacade;
        protected readonly LocalLobby LocalLobby;

        private ConnectionMethodBase _connectionMethod;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void Enter()
        {
#pragma warning disable 4014
            ConnectClientAsync();
#pragma warning restore 4014
        }

        public override void Exit()
        {
        }

        public override void OnClientConnected(ulong _)
        {
            ConnectStatusPublisher.Publish(message: ConnectStatus.Success);
            ConnectionManager.ChangeState(ConnectionManager.ClientConnectedState);
        }

        public override void OnClientDisconnect(ulong _)
        {
            // Client ID is for sure ours here.
            StartingClientFailedAsync();
        }

        public ClientConnectingState Configure(ConnectionMethodBase baseConnectionMethod)
        {
            _connectionMethod = baseConnectionMethod;
            return this;
        }

        internal async UniTask ConnectClientAsync()
        {
            try
            {
                // Setup NGO with current connection method
                await _connectionMethod.SetupClientConnectionAsync();

                // NGO's StartClient launches everything
                if (!ConnectionManager.NetworkManager.StartClient())
                    throw new Exception("NetworkManager StartClient failed");

                ConnectionManager.AddOnSceneEventCallback();
            }
            catch (Exception e)
            {
                Debug.LogError("Error connecting client, see following exception");
                Debug.LogException(e);
                StartingClientFailedAsync();
                throw;
            }
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void StartingClientFailedAsync()
        {
            string disconnectReason = ConnectionManager.NetworkManager.DisconnectReason;

            if (string.IsNullOrEmpty(disconnectReason))
            {
                ConnectStatusPublisher.Publish(message: ConnectStatus.StartClientFailed);
            }
            else
            {
                var connectStatus = JsonUtility.FromJson<ConnectStatus>(disconnectReason);
                ConnectStatusPublisher.Publish(connectStatus);
            }

            ConnectionManager.ChangeState(ConnectionManager.OfflineState);
        }
    }
}