using Cysharp.Threading.Tasks;
using GameCore.Enums.Global;
using GameCore.Gameplay.Network.UnityServices.Lobbies;
using GameCore.Gameplay.PubSub;
using GameCore.Infrastructure.Services.Global;
using GameCore.Infrastructure.StateMachine;
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
        private void Construct(IPublisher<ConnectStatus> connectStatusPublisher,
            IPublisher<ReconnectMessage> reconnectMessagePublisher,
            IPublisher<ConnectionEventMessage> connectionEventPublisher, LobbyServiceFacade lobbyServiceFacade,
            ProfileManager profileManager, LocalLobby localLobby, LocalLobbyUser lobbyUser,
            IScenesLoaderService2 scenesLoaderService, IGameStateMachine gameStateMachine)
        {
            _connectStatusPublisher = connectStatusPublisher;
            _reconnectMessagePublisher = reconnectMessagePublisher;
            _connectionEventPublisher = connectionEventPublisher;
            _lobbyServiceFacade = lobbyServiceFacade;
            _profileManager = profileManager;
            _localLobby = localLobby;
            _lobbyUser = lobbyUser;
            _scenesLoaderService = scenesLoaderService;
            _gameStateMachine = gameStateMachine;
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

        private IPublisher<ConnectStatus> _connectStatusPublisher;
        private IPublisher<ReconnectMessage> _reconnectMessagePublisher;
        private IPublisher<ConnectionEventMessage> _connectionEventPublisher;
        private LobbyServiceFacade _lobbyServiceFacade;
        private ProfileManager _profileManager;
        private LocalLobby _localLobby;
        private LocalLobbyUser _lobbyUser;
        private ConnectionState _currentState;
        private IScenesLoaderService2 _scenesLoaderService;
        private IGameStateMachine _gameStateMachine;

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

        public void AddOnSceneEventCallback() =>
            _scenesLoaderService.AddOnSceneEventCallback();

        public void LoadScene(SceneName sceneName, bool isNetwork) =>
            _scenesLoaderService.LoadScene(sceneName, isNetwork);

        public void EnterLoadMainMenuState() =>
            _gameStateMachine.ChangeState<LoadMainMenuState>();

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
            OfflineState = new OfflineState(connectionManager: this, _connectStatusPublisher, _profileManager,
                _lobbyServiceFacade, _localLobby);

            ClientConnectingState = new ClientConnectingState(connectionManager: this, _connectStatusPublisher,
                _lobbyServiceFacade, _localLobby);

            ClientConnectedState = new ClientConnectedState(connectionManager: this, _connectStatusPublisher,
                _lobbyServiceFacade);

            ClientReconnectingState = new ClientReconnectingState(connectionManager: this, _connectStatusPublisher,
                _lobbyServiceFacade, _localLobby, _reconnectMessagePublisher);

            StartingHostState = new StartingHostState(connectionManager: this, _connectStatusPublisher, _localLobby);
            
            HostingState = new HostingState(connectionManager: this, _connectStatusPublisher, _connectionEventPublisher,
                _gameStateMachine, _lobbyServiceFacade);

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