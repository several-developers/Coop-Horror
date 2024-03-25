using System.Text;
using GameCore.Enums.Global;
using GameCore.Gameplay.Network.Other;
using GameCore.Gameplay.Network.Session_Manager;
using GameCore.Gameplay.Network.UnityServices.Lobbies;
using GameCore.Gameplay.PubSub;
using GameCore.Infrastructure.StateMachine;
using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.Network.ConnectionManagement
{
    internal class HostingState : OnlineState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        public HostingState(ConnectionManager connectionManager, IPublisher<ConnectStatus> connectStatusPublisher,
            IPublisher<ConnectionEventMessage> connectionEventPublisher, IGameStateMachine gameStateMachine,
            LobbyServiceFacade lobbyServiceFacade) : base(connectionManager, connectStatusPublisher)
        {
            _connectionEventPublisher = connectionEventPublisher;
            _gameStateMachine = gameStateMachine;
            _lobbyServiceFacade = lobbyServiceFacade;
        }

        // FIELDS: --------------------------------------------------------------------------------

        // Used in ApprovalCheck. This is intended as a bit of light protection against DOS attacks
        // that rely on sending silly big buffers of garbage.
        private const int MaxConnectPayload = 1024;

        private readonly IPublisher<ConnectionEventMessage> _connectionEventPublisher;
        private readonly IGameStateMachine _gameStateMachine;
        private readonly LobbyServiceFacade _lobbyServiceFacade;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void Enter()
        {
            //_gameStateMachine.ChangeState<LoadGameplayState>();
            
            ConnectionManager.AddOnSceneEventCallback();

            //The "BossRoom" server always advances to CharSelect immediately on start. Different games
            //may do this differently.
            ConnectionManager.LoadScene(SceneName.Gameplay, isNetwork: true);
            
            if (_lobbyServiceFacade.CurrentUnityLobby != null)
                _lobbyServiceFacade.BeginTracking();
        }

        public override void Exit() =>
            SessionManager<SessionPlayerData>.Instance.OnServerEnded();

        public override void OnClientConnected(ulong clientId)
        {
            var message = new ConnectionEventMessage
            {
                ConnectStatus = ConnectStatus.Success,
                PlayerName = SessionManager<SessionPlayerData>.Instance.GetPlayerData(clientId)?.PlayerName
            };
            
            _connectionEventPublisher.Publish(message);
        }

        public override void OnClientDisconnect(ulong clientId)
        {
            if (clientId == ConnectionManager.NetworkManager.LocalClientId)
            {
                ConnectionManager.ChangeState(ConnectionManager.OfflineState);
            }
            else
            {
                string playerId = SessionManager<SessionPlayerData>.Instance.GetPlayerId(clientId);

                if (playerId == null)
                    return;
                
                var sessionData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(playerId);
                
                if (sessionData.HasValue)
                {
                    var message = new ConnectionEventMessage
                    {
                        ConnectStatus = ConnectStatus.GenericDisconnect,
                        PlayerName = sessionData.Value.PlayerName
                    };
                    
                    _connectionEventPublisher.Publish(message);
                }
                
                SessionManager<SessionPlayerData>.Instance.DisconnectClient(clientId);
            }
        }
        
        public override void OnUserRequestedShutdown()
        {
            string reason = JsonUtility.ToJson(ConnectStatus.HostEndedSession);
            
            for (int i = ConnectionManager.NetworkManager.ConnectedClientsIds.Count - 1; i >= 0; i--)
            {
                ulong id = ConnectionManager.NetworkManager.ConnectedClientsIds[i];
                
                if (id != ConnectionManager.NetworkManager.LocalClientId)
                    ConnectionManager.NetworkManager.DisconnectClient(id, reason);
            }
            
            ConnectionManager.ChangeState(ConnectionManager.OfflineState);
        }
        
        /// <summary>
        /// This logic plugs into the "ConnectionApprovalResponse" exposed by Netcode.NetworkManager.
        /// It is run every time a client connects to us.
        /// The complementary logic that runs when the client starts its connection can be found
        /// in ClientConnectingState.
        /// </summary>
        /// <remarks>
        /// Multiple things can be done here, some asynchronously.
        /// For example, it could authenticate your user against an auth service like UGS' auth service.
        /// It can also send custom messages to connecting users before they receive their connection result
        /// (this is useful to set status messages client side when connection is refused, for example).
        /// Note on authentication: It's usually harder to justify having authentication in a client hosted game's
        /// connection approval. Since the host can't be trusted, clients shouldn't send it
        /// private authentication tokens you'd usually send to a dedicated server.
        /// </remarks>
        /// <param name="request"> The initial request contains, among other things,
        /// binary data passed into StartClient. In our case, this is the client's GUID,
        /// which is a unique identifier for their install of the game that persists across app restarts.
        /// <param name="response"> Our response to the approval process.
        /// In case of connection refusal with custom return message, we delay using the Pending field.
        public override void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request,
            NetworkManager.ConnectionApprovalResponse response)
        {
            byte[] connectionData = request.Payload;
            ulong clientId = request.ClientNetworkId;
            
            if (connectionData.Length > MaxConnectPayload)
            {
                // If connectionData too high, deny immediately to avoid wasting time on the server. This is intended as
                // a bit of light protection against DOS attacks that rely on sending silly big buffers of garbage.
                response.Approved = false;
                return;
            }

            string payload = Encoding.UTF8.GetString(connectionData);
            
            // https://docs.unity3d.com/2020.2/Documentation/Manual/JSONSerialization.html
            var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);
            ConnectStatus gameReturnStatus = GetConnectStatus(connectionPayload);

            if (gameReturnStatus == ConnectStatus.Success)
            {
                SessionPlayerData sessionPlayerData = new(clientId, connectionPayload.playerName, new NetworkGuid(),
                    currentHitPoints: 0, isConnected: true);
                
                SessionManager<SessionPlayerData>.Instance.SetupConnectingPlayerSessionData(clientId,
                    connectionPayload.playerId, sessionPlayerData);

                // connection approval will create a player object for you
                response.Approved = true;
                response.CreatePlayerObject = true;
                response.Position = Vector3.zero;
                response.Rotation = Quaternion.identity;
                return;
            }

            response.Approved = false;
            response.Reason = JsonUtility.ToJson(gameReturnStatus);
            
            if (_lobbyServiceFacade.CurrentUnityLobby != null)
            {
                _lobbyServiceFacade.RemovePlayerFromLobbyAsync(connectionPayload.playerId,
                    _lobbyServiceFacade.CurrentUnityLobby.Id);
            }
        }
        
        private ConnectStatus GetConnectStatus(ConnectionPayload connectionPayload)
        {
            if (ConnectionManager.NetworkManager.ConnectedClientsIds.Count >= ConnectionManager.MaxConnectedPlayers)
                return ConnectStatus.ServerFull;

            if (connectionPayload.isDebug != Debug.isDebugBuild)
                return ConnectStatus.IncompatibleBuildType;

            return SessionManager<SessionPlayerData>.Instance.IsDuplicateConnection(connectionPayload.playerId) ?
                ConnectStatus.LoggedInAgain : ConnectStatus.Success;
        }
    }
}