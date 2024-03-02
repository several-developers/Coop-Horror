﻿using Cysharp.Threading.Tasks;
using GameCore.Gameplay.Network.UnityServices.Lobbies;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Network.ConnectionManagement
{
    public class ConnectionManager : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(LobbyServiceFacade lobbyServiceFacade, ProfileManager profileManager,
            LocalLobby localLobby, LocalLobbyUser lobbyUser)
        {
            _lobbyServiceFacade = lobbyServiceFacade;
            _profileManager = profileManager;
            _localLobby = localLobby;
            _lobbyUser = lobbyUser;
        }
        
        // PROPERTIES: ----------------------------------------------------------------------------
        
        public NetworkManager NetworkManager { get; private set; }
        public int ReconnectAttempts => MaxReconnectAttempts; 
        public int MaxConnectedPlayers { get; } = 4;

        // FIELDS: --------------------------------------------------------------------------------
        
        internal OfflineState OfflineState;
        internal ClientConnectingState ClientConnectingState;
        internal ClientConnectedState ClientConnectedState;
        internal ClientReconnectingState ClientReconnectingState;
        internal StartingHostState StartingHostState;
        internal HostingState HostingState;
        
        private const int MaxReconnectAttempts = 2; // TEMP
        
        private static ConnectionManager _instance; 
        
        private LobbyServiceFacade _lobbyServiceFacade;
        private ProfileManager _profileManager;
        private LocalLobby _localLobby;
        private LocalLobbyUser _lobbyUser;
        private ConnectionState _currentState;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _instance = this;
            
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            SetupStates();
            GetNetworkManager();
        }

        private void OnDestroy()
        {
            NetworkManager.OnClientConnectedCallback -= OnClientConnectedCallback;
            NetworkManager.OnClientDisconnectCallback -= OnClientDisconnectCallback;
            NetworkManager.OnServerStarted -= OnServerStarted;
            NetworkManager.ConnectionApprovalCallback -= OnApprovalCheck;
            NetworkManager.OnTransportFailure -= OnTransportFailure;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        internal void ChangeState(ConnectionState nextState)
        {
            Debug.Log($"{name}: Changed connection state from {_currentState.GetType().Name} to " +
                      $"{nextState.GetType().Name}.");

            _currentState?.Exit();
            _currentState = nextState;
            _currentState.Enter();
        }

        public static ConnectionManager Get() => _instance;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async void GetNetworkManager()
        {
            int iterations = 0;
            
            while (NetworkManager.Singleton == null)
            {
                iterations++;

                if (iterations > 600)
                {
                    Debug.LogError("Infinity loop! Network Manager not found!");
                    break;
                }
                
                await UniTask.DelayFrame(1);
            }
            
            NetworkManager = NetworkManager.Singleton;
            
            NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnectCallback;
            NetworkManager.OnServerStarted += OnServerStarted;
            NetworkManager.ConnectionApprovalCallback += OnApprovalCheck;
            NetworkManager.OnTransportFailure += OnTransportFailure;
        }

        private void SetupStates()
        {
            OfflineState = new OfflineState(connectionManager: this, _profileManager, _lobbyServiceFacade, _localLobby);
            
            ClientConnectingState = new ClientConnectingState(connectionManager: this, _lobbyServiceFacade,
                _localLobby);

            ClientConnectedState = new ClientConnectedState(connectionManager: this);
            
            ClientReconnectingState = new ClientReconnectingState(connectionManager: this, _lobbyServiceFacade,
                _localLobby);
            
            StartingHostState = new StartingHostState(connectionManager: this, _localLobby);
            HostingState = new HostingState(connectionManager: this, _lobbyServiceFacade);

            _currentState = OfflineState;
        }
        
        [Button(30, ButtonStyle.FoldoutButton)]
        public void StartClientLobby(string playerName) =>
            _currentState.StartClientLobby(playerName);

        [Button(30, ButtonStyle.FoldoutButton)]
        public void StartClientIp(string playerName, string ipaddress, int port) =>
            _currentState.StartClientIP(playerName, ipaddress, port);

        [Button(30, ButtonStyle.FoldoutButton)]
        public void StartHostLobby(string playerName) =>
            _currentState.StartHostLobby(playerName);

        [Button(30, ButtonStyle.FoldoutButton)]
        public void StartHostIp(string playerName, string ipaddress, int port) =>
            _currentState.StartHostIP(playerName, ipaddress, port);

        [Button(30, ButtonStyle.FoldoutButton)]
        public void RequestShutdown() =>
            _currentState.OnUserRequestedShutdown();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnClientConnectedCallback(ulong clientId) =>
            _currentState.OnClientConnected(clientId);

        private void OnClientDisconnectCallback(ulong clientId) =>
            _currentState.OnClientDisconnect(clientId);

        private void OnServerStarted() =>
            _currentState.OnServerStarted();

        private void OnApprovalCheck(NetworkManager.ConnectionApprovalRequest request,
            NetworkManager.ConnectionApprovalResponse response)
        {
            _currentState.ApprovalCheck(request, response);
        }

        private void OnTransportFailure() =>
            _currentState.OnTransportFailure();
    }
}