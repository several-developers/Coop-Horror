using System.Collections.Generic;
using GameCore.Gameplay.Network.Utilities;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace GameCore.Gameplay.Network
{
    public class NetworkHorror : NetworkBehaviour, INetcodeInitBehaviour, INetcodeDespawnBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(INetworkHorrorDecorator networkHorrorDecorator) =>
            _networkHorrorDecorator = networkHorrorDecorator;

        // FIELDS: --------------------------------------------------------------------------------

        private INetworkHorrorDecorator _networkHorrorDecorator;
        private NetworkManager _networkManager;
        private bool _initialSpawnDone;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Setup(NetworkManager networkManager) =>
            _networkManager = networkManager;

        public void InitServerAndClient()
        {
            _networkHorrorDecorator.OnIsServerEvent += IsServer;
            
            _networkManager.OnClientConnectedCallback += OnClientConnected;
            _networkManager.OnClientDisconnectCallback += OnClientDisconnect;
            //_networkManager.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
            //_networkManager.SceneManager.OnSynchronizeComplete += OnSynchronizeComplete;
        }

        public void InitServer()
        {
            if (!IsOwner)
                return;
        }

        public void InitClient()
        {
            if (IsOwner)
                return;
        }

        public void DespawnServerAndClient()
        {
            _networkHorrorDecorator.OnIsServerEvent -= IsServer;

            _networkManager.OnClientConnectedCallback -= OnClientConnected;
            _networkManager.OnClientDisconnectCallback -= OnClientDisconnect;
        }

        public void DespawnServer()
        {
            if (!IsOwner)
                return;
            
        }

        public void DespawnClient()
        {
            if (IsOwner)
                return;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private bool IsServer() => IsOwner;
        
        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            InitServerAndClient();
            InitServer();
            InitClient();
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            DespawnServerAndClient();
            DespawnServer();
            DespawnClient();
        }

        private void OnClientConnected(ulong clientId)
        {
            Debug.Log($"Client #{clientId} connected.");
        }

        private void OnClientDisconnect(ulong clientId)
        {
            Debug.Log($"Client #{clientId} disconnect.");
        }

        private void OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted,
            List<ulong> clientsTimedOut)
        {
            Debug.Log($"Load event completed.");

            if (!_initialSpawnDone && loadSceneMode == LoadSceneMode.Single)
            {
                _initialSpawnDone = true;

                foreach (ulong clientId in clientsCompleted)
                {
                    Debug.Log($"Player #{clientId} ready");
                }
            }
        }

        private void OnSynchronizeComplete(ulong clientId)
        {
            Debug.Log($"Client ${clientId} synchronize complete.");

            if (_initialSpawnDone)
            {
                Debug.Log($"Late join Player #{clientId} ready");
            }
        }
    }
}