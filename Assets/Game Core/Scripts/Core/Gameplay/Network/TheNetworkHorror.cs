using System;
using System.Collections.Generic;
using GameCore.Gameplay.Entities.Player;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace GameCore.Gameplay.Network
{
    public class TheNetworkHorror : NetworkBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private PlayerEntity _playerPrefab;
        
        // PROPERTIES: ----------------------------------------------------------------------------

        private static ulong ServerID => NetworkManager.ServerClientId; // ID of the server

        // ID of this client (if host, will be same than ServerID), changes for every reconnection, assigned by Netcode
        private ulong ClientID => _offlineMode ? ServerID : _networkManager.LocalClientId;

        // FIELDS: --------------------------------------------------------------------------------

        // Server & Client events
        public UnityAction OnConnectEvent; // Event when self connect, happens before onReady, before sending any data
        public UnityAction<string> OnBeforeChangeSceneEvent; // Before Changing Scene
        public UnityAction<string> OnAfterChangeSceneEvent; // After Changing Scene

        // Server only events
        public event UnityAction<ulong> OnClientJoinEvent; // Server event when any client connect
        public event UnityAction<ulong> OnClientQuitEvent; // Server event when any client disconnect

        public delegate bool ApprovalEvent(ulong clientID, int data);
        public ApprovalEvent OnCheckApprovalEvent; // Additional approval validations for when a client connects
        
        public delegate Vector3 Vector3Event(int playerID);
        public Vector3Event OnFindPlayerSpawnPositionEvent; // Find player spawn position for client

        private static readonly Dictionary<ulong, bool> ReadyPlayersDictionary = new();
        private static readonly List<ulong> SpawnedPlayers = new();
        
        private static TheNetworkHorror _instance;

        private NetworkManager _networkManager;
        private NetworkSceneManager _networkSceneManager;

        private ClientState _localState = ClientState.Offline;
        private float _updateTimer;
        private bool _offlineMode;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            DontDestroyOnLoad(gameObject);

        private void Update()
        {
            const float refreshDuration = 1f;
            _updateTimer += Time.deltaTime;

            if (_updateTimer <= refreshDuration)
                return;

            _updateTimer = 0f;
            SlowUpdate();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Init(NetworkManager networkManager)
        {
            _instance = this;
            _networkManager = networkManager;

            _networkManager.ConnectionApprovalCallback += OnApprovalCheck;
            _networkManager.OnClientConnectedCallback += OnClientConnected;
            _networkManager.OnClientDisconnectCallback += OnClientDisconnect;
        }

        public void StartHost()
        {
            _networkManager.StartHost();
            AfterConnected();
        }

        public void StartClient()
        {
            _networkManager.StartClient();
            AfterConnected();
        }

        public void Disconnect()
        {
            SpawnedPlayers.Remove(ClientID); // TEMP

            if (!IsClient && !IsServer)
                return;

            _networkManager.Shutdown();
            AfterDisconnected();
        }

        public bool IsActive() =>
            _networkManager.IsServer || _networkManager.IsHost || _networkManager.IsClient;

        public static TheNetworkHorror Get() => _instance;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SlowUpdate()
        {
            UpdateHost();
            UpdateClient();
        }

        private void UpdateHost()
        {
            if (!IsHost && !IsServer)
                return;

            if (!IsConnected())
                return;

            CheckIfReady();
            CheckIfCanSpawnPlayers();
        }

        private void UpdateClient()
        {
            if (IsHost || IsServer)
                return;

            if (!IsConnected())
                return;

            CheckIfReady();
        }

        private void CheckIfReady()
        {
            bool isGameplaySceneLoaded = HorrorGame.Get() != null;

            if (!isGameplaySceneLoaded)
                return;

            bool isReady = ReadyPlayersDictionary.ContainsKey(ClientID) && ReadyPlayersDictionary[ClientID];

            if (isReady)
                return;

            SetPlayerReadyServerRpc();

            ReadyPlayersDictionary[ClientID] = true;
        }

        private void CheckIfCanSpawnPlayers()
        {
            NetworkSpawner networkSpawner = NetworkSpawner.Get();

            if (networkSpawner == null)
                return;

            if (!networkSpawner.IsSpawnerReady())
                networkSpawner.Spawn();

            IReadOnlyList<ulong> connectedClientsIDs = _networkManager.ConnectedClientsIds;

            foreach (ulong clientID in connectedClientsIDs)
            {
                bool isPlayerSpawned = SpawnedPlayers.Contains(clientID);

                if (isPlayerSpawned)
                    continue;

                bool isPlayerReady = ReadyPlayersDictionary.ContainsKey(clientID) && ReadyPlayersDictionary[clientID];

                if (!isPlayerReady)
                    continue;

                SpawnedPlayers.Add(clientID);

                Vector3 spawnPosition = Vector3.zero;
                
                if (OnFindPlayerSpawnPositionEvent != null)
                    spawnPosition = OnFindPlayerSpawnPositionEvent.Invoke(playerID: 0);
                
                SpawnPlayerServerRpc(spawnPosition, clientID);
            }
        }

        private void AfterConnected()
        {
            if (_networkManager.SceneManager != null)
                _networkManager.SceneManager.OnLoad += OnBeforeChangeScene;

            if (_networkManager.SceneManager != null)
                _networkManager.SceneManager.OnLoadComplete += OnAfterChangeScene;
        }

        private void AfterDisconnected()
        {
            if (_networkManager.SceneManager != null)
                _networkManager.SceneManager.OnLoad -= OnBeforeChangeScene;

            if (_networkManager.SceneManager != null)
                _networkManager.SceneManager.OnLoadComplete -= OnAfterChangeScene;
        }

        private bool IsConnected() =>
            _networkManager.IsServer || _networkManager.IsConnectedClient;

        // RPC: -----------------------------------------------------------------------------------
        
        [ServerRpc(RequireOwnership = false)]
        private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
        {
            ulong clientID = serverRpcParams.Receive.SenderClientId;
            ReadyPlayersDictionary[clientID] = true;
        }
        
        [ServerRpc]
        private void SpawnPlayerServerRpc(Vector3 spawnPosition, ulong clientID)
        {
            PlayerEntity playerInstance = Instantiate(_playerPrefab, spawnPosition, Quaternion.identity);

            NetworkObject playerNetworkObject = playerInstance.GetNetworkObject();
            playerNetworkObject.SpawnAsPlayerObject(clientID, destroyWithScene: true);

            SetupPlayerClientRpc(playerNetworkObject);
        }
        
        [ClientRpc]
        private void SetupPlayerClientRpc(NetworkObjectReference playerNetworkObjectReference)
        {
            bool isNetworkObjectFound = playerNetworkObjectReference.TryGet(out NetworkObject playerNetworkObject);

            if (!isNetworkObjectFound)
                return;

            bool isPlayerEntityFound = playerNetworkObject.TryGetComponent(out PlayerEntity playerEntity);

            if (!isPlayerEntityFound)
                return;
            
            playerEntity.Setup();
        }
        
        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnApprovalCheck(NetworkManager.ConnectionApprovalRequest request,
            NetworkManager.ConnectionApprovalResponse response)
        {
            bool approved = true; // TEMP
            response.Approved = approved;
        }

        private void OnClientConnected(ulong clientID)
        {
            if (IsServer && clientID != ServerID)
            {
                Debug.Log(message: "Client Connected: " + clientID);

                // Trigger join
                OnClientJoinEvent?.Invoke(clientID);
            }
            else
            {
                Debug.Log(message: "Client NOT Connected: " + clientID);
            }

            if (!IsServer)
                OnConnectEvent?.Invoke(); // Connect wasn't called yet for client
        }

        private void OnClientDisconnect(ulong clientID)
        {
            Debug.Log("Disconnecting: " + clientID);

            SpawnedPlayers.Remove(ClientID);

            if (IsServer)
                OnClientQuitEvent?.Invoke(clientID);
        }

        private void OnBeforeChangeScene(ulong clientId, string sceneName, LoadSceneMode loadSceneMode,
            AsyncOperation asyncOperation)
        {
            OnBeforeChangeSceneEvent?.Invoke(sceneName);
        }

        private void OnAfterChangeScene(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
        {
            OnAfterChangeSceneEvent?.Invoke(sceneName);
        }
    }

    public enum ClientState
    {
        Offline = 0, // Not connected
        Connecting = 5, // Waiting to change scene or receive world data
        Ready = 10, // Everything is loaded and spawned
    }
}