using System;
using GameCore.Enums.Global;
using GameCore.Gameplay.Network.Other;
using GameCore.Gameplay.Network.Session_Manager;
using GameCore.Gameplay.Network.UnityServices.Lobbies;
using GameCore.Gameplay.PubSub;
using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.Network.ConnectionManagement
{
    /// <summary>
    /// Connection state corresponding to a host starting up. Starts the host when entering the state. If successful,
    /// transitions to the Hosting state, if not, transitions back to the Offline state.
    /// </summary>
    internal class StartingHostState : OnlineState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public StartingHostState(ConnectionManager connectionManager, IPublisher<ConnectStatus> connectStatusPublisher,
            LocalLobby localLobby) : base(connectionManager, connectStatusPublisher)
        {
            _localLobby = localLobby;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly LocalLobby _localLobby;

        private ConnectionMethodBase _connectionMethod;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void Enter() => StartHost();

        public override void Exit()
        {
        }

        public override void OnClientDisconnect(ulong clientId)
        {
            if (clientId == ConnectionManager.NetworkManager.LocalClientId)
                StartHostFailed();
        }

        public override void OnServerStarted()
        {
            ConnectStatusPublisher.Publish(message: ConnectStatus.Success);
            ConnectionManager.ChangeState(ConnectionManager.HostingState);
        }

        public override void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request,
            NetworkManager.ConnectionApprovalResponse response)
        {
            byte[] connectionData = request.Payload;
            ulong clientId = request.ClientNetworkId;

            // This happens when starting as a host, before the end of the StartHost call.
            // In that case, we simply approve ourselves.
            if (clientId != ConnectionManager.NetworkManager.LocalClientId)
                return;

            string payload = System.Text.Encoding.UTF8.GetString(connectionData);

            // https://docs.unity3d.com/2020.2/Documentation/Manual/JSONSerialization.html
            var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);

            SessionPlayerData sessionPlayerData = new(clientId, connectionPayload.playerName, new NetworkGuid(),
                currentHitPoints: 0, isConnected: true);
            
            SessionManager<SessionPlayerData>.Instance
                .SetupConnectingPlayerSessionData(clientId, connectionPayload.playerId, sessionPlayerData);

            // connection approval will create a player object for you
            response.Approved = true;
            response.CreatePlayerObject = true;
        }

        public StartingHostState Configure(ConnectionMethodBase baseConnectionMethod)
        {
            _connectionMethod = baseConnectionMethod;
            return this;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async void StartHost()
        {
            try
            {
                await _connectionMethod.SetupHostConnectionAsync();
                Debug.Log($"Created relay allocation with join code {_localLobby.RelayJoinCode}");

                // NGO's StartHost launches everything
                if (!ConnectionManager.NetworkManager.StartHost())
                    OnClientDisconnect(ConnectionManager.NetworkManager.LocalClientId);
            }
            catch (Exception)
            {
                StartHostFailed();
                throw;
            }
        }

        private void StartHostFailed()
        {
            ConnectStatusPublisher.Publish(message: ConnectStatus.StartHostFailed);
            ConnectionManager.ChangeState(ConnectionManager.OfflineState);
        }
    }
}