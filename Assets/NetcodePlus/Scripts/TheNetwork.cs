using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace NetcodePlus
{
    [DefaultExecutionOrder(-10)]
    public class TheNetwork : MonoBehaviour
    {
        public NetworkData data;

        //Server & Client events
        public UnityAction onTick; //Every network tick
        public UnityAction onReady; //Event after connection fully established (save file sent, scene loaded...)
        public UnityAction onConnect; //Event when self connect, happens before onReady, before sending any data
        public UnityAction onDisconnect; //Event when self disconnect
        public UnityAction<string> onBeforeChangeScene; //Before Changing Scene
        public UnityAction<string> onAfterChangeScene; //After Changing Scene

        public delegate bool ReadyCheckEvent();

        public ReadyCheckEvent checkReady; //Additional optional ready validations

        public UnityAction<int, FastBufferReader> onReceivePlayer; //Server receives data from client after connection
        public UnityAction<int, FastBufferWriter> onSendPlayer; //Client send data to server after connection
        public UnityAction<FastBufferReader> onReceiveWorld; //Client receives data from server after connection
        public UnityAction<FastBufferWriter> onSendWorld; //Server sends data to client after connection

        //Server only events
        public UnityAction<ulong> onClientJoin; //Server event when any client connect
        public UnityAction<ulong> onClientQuit; //Server event when any client disconnect
        public UnityAction<ulong> onClientReady; //Server event when any client become ready
        public UnityAction<string> onSaveRequest; //Server event when a player asks to save

        public UnityAction<ulong, ConnectionData> onClientApproved; //Called when a new client was succesfully approved
        public UnityAction<ulong, int> onAssignPlayerID; //Server event after assigning id (client_id, player_id)

        public UnityAction<int, SNetworkObject>
            onBeforePlayerSpawn; //Server event before a player spawn (player_id, player object)

        public UnityAction<int, SNetworkObject>
            onSpawnPlayer; //Server event after a player is spawned (player_id, player object)

        public delegate bool ApprovalEvent(ulong clientID, ConnectionData connectData);

        public ApprovalEvent checkApproval; //Additional approval validations for when a client connects

        public delegate Vector3 Vector3Event(int playerID);

        public Vector3Event findPlayerSpawnPos; //Find player spawn position for client

        public delegate GameObject PrefabEvent(int playerID);

        public PrefabEvent findPlayerPrefab; //Find player prefab for client

        public delegate int IntEvent(ulong clientID);

        public IntEvent findPlayerID; //Find player ID for client

        //---------

        private NetworkManager _network;
        private UnityTransport _transport;
        private ConnectionData _connection;
        private NetworkMessaging _messaging;
        private Authenticator _auth;
        private UnityAction _refreshCallback;

        private readonly Dictionary<ulong, ClientData> _clientList = new();
        private readonly Dictionary<ulong, GameObject> _prefabList = new();
        private readonly Dictionary<ulong, SNetworkObject> _playersList = new();

        private int _playerID = -1;
        private bool _changingScene; //Is loading a new scene
        private bool _worldReceived; //Save file has been received
        private bool _offlineMode; //If true, means its in singleplayer mode
        private ClientState _localState = ClientState.Offline;

        private ServerType
            _serverType = ServerType.None; //This value is only accurate on the server (client doesnt know this)

        private LobbyGame _currentGame; //Saves lobby data for future ref
        private float _updateTimer;

        [NonSerialized]
        private static bool _inited;

        private static TheNetwork _instance;

        private const string _listenAll = "0.0.0.0";
        private const int _msgSize = 1024 * 1024;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return; //Manager already exists, destroy this one
            }

            Init();
            RegisterDefaultPrefabs();
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            const float refreshDuration = 1f;

            _updateTimer += Time.deltaTime;

            if (_updateTimer <= refreshDuration)
                return;
            
            _updateTimer = 0f;
            SlowUpdate(refreshDuration);
        }

        public void Init()
        {
            if (!_inited || _transport == null)
            {
                _instance = this;
                _inited = true;
                _network = GetComponent<NetworkManager>();
                _transport = GetComponent<UnityTransport>();
                _connection = new ConnectionData();
                _messaging = new NetworkMessaging(this);

                _transport.ConnectionData.ServerListenAddress = _listenAll;
                _transport.ConnectionData.Address = _listenAll;

                _network.ConnectionApprovalCallback += ApprovalCheck;
                _network.OnClientConnectedCallback += OnClientConnect;
                _network.OnClientDisconnectCallback += OnClientDisconnect;

                InitAuthentication();
            }
        }

        private void SlowUpdate(float delta)
        {
            CheckIfPlayerAssigned();
            CheckIfReady();
            _auth?.Update(delta);
        }

        private async void InitAuthentication()
        {
            _auth = Authenticator.Create(data._authType);
            await _auth.Initialize();

            if (data._authAutoLogout)
                _auth.Logout();
        }

        //Start simulated host with all networking turned off
        public void StartHostOffline()
        {
            Debug.Log("Host Offline");
            ResetValues();
            _offlineMode = true;
            _serverType = ServerType.None;
            _network.Shutdown();
            CreateClient(ClientID, _auth.UserID, _auth.Username);
            AfterConnected();
            OnHostConnect(ClientID);
        }

        //Start a host (client + server)
        public void StartHost(ushort port)
        {
            if (_localState != ClientState.Offline)
                return;
            if (!_auth.IsConnected())
                return; //Not logged in

            Debug.Log("Host Game");
            ResetValues();
            _serverType = ServerType.PeerToPeer;
            _connection._userID = _auth.UserID;
            _connection._username = _auth.Username;
            _connection._isHost = true;

            _transport.SetConnectionData(_listenAll, port);
            _network.NetworkConfig.ConnectionData = NetworkTool.NetSerialize(_connection);

            _network.StartHost();
            CreateClient(ClientID, _auth.UserID, _auth.Username);
            AfterConnected();
            OnHostConnect(ClientID);
        }

        //Start a dedicated server
        public void StartServer(ushort port)
        {
            if (_localState != ClientState.Offline)
                return;

            Debug.Log("Create Game Server");

            ResetValues();
            _serverType = ServerType.DedicatedServer;
            _connection._userID = "";
            _connection._username = "";

            _transport.SetConnectionData(_listenAll, port);
            _network.NetworkConfig.ConnectionData = NetworkTool.NetSerialize(_connection);

            _network.StartServer();
            AfterConnected();
            onConnect?.Invoke();
        }

        //If is_host is set to true, it means this player created the game on a dedicated server
        //so its still a client (not server) but is the one who selected game settings
        public void StartClient(string serverURL, ushort port, bool isHost = false)
        {
            if (_localState != ClientState.Offline)
                return;
            
            if (!_auth.IsConnected())
                return; //Not logged in

            Debug.Log("Join Game: " + serverURL);
            ResetValues();
            _serverType = ServerType.None; //Unknown, could be dedicated or peer2peer
            _connection._userID = _auth.UserID;
            _connection._username = _auth.Username;
            _connection._isHost = isHost;

            string ip = NetworkTool.HostToIP(serverURL);
            _transport.SetConnectionData(ip, port);
            _network.NetworkConfig.ConnectionData = NetworkTool.NetSerialize(_connection);

            _network.StartClient();
            AfterConnected();
        }

        //Make sure the Unity Transport protocol is set to Relay when using Relay
        public void StartHostRelay(RelayConnectData relay)
        {
            if (_localState != ClientState.Offline)
                return;
            if (relay == null)
                return;
            if (!_auth.IsConnected())
                return; //Not logged in

            Debug.Log("Host Relay Game");
            ResetValues();
            _serverType = ServerType.RelayServer;
            _connection._userID = _auth.UserID;
            _connection._username = _auth.Username;
            _connection._isHost = true;

            _transport.SetHostRelayData(relay.url, relay.port, relay.alloc_id, relay.alloc_key, relay.connect_data);
            _network.NetworkConfig.ConnectionData = NetworkTool.NetSerialize(_connection);

            _network.StartHost();
            CreateClient(ClientID, _auth.UserID, _auth.Username);
            AfterConnected();
            OnHostConnect(ClientID);
        }

        public void StartClientRelay(RelayConnectData relay)
        {
            if (_localState != ClientState.Offline)
                return;
            if (relay == null)
                return;
            if (!_auth.IsConnected())
                return; //Not logged in

            Debug.Log("Join Relay Game: " + relay.url + " Join Code: " + relay.join_code);
            ResetValues();
            _serverType = ServerType.RelayServer;
            _connection._userID = _auth.UserID;
            _connection._username = _auth.Username;

            _transport.SetClientRelayData(relay.url, relay.port, relay.alloc_id, relay.alloc_key, relay.connect_data,
                relay.host_connect_data);
            _network.NetworkConfig.ConnectionData = NetworkTool.NetSerialize(_connection);

            _network.StartClient();
            AfterConnected();
        }

        public void Disconnect()
        {
            if (!IsClient && !IsServer)
                return;

            Debug.Log("Disconnect");
            _network.Shutdown();
            AfterDisconnected();
        }

        private void AfterConnected()
        {
            _localState = ClientState.Connecting;
            if (_network.SceneManager != null)
                _network.SceneManager.OnLoad += OnBeforeChangeScene;
            if (_network.SceneManager != null)
                _network.SceneManager.OnLoadComplete += OnAfterChangeScene;
            if (_network.NetworkTickSystem != null)
                _network.NetworkTickSystem.Tick += OnTick;
            _messaging.ListenMsg("id", OnReceivePlayerID);
            _messaging.ListenMsg("state", OnReceiveState);
            Messaging.ListenMsg("save", OnReceiveSave);
            Messaging.ListenMsg("request", OnReceiveRequest);
            Messaging.ListenMsg("world", OnReceiveWorld);
            Messaging.ListenMsg("player", OnReceivePlayerData);
        }

        private void AfterDisconnected()
        {
            if (_localState == ClientState.Offline)
                return;

            if (_network.SceneManager != null)
                _network.SceneManager.OnLoad -= OnBeforeChangeScene;
            if (_network.SceneManager != null)
                _network.SceneManager.OnLoadComplete -= OnAfterChangeScene;
            if (_network.NetworkTickSystem != null)
                _network.NetworkTickSystem.Tick -= OnTick;
            _messaging.UnListenMsg("id");
            _messaging.UnListenMsg("state");
            Messaging.UnListenMsg("save");
            Messaging.UnListenMsg("request");
            Messaging.UnListenMsg("world");
            Messaging.UnListenMsg("player");

            _connection = new ConnectionData(); //Reset default
            _clientList.Clear();
            _currentGame = null;
            ResetValues();
            onDisconnect?.Invoke();
        }

        private void ResetValues()
        {
            _localState = ClientState.Offline;
            _serverType = ServerType.None;
            _offlineMode = false;
            _changingScene = false;
            _worldReceived = false;
            _playerID = -1; //-1 means not a player
        }

        private void OnHostConnect(ulong clientID)
        {
            ClientData client = GetClient(clientID);
            
            if (client == null)
                return;

            client.state = ClientState.Connecting;
            client.IsHost = true;
            client.extra = _connection._extra;
            _worldReceived = true; //Host already has data

            onConnect?.Invoke();
        }

        private void OnClientConnect(ulong clientID)
        {
            if (IsServer && clientID != ServerID)
            {
                ClientData client = GetClient(clientID);
                if (client == null)
                    return;

                Debug.Log("Client Connected: " + client.ClientID);

                //Ask for save file on dedicated server
                RequestWorldFrom(client);

                //Trigger join
                onClientJoin?.Invoke(clientID);
            }

            if (!IsServer)
            {
                onConnect?.Invoke(); //Connect wasn't called yet for client
            }
        }

        private void OnClientDisconnect(ulong clientID)
        {
            Debug.Log("Disconnecting: " + clientID);
            if (IsServer)
                onClientQuit?.Invoke(clientID);
            DespawnPlayer(clientID);
            RemoveClient(clientID);
            if (ClientID == clientID || clientID == ServerID)
                AfterDisconnected();
        }

        private void OnTick()
        {
            onTick?.Invoke();
        }

        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest req,
            NetworkManager.ConnectionApprovalResponse res)
        {
            ConnectionData connect = NetworkTool.NetDeserialize<ConnectionData>(req.Payload);
            bool approved = ApproveClient(req.ClientNetworkId, connect);
            res.Approved = approved;
        }

        private bool ApproveClient(ulong clientID, ConnectionData connect)
        {
            if (clientID == ServerID)
                return true; //Server always approve itself

            if (_offlineMode)
                return false;

            if (connect == null)
                return false; //Invalid data

            if (string.IsNullOrEmpty(connect._username) || string.IsNullOrEmpty(connect._userID))
                return false; //Invalid username

            if (NetworkGame.Get() == null)
                return false; //Dont accept connection if game scene not loaded yet

            if (checkApproval != null && !checkApproval.Invoke(clientID, connect))
                return false; //Custom approval condition

            ClientData client = GetClientByUserID(connect._userID);
            if (client != null)
                return false; //Client already connected with this user_id

            //Clear previous data
            RemoveClient(clientID);

            if (_clientList.Count >= NetworkData.Get()._playersMax)
                return false; //Maximum number of clients

            Debug.Log("Approve connection: " + connect._userID + " " + connect._username);
            ClientData nclient = CreateClient(clientID, connect._userID, connect._username);
            nclient.IsHost = connect._isHost;
            nclient.extra = connect._extra;

            onClientApproved?.Invoke(clientID, connect);
            return true; //New Client approved
        }

        public void SetLobbyGame(LobbyGame game)
        {
            _currentGame = game;
        }

        public void SetWorldReceived(bool received)
        {
            _worldReceived = received;
        }

        public void SetConnectionExtraData(byte[] bytes)
        {
            _connection._extra = bytes;
        }

        public void SetConnectionExtraData(string data)
        {
            _connection._extra = NetworkTool.SerializeString(data);
        }

        public void SetConnectionExtraData<T>(T data) where T : INetworkSerializable, new()
        {
            _connection._extra = NetworkTool.NetSerialize(data);
        }

        private void RegisterDefaultPrefabs()
        {
            RegisterPrefab(NetworkData.Get()._playerDefault);
        }

        public void RegisterPrefab(GameObject prefab)
        {
            if (prefab == null)
                return;
            
            SNetworkObject sobject = prefab.GetComponent<SNetworkObject>();
            if (sobject != null && !sobject._isScene && !_prefabList.ContainsKey(sobject._prefabID))
            {
                _prefabList[sobject._prefabID] = prefab;
            }

            NetworkObject nobject = prefab.GetComponent<NetworkObject>();

            if (nobject == null)
                return;
            
            _network.PrefabHandler.AddHandler(prefab, new NetworkPrefabHandler(prefab));
        }

        public void UnRegisterPrefab(GameObject prefab)
        {
            if (prefab != null)
            {
                SNetworkObject sobject = prefab.GetComponent<SNetworkObject>();
                if (sobject != null && _prefabList.ContainsKey(sobject._prefabID))
                {
                    _prefabList.Remove(sobject._prefabID);
                }

                NetworkObject nobject = prefab.GetComponent<NetworkObject>();
                if (nobject != null)
                {
                    _network.PrefabHandler.RemoveHandler(prefab);
                }
            }
        }

        public void SpawnPlayer(ulong client_id)
        {
            if (!IsServer)
                return;
            if (GetPlayerObject(client_id) != null)
                return; //Already Spawned
            ClientData client = GetClient(client_id);
            if (client == null)
                return; //Client not found
            if (client.PlayerID < 0)
                return; //Just an observer

            Vector3 pos = GetPlayerSpawnPos(client.PlayerID);
            GameObject prefab = GetPlayerPrefab(client.PlayerID);
            if (prefab == null)
                return;

            Debug.Log("Spawn Player: " + client.UserID + " " + client.Username + " " + client.PlayerID);

            GameObject player_obj = Instantiate(prefab, pos, prefab.transform.rotation);
            SNetworkObject player = player_obj.GetComponent<SNetworkObject>();
            _playersList[client_id] = player;
            onBeforePlayerSpawn?.Invoke(client.PlayerID, player);
            player.Spawn(client_id);
            onSpawnPlayer?.Invoke(client.PlayerID, player);
        }

        //Use this function to spawn player manually (return null from the findPlayerPrefab event to prevent spawning automatically)
        //This function will only work if the player_id has already been assigned (after ready was called)
        public void SpawnPlayer(int player_id, GameObject prefab, Vector3 pos)
        {
            if (!IsServer)
                return;

            ClientData client = GetClientByPlayerID(player_id);
            if (client == null)
                return; //Client not found

            ulong client_id = client.ClientID;
            if (GetPlayerObject(client_id) != null)
                return; //Already Spawned
            if (client.PlayerID < 0)
                return; //Just an observer

            Debug.Log("Spawn Player: " + client.UserID + " " + client.Username + " " + client.PlayerID);

            GameObject player_obj = Instantiate(prefab, pos, prefab.transform.rotation);
            SNetworkObject player = player_obj.GetComponent<SNetworkObject>();
            _playersList[client_id] = player;
            onBeforePlayerSpawn?.Invoke(client.PlayerID, player);
            player.Spawn(client_id);
            onSpawnPlayer?.Invoke(client.PlayerID, player);
        }

        private Vector3 GetPlayerSpawnPos(int player_id)
        {
            PlayerSpawn spawn = PlayerSpawn.Get(player_id); //Specific to this player_id
            if (spawn == null)
                spawn = PlayerSpawn.Get(); //Generic spawn position for all players
            Vector3 pos = spawn != null ? spawn.GetRandomPosition() : Vector3.zero;
            if (findPlayerSpawnPos != null)
                pos = findPlayerSpawnPos.Invoke(player_id);
            return pos;
        }

        private GameObject GetPlayerPrefab(int player_id)
        {
            if (findPlayerPrefab != null)
                return findPlayerPrefab.Invoke(player_id);
            return NetworkData.Get()._playerDefault;
        }

        private int FindPlayerID(ulong clientID)
        {
            int player_id = (int)clientID;
            if (findPlayerID != null)
                player_id = findPlayerID.Invoke(clientID);
            return player_id;
        }

        public void DespawnPlayer(ulong clientID)
        {
            if (!IsServer)
                return;

            SNetworkObject player = GetPlayerObject(clientID);
            if (player != null)
            {
                _playersList.Remove(clientID);
                player.Despawn(true);
            }
        }

        public void SpawnClientObjects(ulong clientID)
        {
            if (IsServer && clientID != ServerID)
            {
                NetworkGame.Get().Spawner.SpawnClientObjects(clientID);
            }
        }

        private void TriggerReady()
        {
            SetState(ClientState.Ready);
            TriggerReadyObjects();
            TriggerReadyPlayers();
            onReady?.Invoke();
        }

        private void TriggerReadyObjects()
        {
            List<SNetworkObject> nobjs = SNetworkObject.GetAll();
            for (int i = 0; i < nobjs.Count; i++)
            {
                nobjs[i].TriggerReady();
            }
        }

        private void TriggerReadyPlayers()
        {
            if (IsServer && _localState == ClientState.Ready)
            {
                foreach (KeyValuePair<ulong, ClientData> pair in _clientList)
                {
                    ClientData client = pair.Value;
                    if (client.state == ClientState.Ready)
                        TriggerReadyPlayer(client.ClientID);
                }
            }
        }

        //This will be triggered when both the server and the player are ready
        private void TriggerReadyPlayer(ulong clientID)
        {
            bool canTriggerReady = IsServer && _localState == ClientState.Ready;

            if (!canTriggerReady)
                return;

            Debug.Log("Client is Ready:" + clientID);

            SpawnClientObjects(clientID);
            SpawnPlayer(clientID);
            onClientReady?.Invoke(clientID);
        }

        private void CheckIfReady()
        {
            if (_localState != ClientState.Connecting || !IsConnected())
                return; //Wrong state, no need to check

            bool rvalid = checkReady == null || checkReady.Invoke(); // Custom condition
            bool pvalid = !IsClient || _playerID >= 0; // Client has ID assigned
            bool gvalid = NetworkGame.Get() != null; // Game scene is loaded
            bool canTriggerReady = rvalid && pvalid && gvalid && !_changingScene && _worldReceived;

            if (!canTriggerReady)
                return;

            Debug.Log("Ready!");
            TriggerReady();
        }

        //Loop on connected player and assign a Player ID, will only work if the server is ready yet (world received)
        private void CheckIfPlayerAssigned()
        {
            bool canAssign = IsServer && _worldReceived;

            if (!canAssign)
                return;

            foreach (KeyValuePair<ulong, ClientData> pair in _clientList)
            {
                ClientData client = pair.Value;

                if (client.PlayerID < 0)
                    AssignPlayerID(client.ClientID);
            }
        }

        //This will be triggered when the server is ready and a player has joined
        private void AssignPlayerID(ulong clientID)
        {
            bool canAssign = clientID == ServerID || _localState == ClientState.Ready;

            if (!canAssign)
                return;

            //Assign self before assigning other clients

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

            //If self, change this.player_id
            if (clientID == ClientID)
                _playerID = playerID;

            onAssignPlayerID?.Invoke(clientID, playerID);

            //If not self, send the ID to client
            if (clientID != ClientID)
                _messaging.SendInt("id", clientID, playerID, NetworkDelivery.ReliableSequenced);
        }

        //Call from server only
        public void LoadScene(string scene, bool forceReload = false, Action loadOfflineCallback = null)
        {
            bool reload = forceReload || scene != SceneManager.GetActiveScene().name;
            bool canLoadScene = IsServer && reload && !_changingScene;

            if (!canLoadScene)
                return;

            if (_offlineMode)
            {
                OnBeforeChangeScene(ClientID, scene, LoadSceneMode.Single, null);
                SceneManager.LoadScene(scene);
                Debug.LogWarning("----------- BEFORE LOADING");
                loadOfflineCallback?.Invoke();
                Debug.LogWarning("----------- AFTER LOADING");
                OnAfterChangeScene(ClientID, scene, LoadSceneMode.Single);
            }
            else
            {
                _changingScene = true;
                _localState = ClientState.Connecting;
                _network.SceneManager.LoadScene(scene, LoadSceneMode.Single);

                foreach (KeyValuePair<ulong, ClientData> client in _clientList)
                    client.Value.state = ClientState.Connecting;
            }
        }

        public void RestartScene()
        {
            if (!IsServer) return;

            string scene = SceneManager.GetActiveScene().name;

            if (IsOnline)
                _network.SceneManager.LoadScene(scene, LoadSceneMode.Single);
            else
                SceneManager.LoadScene(scene);
        }

        public void SetState(ClientState state)
        {
            _localState = state;

            if (!IsServer)
                _messaging.SendInt("state", ServerID, (int)state, NetworkDelivery.Reliable);
            else
                ReceiveState(ServerID, _localState);
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

        private void OnReceiveState(ulong clientID, FastBufferReader reader)
        {
            reader.ReadValueSafe(out int iState);
            ClientState state = (ClientState)iState;
            ReceiveState(clientID, state);
        }

        //Send save file to client_id (or to dedicated server)
        public void SendWorld(ulong clientID)
        {
            if (clientID == ClientID)
                return;

            Debug.Log("Send World Data to: " + clientID);
            FastBufferWriter writer = new FastBufferWriter(128, Allocator.Temp, MsgSizeMax);
            onSendWorld?.Invoke(writer); //Write the save file in this callback
            Messaging.Send("world", clientID, writer, NetworkDelivery.ReliableFragmentedSequenced);
            writer.Dispose();
        }

        private void OnReceiveWorld(ulong clientID, FastBufferReader reader)
        {
            if (IsServer && _serverType != ServerType.DedicatedServer)
                return; //Servers cant receive world (except dedicated server)
            if (_worldReceived)
                return; //Already received

            Debug.Log("Receive World Data from: " + clientID);
            _worldReceived = true;
            onReceiveWorld?.Invoke(reader); //Read the save file in this callback

            _refreshCallback?.Invoke();
            _refreshCallback = null;
        }

        //Send player save file to server
        public void SendPlayerData()
        {
            if (IsServer)
                return; //No need to send on server

            Debug.Log("Send Player Data");
            FastBufferWriter writer = new FastBufferWriter(128, Allocator.Temp, MsgSizeMax);
            onSendPlayer?.Invoke(_playerID, writer); //Write the save file in this callback
            Messaging.Send("player", ServerID, writer, NetworkDelivery.ReliableFragmentedSequenced);
            writer.Dispose();
        }

        private void OnReceivePlayerData(ulong clientID, FastBufferReader reader)
        {
            if (!IsServer)
                return; //Clients cant receive player data 

            ClientData client = GetClient(clientID);
            if (client == null || client.DataReceived)
                return; //Already received, or client invalid

            Debug.Log("Receive Player Data from: " + clientID);
            client.DataReceived = true;
            onReceivePlayer?.Invoke(client.PlayerID, reader); //Read the save file in this callback

            SendWorld(clientID); //Send back world
        }

        //Client asks the server to send back the up-to-date save file
        public void RequestWorld(UnityAction callback = null)
        {
            if (IsServer)
                return;

            _worldReceived = false;
            _refreshCallback = callback;
            Messaging.SendEmpty("request", ServerID, NetworkDelivery.Reliable);
        }

        public void RequestWorldFrom(ClientData client)
        {
            if (IsHost || ServerID == client.ClientID || _worldReceived)
                return;

            LobbyGame lobbyGame = GetLobbyGame();
            bool firstClient = !lobbyGame.IsValid() && CountClients() == 1; //For direct connect when game_id is 0

            if (client.IsHost || firstClient)
                Messaging.SendEmpty("request", client.ClientID, NetworkDelivery.Reliable);
        }

        //A "request" is asking for the save file
        private void OnReceiveRequest(ulong clientID, FastBufferReader reader) =>
            SendWorld(clientID); //Send back world file to client

        //Client asks the server to save the world (will be saved on the server)
        public void SaveWorld(string saveFile)
        {
            if (IsServer)
                return;

            Messaging.SendString("save", ServerID, saveFile, NetworkDelivery.Reliable);
        }

        private void OnReceiveSave(ulong clientID, FastBufferReader reader)
        {
            if (!IsServer || IsClient)
                return;

            reader.ReadValueSafe(out string saveFile);

            if (!string.IsNullOrEmpty(saveFile))
                onSaveRequest?.Invoke(saveFile);
        }

        private void OnReceivePlayerID(ulong clientID, FastBufferReader reader)
        {
            reader.ReadValueSafe(out _playerID);

            if (_playerID >= 0)
                SendPlayerData();
        }

        private void OnBeforeChangeScene(ulong clientID, string scene, LoadSceneMode loadSceneMode,
            AsyncOperation async)
        {
            Debug.Log("Change Scene: " + scene);
            _localState = ClientState.Connecting;
            _changingScene = true;
            onBeforeChangeScene?.Invoke(scene);
        }

        private void OnAfterChangeScene(ulong clientID, string scene, LoadSceneMode loadSceneMode)
        {
            if (clientID == ClientID)
                Debug.Log("Completed Load Scene: " + scene);

            if (ClientID != clientID)
                return;

            _changingScene = false;
            onAfterChangeScene?.Invoke(scene);
        }

        private ClientData CreateClient(ulong clientID, string userID, string username)
        {
            ClientData client = new(clientID);
            client.UserID = userID;
            client.Username = username;
            client.ClientID = clientID;
            client.PlayerID = -1; //Not assigned yet
            client.state = ClientState.Connecting;
            client.DataReceived = clientID == ServerID;
            _clientList[clientID] = client;
            return client;
        }

        private void RemoveClient(ulong clientID)
        {
            if (_clientList.ContainsKey(clientID))
                _clientList.Remove(clientID);
        }

        public ClientData GetClient(ulong clientID)
        {
            if (_clientList.ContainsKey(clientID))
                return _clientList[clientID];

            return null;
        }

        public ClientData GetClientByUserID(string userID)
        {
            foreach (ClientData client in _clientList.Values)
            {
                if (client.UserID == userID)
                    return client;
            }

            return null;
        }

        public ClientData GetClientByPlayerID(int playerID)
        {
            foreach (ClientData client in _clientList.Values)
            {
                if (client.PlayerID == playerID)
                    return client;
            }

            return null;
        }

        public bool HasClient(ulong clientID) =>
            _clientList.ContainsKey(clientID);

        public ClientState GetClientState(ulong clientID)
        {
            ClientData client = GetClient(clientID);

            if (client != null)
                return client.state;

            return ClientState.Offline;
        }

        public GameObject GetPrefab(ulong prefabID)
        {
            if (_prefabList.ContainsKey(prefabID))
                return _prefabList[prefabID];

            return null;
        }

        public SNetworkObject GetNetworkObject(ulong netID) =>
            NetworkGame.Get().Spawner.GetSpawnedObject(netID);

        public SNetworkBehaviour GetNetworkBehaviour(ulong netID, ushort behaviourID)
        {
            SNetworkObject sNetworkObject = NetworkGame.Get().Spawner.GetSpawnedObject(netID);

            if (sNetworkObject != null)
                return sNetworkObject.GetBehaviour(behaviourID);

            return null;
        }

        public SNetworkObject GetPlayerObject(ulong clientID)
        {
            //Works on server only
            if (_playersList.ContainsKey(clientID))
                return _playersList[clientID];

            return null;
        }

        public int GetClientPlayerID(ulong clientID)
        {
            if (_clientList.ContainsKey(clientID))
                return _clientList[clientID].PlayerID;

            return -1;
        }

        public Dictionary<ulong, ClientData> GetClientsData() => _clientList;

        public IReadOnlyList<ulong> GetClientsIds() => _network.ConnectedClientsIds;

        public int CountClients()
        {
            if (_offlineMode)
                return 1;

            if (IsServer && IsConnected())
                return _network.ConnectedClientsIds.Count;

            return 0;
        }

        public LobbyGame GetLobbyGame() => _currentGame;

        public ConnectionData GetConnectionData() => _connection;

        public bool HasAuthority(NetworkObject obj) =>
            IsServer || ClientID == obj.OwnerClientId;

        public bool IsGameScene()
        {
            return
                !_changingScene &&
                NetworkGame.Get() != null; // NetworkGame shouldn't be in menus, just in the game scenes
        }

        public bool IsChangingScene() => _changingScene;

        //Trying to connect but not yet
        public bool IsConnecting() =>
            IsActive() && !IsConnected();

        public bool IsConnected() =>
            _offlineMode || _network.IsServer || _network.IsConnectedClient;

        public bool IsActive() =>
            _offlineMode || _network.IsServer || _network.IsClient;

        public bool IsReady() =>
            _localState == ClientState.Ready && IsConnected();

        public string Address
        {
            get => _transport.ConnectionData.Address;
            set => _transport.ConnectionData.Address = value;
        }

        public ushort Port
        {
            get => _transport.ConnectionData.Port;
            set => _transport.ConnectionData.Port = value;
        }

        //ID of this client (if host, will be same than ServerID), changes for every reconnection, assigned by Netcode
        public ulong ClientID => _offlineMode ? ServerID : _network.LocalClientId;

        public ulong ServerID => NetworkManager.ServerClientId; //ID of the server

        //Player ID is specific to this game only and stays the same for this user when reconnecting (unlike clientID), assigned by NetcodePlus
        public int PlayerID => _playerID;

        //User ID is linked to authentication and not related to a specific game, assigned by the Authenticator
        public string UserID => _auth.UserID;

        public string Username => _auth.Username; //Display name of the player

        public bool IsServer => _offlineMode || _network.IsServer;

        public bool IsClient => _offlineMode || _network.IsClient;

        public bool IsHost => IsClient && IsServer; //Host is both a client and server

        public bool IsOnline => !_offlineMode && IsActive();

        public ClientState State => _localState;

        public ServerType ServerType => _serverType;

        public NetworkTime LocalTime => _network.LocalTime;

        public NetworkTime ServerTime => _network.ServerTime;

        public float DeltaTick => 1f / _network.NetworkTickSystem.TickRate;

        public NetworkManager NetworkManager => _network;

        public UnityTransport Transport => _transport;

        public NetworkMessaging Messaging => _messaging;

        public Authenticator Auth => _auth;

        public NetworkSpawner Spawner => NetworkGame.Get().Spawner;

        public static string ListenAll => _listenAll;

        public static int MsgSizeMax => _msgSize;

        public static int MsgSize => MsgSizeMax; //Old name

        public static TheNetwork Get()
        {
            if (_instance != null)
                return _instance;

            TheNetwork net = FindObjectOfType<TheNetwork>();
            net?.Init();

            return _instance;
        }
    }

    [Serializable]
    public class ConnectionData : INetworkSerializable
    {
        public string _userID = "";
        public string _username = "";

        //Client created the game? (Could be true for client that created the game on dedicated server)
        public bool _isHost;

        public byte[] _extra = Array.Empty<byte>();

        //If you add extra data, make sure the total size of ConnectionData doesn't exceed Netcode max unfragmented msg (1400 bytes)
        //Fragmented msg are not possible for connection data, since connection is done in a single request

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _userID);
            serializer.SerializeValue(ref _username);
            serializer.SerializeValue(ref _isHost);
            serializer.SerializeValue(ref _extra);
        }
    }

    public enum ServerType
    {
        None = 0,
        DedicatedServer = 10,
        RelayServer = 20,
        PeerToPeer = 30, //Requires Port Forwarding for the host
    }

    public enum ClientState
    {
        Offline = 0, //Not connected
        Connecting = 5, //Waiting to change scene or receive world data
        Ready = 10, //Everything is loaded and spawned
    }

    public class ClientData
    {
        public ulong ClientID;
        public int PlayerID;
        public string UserID;
        public string Username;
        public bool IsHost; //Doesn't necessarily mean its the server host, just that its the one that created the game
        public bool DataReceived; //Data was received from this user
        public ClientState state = ClientState.Offline;
        public byte[] extra = Array.Empty<byte>();

        public ClientData()
        {
        }

        public ClientData(ulong clientID) =>
            ClientID = clientID;

        public string GetExtraString() =>
            NetworkTool.DeserializeString(extra);

        public T GetExtraData<T>() where T : INetworkSerializable, new() =>
            NetworkTool.NetDeserialize<T>(extra);
    }
}