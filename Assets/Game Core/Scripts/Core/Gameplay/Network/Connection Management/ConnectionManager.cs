using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using GameCore.Enums.Global;
using GameCore.Gameplay.Network.DynamicPrefabs;
using GameCore.Gameplay.Network.UnityServices.Lobbies;
using GameCore.Gameplay.PubSub;
using GameCore.Gameplay.Storages.Assets;
using GameCore.Infrastructure.Services.Global;
using GameCore.StateMachine;
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
        private void Construct(
            IPublisher<ConnectStatus> connectStatusPublisher,
            IPublisher<ReconnectMessage> reconnectMessagePublisher,
            IPublisher<ConnectionEventMessage> connectionEventPublisher,
            LobbyServiceFacade lobbyServiceFacade,
            ProfileManager profileManager,
            LocalLobby localLobby,
            LocalLobbyUser lobbyUser,
            IScenesLoaderService scenesLoaderService,
            IScenesAssetsStorage scenesAssetsStorage,
            IGameStateMachine gameStateMachine
        )
        {
            _connectStatusPublisher = connectStatusPublisher;
            _reconnectMessagePublisher = reconnectMessagePublisher;
            _connectionEventPublisher = connectionEventPublisher;
            _lobbyServiceFacade = lobbyServiceFacade;
            _profileManager = profileManager;
            _localLobby = localLobby;
            _lobbyUser = lobbyUser;
            _scenesLoaderService = scenesLoaderService;
            _scenesAssetsStorage = scenesAssetsStorage;
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

        // Used in ApprovalCheck. This is intended as a bit of light protection against DOS attacks
        // that rely on sending silly big buffers of garbage.
        public static readonly int MaxConnectPayload = 1024;

        private const int MaxReconnectAttempts = 0; // TEMP

        private static ConnectionManager _instance;

        private IPublisher<ConnectStatus> _connectStatusPublisher;
        private IPublisher<ReconnectMessage> _reconnectMessagePublisher;
        private IPublisher<ConnectionEventMessage> _connectionEventPublisher;
        private LobbyServiceFacade _lobbyServiceFacade;
        private ProfileManager _profileManager;
        private LocalLobby _localLobby;
        private LocalLobbyUser _lobbyUser;
        private ConnectionState _currentState;
        private IScenesLoaderService _scenesLoaderService;
        private IScenesAssetsStorage _scenesAssetsStorage;
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
            GetNetworkManager().Forget();
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

        public void AddLocationsScenes()
        {
            NetworkSceneManager sceneManager = NetworkManager.SceneManager;
            sceneManager.SceneManagerHandler = new AddressablesSceneManagerHandler();

            IEnumerable<string> allScenesPath = _scenesAssetsStorage.GetAllScenesPath();
            string[] scenesPaths = allScenesPath.ToArray();
            
            sceneManager.RegisterExternalScenes(scenesPaths);
        }
        
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

        public IPublisher<ConnectStatus> GetConnectStatusPublisher() => _connectStatusPublisher;

        public LobbyServiceFacade GetLobbyServiceFacade() => _lobbyServiceFacade;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async UniTaskVoid GetNetworkManager()
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
            OfflineState = new OfflineState(connectionManager: this, _profileManager, _localLobby);
            ClientConnectingState = new ClientConnectingState(connectionManager: this, _localLobby);
            ClientConnectedState = new ClientConnectedState(connectionManager: this);

            ClientReconnectingState = new ClientReconnectingState(connectionManager: this, _localLobby,
                _reconnectMessagePublisher);

            StartingHostState = new StartingHostState(connectionManager: this, _localLobby);
            HostingState = new HostingState(connectionManager: this, _connectionEventPublisher);

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

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private bool FirstStepApproval(NetworkManager.ConnectionApprovalRequest request,
            NetworkManager.ConnectionApprovalResponse response)
        {
            byte[] connectionData = request.Payload;
            ulong clientID = request.ClientNetworkId;

            // Allow the host to connect.
            if (clientID == NetworkManager.LocalClientId)
                return true;

            // A sample-specific denial on clients after k_MaxConnectedClientCount clients have been connected.
            if (NetworkManager.ConnectedClientsList.Count >= MaxConnectedPlayers)
                return false;

            // If connectionData is too big, deny immediately to avoid wasting time on the server. This is intended
            // as a bit of light protection against DOS attacks that rely on sending silly big buffers of garbage.
            if (connectionData.Length > MaxConnectPayload)
                return false;

            // Immediately approve the connection if we haven't loaded any prefabs yet.
            if (DynamicPrefabLoadingUtilities.LoadedPrefabCount == 0)
                return true;

            string payload = Encoding.UTF8.GetString(connectionData);

            // https://docs.unity3d.com/2020.2/Documentation/Manual/JSONSerialization.html
            var connectionPayload =
                JsonUtility.FromJson<DynamicPrefabs.ConnectionPayload>(payload);

            int clientPrefabHash = connectionPayload.hashOfDynamicPrefabGUIDs;
            int serverPrefabHash = DynamicPrefabLoadingUtilities.HashOfDynamicPrefabGUIDs;

            // If the client has the same prefabs as the server - approve the connection.
            if (clientPrefabHash == serverPrefabHash)
            {
                // Approve();
                DynamicPrefabLoadingUtilities.RecordThatClientHasLoadedAllPrefabs(clientID);
                return true;
            }

            // In order for clients to not just get disconnected with no feedback, the server needs to tell the client
            // why it disconnected it. This could happen after an auth check on a service or because of gameplay
            // reasons (server full, wrong build version, etc).
            // The server can do so via the DisconnectReason in the ConnectionApprovalResponse. The guids of the prefabs
            // the client will need to load will be sent, such that the client loads the needed prefabs, and reconnects.

            // A note: DisconnectReason will not be written to if the string is too large in size. This should be used
            // only to tell the client "why" it failed -- the client should instead use services like UGS to fetch the
            // relevant data it needs to fetch & download.

            DynamicPrefabLoadingUtilities.RefreshLoadedPrefabGuids();

            response.Reason = DynamicPrefabLoadingUtilities.GenerateDisconnectionPayload();

            return false;
        }

        private bool SecondStepApproval(NetworkManager.ConnectionApprovalRequest request,
            NetworkManager.ConnectionApprovalResponse response)
        {
            return _currentState.ApprovalCheck(request, response);
        }

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
            ulong clientID = request.ClientNetworkId;
            bool isFirstStepApproved = FirstStepApproval(request, response);
            bool isSecondStepApproved = SecondStepApproval(request, response);
            bool isApproved = isFirstStepApproved && isSecondStepApproved;
            
            if (isApproved)
                Approve();
            else
                ImmediateDeny();

            // LOCAL METHODS: -----------------------------

            // A note: sending large strings through Netcode is not ideal -- you'd usually want to use REST services to
            // accomplish this instead. UGS services like Lobby can be a useful alternative. Another route may be to
            // set ConnectionApprovalResponse's Pending flag to true, and send a CustomMessage containing the array of 
            // GUIDs to a client, which the client would load and reattempt a reconnection.

            void Approve()
            {
                Debug.Log(message: $"Client {clientID} approved");

                response.Approved = true;
                response.CreatePlayerObject = true; // We're not going to spawn a player object for this sample.
            }

            void ImmediateDeny()
            {
                Debug.Log(message: $"Client {clientID} denied connection");

                response.Approved = false;
                response.CreatePlayerObject = false;
            }
        }

        private void OnTransportFailure() =>
            _currentState.OnTransportFailure();
    }
}