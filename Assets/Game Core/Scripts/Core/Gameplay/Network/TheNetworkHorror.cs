using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace GameCore.Gameplay.Network
{
    public class TheNetworkHorror : MonoBehaviour
    {
        // PROPERTIES: ----------------------------------------------------------------------------

        private static ulong ServerID => NetworkManager.ServerClientId; // ID of the server

        private bool IsServer => _networkManager.IsServer;
        private bool IsClient => _networkManager.IsClient;

        // FIELDS: --------------------------------------------------------------------------------

        // Server only events
        public event UnityAction<ulong> OnClientJoinEvent; // Server event when any client connect
        public event UnityAction<ulong> OnClientQuitEvent; // Server event when any client disconnect

        private static TheNetworkHorror _instance;
        private static List<ulong> _spawnedPlayers = new();

        private NetworkManager _networkManager;

        private ClientState _localState = ClientState.Offline;
        private float _updateTimer;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Init();
        }

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

        public void StartHost() =>
            _networkManager.StartHost();

        public void StartClient() =>
            _networkManager.StartClient();

        public bool IsActive() =>
            _networkManager.IsServer || _networkManager.IsHost || _networkManager.IsClient;

        public static TheNetworkHorror Get() => _instance;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void Init()
        {
            _instance = this;
            _networkManager = GetComponent<NetworkManager>();

            _networkManager.ConnectionApprovalCallback += OnApprovalCheck;
            _networkManager.OnClientConnectedCallback += OnClientConnected;
            _networkManager.OnClientDisconnectCallback += OnClientDisconnect;
        }

        private void SlowUpdate()
        {
            CheckIfReady();
        }

        private void CheckIfReady()
        {
            if (!IsConnected())
                return;

            NetworkSpawner networkSpawner = NetworkSpawner.Get();

            if (networkSpawner == null)
                return;

            if (!networkSpawner.IsSpawnerReady())
                networkSpawner.SpawnNetworkObject();

            bool isGameSceneValid = HorrorGame.Get() != null; // Game scene is loaded
            bool isReady = isGameSceneValid;

            if (!isReady)
                return;

            TriggerReady();
        }

        private void TriggerReady()
        {
            if (!IsServer)
                return;

            IReadOnlyList<ulong> connectedClientsIDs = _networkManager.ConnectedClientsIds;

            foreach (ulong clientID in connectedClientsIDs)
            {
                bool isSpawned = _spawnedPlayers.Contains(clientID);

                if (isSpawned)
                    continue;

                _spawnedPlayers.Add(clientID);
                NetworkSpawner.Get().SpawnPlayer(clientID);
            }
        }

        private bool IsConnected() =>
            _networkManager.IsServer || _networkManager.IsConnectedClient;

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

            //if (!IsServer)
            //OnConnectEvent?.Invoke(); // Connect wasn't called yet for client
        }

        private void OnClientDisconnect(ulong clientID)
        {
            Debug.Log("Disconnecting: " + clientID);

            if (IsServer)
                OnClientQuitEvent?.Invoke(clientID);
        }
    }

    public enum ClientState
    {
        Offline = 0, // Not connected
        Connecting = 5, // Waiting to change scene or receive world data
        Ready = 10, // Everything is loaded and spawned
    }
}