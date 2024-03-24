using System;
using Cysharp.Threading.Tasks;
using GameCore.Gameplay.Network.UnityServices.Lobbies;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace GameCore.Gameplay.Network.ConnectionManagement
{
    public class ConnectionMethodRelay : ConnectionMethodBase
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ConnectionMethodRelay(ConnectionManager connectionManager, ProfileManager profileManager,
            string playerName, LobbyServiceFacade lobbyServiceFacade, LocalLobby localLobby)
            : base(connectionManager, profileManager, playerName)
        {
            _lobbyServiceFacade = lobbyServiceFacade;
            _localLobby = localLobby;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly LobbyServiceFacade _lobbyServiceFacade;
        private readonly LocalLobby _localLobby;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override async UniTask SetupClientConnectionAsync()
        {
            Debug.Log("Setting up Unity Relay client");

            SetConnectionPayload(GetPlayerId(), PlayerName);

            if (_lobbyServiceFacade.CurrentUnityLobby == null)
            {
                throw new Exception("Trying to start relay while Lobby isn't set");
            }

            Debug.Log($"Setting Unity Relay client with join code {_localLobby.RelayJoinCode}");

            // Create client joining allocation from join code
            var joinedAllocation = await RelayService.Instance.JoinAllocationAsync(_localLobby.RelayJoinCode);
            Debug.Log($"client: {joinedAllocation.ConnectionData[0]} {joinedAllocation.ConnectionData[1]}, " +
                $"host: {joinedAllocation.HostConnectionData[0]} {joinedAllocation.HostConnectionData[1]}, " +
                $"client: {joinedAllocation.AllocationId}");

            await _lobbyServiceFacade
                .UpdatePlayerRelayInfoAsync(joinedAllocation.AllocationId.ToString(), _localLobby.RelayJoinCode);

            // Configure UTP with allocation
            var utp = (UnityTransport)ConnectionManager.NetworkManager.NetworkConfig.NetworkTransport;
            utp.SetRelayServerData(new RelayServerData(joinedAllocation, OnlineState.DtlsConnType));
        }

        public override async UniTask SetupHostConnectionAsync()
        {
            Debug.Log("Setting up Unity Relay host");

            SetConnectionPayload(GetPlayerId(), PlayerName); // Need to set connection payload for host as well, as host is a client too

            // Create relay allocation
            Allocation hostAllocation = await RelayService.Instance
                .CreateAllocationAsync(ConnectionManager.MaxConnectedPlayers, region: null);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(hostAllocation.AllocationId);

            Debug.Log($"server: connection data: {hostAllocation.ConnectionData[0]} {hostAllocation.ConnectionData[1]}, " +
                $"allocation ID:{hostAllocation.AllocationId}, region:{hostAllocation.Region}");

            _localLobby.RelayJoinCode = joinCode;

            //next line enable lobby and relay services integration
            await _lobbyServiceFacade.UpdateLobbyDataAsync(_localLobby.GetDataForUnityServices());
            await _lobbyServiceFacade.UpdatePlayerRelayInfoAsync(hostAllocation.AllocationIdBytes.ToString(), joinCode);

            // Setup UTP with relay connection info
            var utp = (UnityTransport)ConnectionManager.NetworkManager.NetworkConfig.NetworkTransport;
            // This is with DTLS enabled for a secure connection
            utp.SetRelayServerData(new RelayServerData(hostAllocation, OnlineState.DtlsConnType));
        }
    }
}