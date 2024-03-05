using System.Collections;
using Cysharp.Threading.Tasks;
using GameCore.Enums.Global;
using GameCore.Gameplay.Network.UnityServices.Lobbies;
using GameCore.Gameplay.PubSub;
using UnityEngine;

namespace GameCore.Gameplay.Network.ConnectionManagement
{
    /// <summary>
    /// Connection state corresponding to a client attempting to reconnect to a server. It will try to reconnect a
    /// number of times defined by the ConnectionManager's NbReconnectAttempts property. If it succeeds, it will
    /// transition to the ClientConnected state. If not, it will transition to the Offline state. If given a disconnect
    /// reason first, depending on the reason given, may not try to reconnect again and transition directly to the
    /// Offline state.
    /// </summary>
    internal class ClientReconnectingState : ClientConnectingState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ClientReconnectingState(ConnectionManager connectionManager,
            IPublisher<ConnectStatus> connectStatusPublisher, LobbyServiceFacade lobbyServiceFacade,
            LocalLobby localLobby, IPublisher<ReconnectMessage> reconnectMessagePublisher)
            : base(connectionManager, connectStatusPublisher, lobbyServiceFacade, localLobby)
        {
            _reconnectMessagePublisher = reconnectMessagePublisher;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private const float TimeBetweenAttempts = 5;

        private readonly IPublisher<ReconnectMessage> _reconnectMessagePublisher;

        private Coroutine _reconnectCoroutine;
        private string _lobbyCode = "";
        private int _attempts;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void Enter()
        {
            _attempts = 0;

            _lobbyCode = LobbyServiceFacade.CurrentUnityLobby != null
                ? LobbyServiceFacade.CurrentUnityLobby.LobbyCode
                : "";

            _reconnectCoroutine = ConnectionManager.StartCoroutine(ReconnectCoroutine());
        }

        public override void Exit()
        {
            if (_reconnectCoroutine != null)
            {
                ConnectionManager.StopCoroutine(_reconnectCoroutine);
                _reconnectCoroutine = null;
            }

            ReconnectMessage message = new(currentAttempt: ConnectionManager.ReconnectAttempts,
                maxAttempt: ConnectionManager.ReconnectAttempts);

            _reconnectMessagePublisher.Publish(message);
        }

        public override void OnClientConnected(ulong _) =>
            ConnectionManager.ChangeState(ConnectionManager.ClientConnectedState);

        public override void OnClientDisconnect(ulong _)
        {
            string disconnectReason = ConnectionManager.NetworkManager.DisconnectReason;

            if (_attempts < ConnectionManager.ReconnectAttempts)
            {
                if (string.IsNullOrEmpty(disconnectReason))
                {
                    _reconnectCoroutine = ConnectionManager.StartCoroutine(ReconnectCoroutine());
                }
                else
                {
                    var connectStatus = JsonUtility.FromJson<ConnectStatus>(disconnectReason);
                    ConnectStatusPublisher.Publish(connectStatus);

                    switch (connectStatus)
                    {
                        case ConnectStatus.UserRequestedDisconnect:
                        case ConnectStatus.HostEndedSession:
                        case ConnectStatus.ServerFull:
                        case ConnectStatus.IncompatibleBuildType:
                            ConnectionManager.ChangeState(ConnectionManager.OfflineState);
                            break;

                        default:
                            _reconnectCoroutine = ConnectionManager.StartCoroutine(ReconnectCoroutine());
                            break;
                    }
                }
            }
            else
            {
                if (string.IsNullOrEmpty(disconnectReason))
                {
                    ConnectStatusPublisher.Publish(message: ConnectStatus.GenericDisconnect);
                }
                else
                {
                    var connectStatus = JsonUtility.FromJson<ConnectStatus>(disconnectReason);
                    ConnectStatusPublisher.Publish(connectStatus);
                }

                ConnectionManager.ChangeState(ConnectionManager.OfflineState);
            }
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private IEnumerator ReconnectCoroutine()
        {
            // If not on first attempt, wait some time before trying again, so that if the issue causing the disconnect
            // is temporary, it has time to fix itself before we try again. Here we are using a simple fixed cooldown
            // but we could want to use exponential backoff instead, to wait a longer time between each failed attempt.
            // See https://en.wikipedia.org/wiki/Exponential_backoff
            if (_attempts > 0)
                yield return new WaitForSeconds(TimeBetweenAttempts);

            Debug.Log("Lost connection to host, trying to reconnect...");

            ConnectionManager.NetworkManager.Shutdown();

            // Wait until NetworkManager completes shutting down.
            yield return new WaitWhile(() => ConnectionManager.NetworkManager.ShutdownInProgress);

            Debug.Log($"Reconnecting attempt {_attempts + 1}/{ConnectionManager.ReconnectAttempts}...");
            
            ReconnectMessage message = new(currentAttempt: _attempts,
                maxAttempt: ConnectionManager.ReconnectAttempts);
            
            _reconnectMessagePublisher.Publish(message);

            _attempts++;

            if (!string.IsNullOrEmpty(_lobbyCode)) // Attempting to reconnect to lobby.
            {
                // When using Lobby with Relay, if a user is disconnected from the Relay server, the server will notify
                // the Lobby service and mark the user as disconnected, but will not remove them from the lobby. They
                // then have some time to attempt to reconnect (defined by the "Disconnect removal time" parameter on
                // the dashboard), after which they will be removed from the lobby completely.
                // See https://docs.unity.com/lobby/reconnect-to-lobby.html
                var reconnectingToLobby = LobbyServiceFacade.ReconnectToLobbyAsync(LocalLobby?.LobbyID);

                yield return new WaitUntil(() => reconnectingToLobby.IsCompleted);

                // If succeeded, attempt to connect to Relay
                if (!reconnectingToLobby.IsFaulted && reconnectingToLobby.Result != null)
                {
                    // If this fails, the OnClientDisconnect callback will be invoked by Netcode
                    UniTask connectingToRelay = ConnectClientAsync();
                    UniTask.Awaiter awaiter = connectingToRelay.GetAwaiter();

                    yield return new WaitUntil(() => awaiter.IsCompleted);
                }
                else
                {
                    Debug.Log("Failed reconnecting to lobby.");
                    // Calling OnClientDisconnect to mark this attempt as failed and either start a new one or give up
                    // and return to the Offline state
                    OnClientDisconnect(0);
                }
            }
            else // If not using Lobby, simply try to reconnect to the server directly
            {
                // If this fails, the OnClientDisconnect callback will be invoked by Netcode
                UniTask connectingClient = ConnectClientAsync();
                UniTask.Awaiter awaiter = connectingClient.GetAwaiter();

                yield return new WaitUntil(() => awaiter.IsCompleted);
            }
        }
    }
}