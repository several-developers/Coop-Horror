using System.Collections.Generic;
using GameCore.Utilities;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace GameCore.Gameplay.NetworkDepricated2
{
    public enum ClientState
    {
        Offline = 0, // Not connected
        Connecting = 5, // Waiting to change scene or receive world data
        Ready = 10, // Everything is loaded and spawned
    }
    
    public enum ServerType
    {
        None = 0,
        DedicatedServer = 10,
        RelayServer = 20,
        PeerToPeer = 30, // Requires Port Forwarding for the host
    }

    public class TheNetworkHorrorDepricated2 : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        public GameObject _playerPrefab; // TEMP
        
        // PROPERTIES: ----------------------------------------------------------------------------
        
        private static ulong ServerID => NetworkManager.ServerClientId; // ID of the server
        
        private bool IsServer => _offlineMode || _networkManager.IsServer;
        private bool IsHost => IsClient && IsServer; // Host is both a client and server
        private bool IsClient => _offlineMode || _networkManager.IsClient;
        
        //ID of this client (if host, will be same than ServerID), changes for every reconnection, assigned by Netcode
        private ulong ClientID => _offlineMode ? ServerID : _networkManager.LocalClientId;
        
        // FIELDS: --------------------------------------------------------------------------------

        public const int MsgSize = 1024 * 1024;

        // Server & Client events
        public event UnityAction OnTickEvent; // Every network tick
        public event UnityAction OnReadyEvent; // Event after connection fully established (save file sent, scene loaded...)
        public event UnityAction OnConnectEvent; // Event when self connect, happens before onReady, before sending any data
        public event UnityAction OnDisconnectEvent; // Event when self disconnect
        public event UnityAction<string> OnBeforeChangeSceneEvent; // Before Changing Scene
        public event UnityAction<string> OnAfterChangeSceneEvent; // After Changing Scene

        // Server only events
        public event UnityAction<ulong> OnClientJoinEvent; // Server event when any client connect
        public event UnityAction<ulong> OnClientQuitEvent; // Server event when any client disconnect
        public event UnityAction<ulong> OnClientReadyEvent; // Server event when any client become ready
        public event UnityAction<string> OnSaveRequestEvent; // Server event when a player asks to save
        
        public UnityAction<ulong, ConnectionData> OnClientApprovedEvent; //Called when a new client was succesfully approved
        public UnityAction<ulong, int> OnAssignPlayerIDEvent; // Server event after assigning id (client_id, player_id)
        
        public delegate bool ApprovalEvent(ulong clientID, ConnectionData connectData);
        public ApprovalEvent OnCheckApprovalEvent; // Additional approval validations for when a client connects
        
        public delegate bool ReadyCheckEvent();
        public ReadyCheckEvent OnCheckReadyEvent; // Additional optional ready validations
        
        public delegate int IntEvent(ulong clientID);
        public IntEvent OnFindPlayerIDEvent; // Find player ID for client

        private const string ListenAll = "0.0.0.0";
        private const float SlowUpdateRefreshDelay = 1f;
        
        private readonly Dictionary<ulong, ClientData> _clientList = new();
        
        private static TheNetworkHorrorDepricated2 _instance;
        
        private NetworkManager _networkManager;
        private UnityTransport _unityTransport;
        private ConnectionData _connection;

        private ClientState _localState = ClientState.Offline;
        private ServerType _serverType = ServerType.None; //This value is only accurate on the server (client doesnt know this)
        private float _updateTimer;
        private int _playerID = -1;
        private bool _changingScene; // Is loading a new scene
        private bool _offlineMode; // If true, means it's in singleplayer mode
        private bool _isInitialized;
        private bool _isWorldReceived; // Save file has been received

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            
            Init();
            RegisterDefaultPrefabs();
        }

        private void Update()
        {
            _updateTimer += Time.deltaTime;

            if (_updateTimer <= SlowUpdateRefreshDelay)
                return;
            
            _updateTimer = 0f;
            SlowUpdate();
        }

        private void OnDestroy()
        {
            if (!_isInitialized)
                return;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        // Start simulated host with all networking turned off
        public void StartHostOffline()
        {
            Debug.Log(message: "Host Offline");
            
            ResetValues();
            
            _offlineMode = true;
            _serverType = ServerType.None;
            
            _networkManager.Shutdown();
            //CreateClient(ClientID, _auth.UserID, _auth.Username);
            CreateClient(ClientID, "ID is bitch", "Eminem"); // TEMP
            AfterConnected();
            OnHostConnect(ClientID);
        }
        
        // Start a host (client + server)
        public void StartHost(ushort port)
        {
            if (_localState != ClientState.Offline)
                return;
            
            //if (!_auth.IsConnected())
                //return; //Not logged in

            Debug.Log("Host Game");
            ResetValues();
            _serverType = ServerType.PeerToPeer;
            //_connection._userID = _auth.UserID;
            //_connection._username = _auth.Username;
            _connection._isHost = true;

            _unityTransport.SetConnectionData(ipv4Address: ListenAll, port);
            _networkManager.NetworkConfig.ConnectionData = NetworkUtilities.NetSerialize(_connection);

            _networkManager.StartHost();
            //CreateClient(ClientID, _auth.UserID, _auth.Username);
            CreateClient(ClientID, "ID is bitch", "Eminem"); // TEMP
            AfterConnected();
            OnHostConnect(ClientID);
        }
        
        // Start a dedicated server
        public void StartServer(ushort port)
        {
            if (_localState != ClientState.Offline)
                return;

            Debug.Log("Create Game Server");

            ResetValues();
            _serverType = ServerType.DedicatedServer;
            _connection._userID = "";
            _connection._username = "";

            _unityTransport.SetConnectionData(ipv4Address: ListenAll, port);
            _networkManager.NetworkConfig.ConnectionData = NetworkUtilities.NetSerialize(_connection);

            _networkManager.StartServer();
            AfterConnected();
            OnConnectEvent?.Invoke();
        }
        
        // If 'isHost' is set to true, it means this player created the game on a dedicated server
        // so it's still a client (not server) but is the one who selected game settings
        public void StartClient(string serverURL, ushort port, bool isHost = false)
        {
            if (_localState != ClientState.Offline)
                return;
            
            //if (!_auth.IsConnected())
                //return; //Not logged in

            Debug.Log("Join Game: " + serverURL);
            ResetValues();
            _serverType = ServerType.None; //Unknown, could be dedicated or peer2peer
            //_connection._userID = _auth.UserID;
            //_connection._username = _auth.Username;
            _connection._isHost = isHost;

            string ip = NetworkUtilities.HostToIP(serverURL);
            _unityTransport.SetConnectionData(ip, port);
            _networkManager.NetworkConfig.ConnectionData = NetworkUtilities.NetSerialize(_connection);

            _networkManager.StartClient();
            AfterConnected();
        }
        
        // Make sure the Unity Transport protocol is set to Relay when using Relay
        public void StartHostRelay(RelayConnectData relay)
        {
            if (_localState != ClientState.Offline)
                return;
            
            if (relay == null)
                return;
            
            //if (!_auth.IsConnected())
                //return; //Not logged in

            Debug.Log("Host Relay Game");
            
            ResetValues();
            _serverType = ServerType.RelayServer;
            //_connection._userID = _auth.UserID;
            //_connection._username = _auth.Username;
            _connection._isHost = true;

            _unityTransport.SetHostRelayData(relay.url, relay.port, relay.alloc_id, relay.alloc_key, relay.connect_data);
            _networkManager.NetworkConfig.ConnectionData = NetworkUtilities.NetSerialize(_connection);

            _networkManager.StartHost();
            //CreateClient(ClientID, _auth.UserID, _auth.Username);
            CreateClient(ClientID, "ID is bitch", "Eminem"); // TEMP
            AfterConnected();
            OnHostConnect(ClientID);
        }
        
        public void StartClientRelay(RelayConnectData relay)
        {
            if (_localState != ClientState.Offline)
                return;
            
            if (relay == null)
                return;
            
            //if (!_auth.IsConnected())
                //return; //Not logged in

            Debug.Log("Join Relay Game: " + relay.url + " Join Code: " + relay.join_code);
            
            ResetValues();
            _serverType = ServerType.RelayServer;
            //_connection._userID = _auth.UserID;
            //_connection._username = _auth.Username;

            _unityTransport.SetClientRelayData(relay.url, relay.port, relay.alloc_id, relay.alloc_key,
                connectionData: relay.connect_data, relay.host_connect_data);
            
            _networkManager.NetworkConfig.ConnectionData = NetworkUtilities.NetSerialize(_connection);

            _networkManager.StartClient();
            AfterConnected();
        }

        public void Disconnect()
        {
            if (!IsClient && !IsServer)
                return;

            Debug.Log("Disconnect");
            
            _networkManager.Shutdown();
            AfterDisconnected();
        }

        public static TheNetworkHorrorDepricated2 Get() => _instance;

        public bool IsActive() =>
            _networkManager.IsServer || _networkManager.IsHost || _networkManager.IsClient;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void Init()
        {
            _isInitialized = true;
            _networkManager = GetComponent<NetworkManager>();
            _unityTransport = GetComponent<UnityTransport>();
            _connection = new ConnectionData();
            
            _unityTransport.ConnectionData.ServerListenAddress = ListenAll;
            _unityTransport.ConnectionData.Address = ListenAll;

            _networkManager.ConnectionApprovalCallback += OnApprovalCheck;
            _networkManager.OnClientConnectedCallback += OnClientConnected;
            _networkManager.OnClientDisconnectCallback += OnClientDisconnect;
        }
        
        private void SlowUpdate()
        {
            CheckIfPlayerAssigned();
            CheckIfReady();
            //_auth?.Update(SlowUpdateRefreshDelay);
        }
        
        // Loop on connected player and assign a Player ID, will only work if the server is ready yet (world received)
        private void CheckIfPlayerAssigned()
        {
            bool canAssign = IsServer && _isWorldReceived;

            if (!canAssign)
                return;

            foreach (KeyValuePair<ulong, ClientData> pair in _clientList)
            {
                ClientData client = pair.Value;

                if (client.PlayerID < 0)
                    AssignPlayerID(client.ClientID);
            }
        }
        
        // This will be triggered when the server is ready and a player has joined
        private void AssignPlayerID(ulong clientID)
        {
            bool canAssign = clientID == ServerID || _localState == ClientState.Ready;

            if (!canAssign)
                return;

            // Assign self before assigning other clients

            ClientData client = GetClient(clientID);
            bool isClientValid = client != null && client.PlayerID < 0;

            if (!isClientValid)
                return;

            int playerID = FindPlayerID(clientID);
            bool isPlayerIDValid = playerID >= 0;

            if (!isPlayerIDValid)
                return;

            client.PlayerID = playerID;
            Debug.Log("Player ID " + playerID + " assigned to: " + client.UserID + " " + client.Username);

            // If self, change this.player_id
            if (clientID == ClientID)
                _playerID = playerID;

            OnAssignPlayerIDEvent?.Invoke(clientID, playerID);

            // If not self, send the ID to client
            if (clientID != ClientID)
            {
                //_messaging.SendInt("id", clientID, playerID, NetworkDelivery.ReliableSequenced);
            }
        }

        private void CheckIfReady()
        {
            //if (_localState != ClientState.Connecting || !IsConnected())
                //return; // Wrong state, no need to check

            bool rvalid = OnCheckReadyEvent == null || OnCheckReadyEvent.Invoke(); // Custom condition
            bool pvalid = !IsClient || _playerID >= 0; // Client has ID assigned
            bool gvalid = HorrorGameDepricated2.Get() != null; // Game scene is loaded
            bool canTriggerReady = rvalid && pvalid && gvalid && !_changingScene && _isWorldReceived;

            if (!canTriggerReady)
                return;

            Debug.Log("Ready!");
            TriggerReady();
        }
        
        private void TriggerReady()
        {
            SetState(ClientState.Ready);
            //TriggerReadyObjects();
            //TriggerReadyPlayers();
            OnReadyEvent?.Invoke();
        }
        
        private void SetState(ClientState state)
        {
            _localState = state;

            if (!IsServer)
            {
                //_messaging.SendInt("state", ServerID, (int)state, NetworkDelivery.Reliable);
            }
            else
            {
                ReceiveState(ServerID, _localState);
            }
        }
        
        private void ReceiveState(ulong clientID, ClientState state)
        {
            if (!IsServer)
                return;

            ClientData client = GetClient(clientID);

            if (client == null || client.PlayerID < 0)
                return;

            client.state = state;
            
            bool triggerReadyPlayer =
                _localState == ClientState.Ready && state == ClientState.Ready && clientID != ServerID;

            if (triggerReadyPlayer)
                TriggerReadyPlayer(clientID);
        }
        
        // This will be triggered when both the server and the player are ready
        private void TriggerReadyPlayer(ulong clientID)
        {
            bool canTriggerReady = IsServer && _localState == ClientState.Ready;

            if (!canTriggerReady)
                return;

            Debug.Log("Client is Ready:" + clientID);

            //SpawnClientObjects(clientID);
            //SpawnPlayer(clientID);
            OnClientReadyEvent?.Invoke(clientID);
        }
        
        private void RegisterDefaultPrefabs() => RegisterPrefab(_playerPrefab);
        
        private void RegisterPrefab(GameObject prefab)
        {
            if (prefab == null)
                return;
            
            var networkObject = prefab.GetComponent<NetworkObject>();

            if (networkObject == null)
                return;
            
            _networkManager.PrefabHandler.AddHandler(prefab, new NetworkPrefabHandler(prefab));
        }

        private void UnRegisterPrefab(GameObject prefab)
        {
            if (prefab == null)
                return;
            
            var networkObject = prefab.GetComponent<NetworkObject>();

            if (networkObject == null)
                return;
            
            _networkManager.PrefabHandler.RemoveHandler(prefab);
        }

        private void RemoveClient(ulong clientID)
        {
            if (!_clientList.ContainsKey(clientID))
                return;
            
            _clientList.Remove(clientID);
        }

        private void RequestWorldFrom(ClientData client)
        {
            if (IsHost || ServerID == client.ClientID || _isWorldReceived)
                return;

            // LobbyGame lobbyGame = GetLobbyGame();
            // bool firstClient = !lobbyGame.IsValid() && CountClients() == 1; //For direct connect when game_id is 0
            //
            // if (client.IsHost || firstClient)
            //     Messaging.SendEmpty("request", client.ClientID, NetworkDelivery.Reliable);
        }
        
        private void AfterConnected()
        {
            _localState = ClientState.Connecting;
            
            if (_networkManager.SceneManager != null)
                _networkManager.SceneManager.OnLoad += OnBeforeChangeScene;
            
            if (_networkManager.SceneManager != null)
                _networkManager.SceneManager.OnLoadComplete += OnAfterChangeScene;
            
            if (_networkManager.NetworkTickSystem != null)
                _networkManager.NetworkTickSystem.Tick += OnTick;
        }
        
        private void AfterDisconnected()
        {
            if (_localState == ClientState.Offline)
                return;

            if (_networkManager.SceneManager != null)
                _networkManager.SceneManager.OnLoad -= OnBeforeChangeScene;
            
            if (_networkManager.SceneManager != null)
                _networkManager.SceneManager.OnLoadComplete -= OnAfterChangeScene;
            
            if (_networkManager.NetworkTickSystem != null)
                _networkManager.NetworkTickSystem.Tick -= OnTick;
            
            _connection = new ConnectionData(); //Reset default
            _clientList.Clear();
            //_currentGame = null;
            ResetValues();
            OnDisconnectEvent?.Invoke();
        }
        
        private void DespawnPlayer(ulong clientID)
        {
            if (!IsServer)
                return;

            // SNetworkObject player = GetPlayerObject(clientID);
            // if (player != null)
            // {
            //     _playersList.Remove(clientID);
            //     player.Despawn(true);
            // }
        }
        
        private void ResetValues()
        {
            _localState = ClientState.Offline;
            _serverType = ServerType.None;
            _offlineMode = false;
            _changingScene = false;
            _isWorldReceived = false;
            _playerID = -1; //-1 means not a player
        }

        private ClientData CreateClient(ulong clientID, string userID, string username)
        {
            ClientData client = new(clientID)
            {
                UserID = userID,
                Username = username,
                ClientID = clientID,
                PlayerID = -1, //Not assigned yet
                state = ClientState.Connecting,
                DataReceived = clientID == ServerID
            };
            
            _clientList[clientID] = client;
            return client;
        }
        
        private ClientData GetClientByUserID(string userID)
        {
            foreach (ClientData client in _clientList.Values)
            {
                if (client.UserID == userID)
                    return client;
            }

            return null;
        }
        
        private ClientData GetClient(ulong clientID)
        {
            if (_clientList.ContainsKey(clientID))
                return _clientList[clientID];

            return null;
        }

        private int FindPlayerID(ulong clientID)
        {
            int playerID = (int)clientID;
            
            if (OnFindPlayerIDEvent != null)
                playerID = OnFindPlayerIDEvent.Invoke(clientID);
            
            return playerID;
        }
        
        private bool ApproveClient(ulong clientID, ConnectionData connect)
        {
            if (clientID == ServerID)
                return true; // Server always approve itself

            if (_offlineMode)
                return false;

            if (connect == null)
                return false; // Invalid data

            if (string.IsNullOrEmpty(connect._username) || string.IsNullOrEmpty(connect._userID))
                return false; // Invalid username

            if (HorrorGameDepricated2.Get() == null)
                return false; // Don't accept connection if game scene not loaded yet

            if (OnCheckApprovalEvent != null && !OnCheckApprovalEvent.Invoke(clientID, connect))
                return false; // Custom approval condition

            ClientData client = GetClientByUserID(connect._userID);
            if (client != null)
                return false; // Client already connected with this userID

            // Clear previous data
            RemoveClient(clientID);

            if (_clientList.Count >= Constants.MaxPlayers)
                return false; // Maximum number of Clients

            string message = "Approve connection: " + connect._userID + " " + connect._username;
            Debug.Log(message);
            
            ClientData clientData = CreateClient(clientID, connect._userID, connect._username);
            clientData.IsHost = connect._isHost;
            clientData.extra = connect._extra;

            OnClientApprovedEvent?.Invoke(clientID, connect);
            return true; // New Client approved
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnApprovalCheck(NetworkManager.ConnectionApprovalRequest request,
            NetworkManager.ConnectionApprovalResponse response)
        {
            var connectionData = NetworkUtilities.NetDeserialize<ConnectionData>(request.Payload);
            bool approved = ApproveClient(request.ClientNetworkId, connectionData);
            response.Approved = approved;
        }

        private void OnClientConnected(ulong clientID)
        {
            if (IsServer && clientID != ServerID)
            {
                ClientData client = GetClient(clientID);
                if (client == null)
                    return;

                string message = "Client Connected: " + client.ClientID;
                Debug.Log(message);

                // Ask for save file on dedicated server
                RequestWorldFrom(client);

                // Trigger join
                OnClientJoinEvent?.Invoke(clientID);
            }

            if (!IsServer)
                OnConnectEvent?.Invoke(); //Connect wasn't called yet for client
        }

        private void OnClientDisconnect(ulong clientID)
        {
            Debug.Log("Disconnecting: " + clientID);
            
            if (IsServer)
                OnClientQuitEvent?.Invoke(clientID);
            
            DespawnPlayer(clientID);
            RemoveClient(clientID);
            
            if (ClientID == clientID || clientID == ServerID)
                AfterDisconnected();
        }
        
        private void OnHostConnect(ulong clientID)
        {
            ClientData client = GetClient(clientID);
            
            if (client == null)
                return;

            client.state = ClientState.Connecting;
            client.IsHost = true;
            client.extra = _connection._extra;
            _isWorldReceived = true; // Host already has data

            OnConnectEvent?.Invoke();
        }
        
        private void OnBeforeChangeScene(ulong clientID, string scene, LoadSceneMode loadSceneMode,
            AsyncOperation async)
        {
            Debug.Log("Change Scene: " + scene);
            
            _localState = ClientState.Connecting;
            _changingScene = true;
            OnBeforeChangeSceneEvent?.Invoke(scene);
        }
        
        private void OnAfterChangeScene(ulong clientID, string scene, LoadSceneMode loadSceneMode)
        {
            if (clientID == ClientID)
                Debug.Log("Completed Load Scene: " + scene);

            if (ClientID != clientID)
                return;

            _changingScene = false;
            OnAfterChangeSceneEvent?.Invoke(scene);
        }
        
        private void OnTick() =>
            OnTickEvent?.Invoke();
    }
}