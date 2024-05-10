using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameCore.Gameplay.Network
{
    public class NetworkHorror : INetworkHorror, IDisposable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public NetworkHorror(INetcodeHooks netcodeHooks)
        {
            _netcodeHooks = netcodeHooks;
            
            _netcodeHooks.OnNetworkSpawnHookEvent += OnNetworkSpawnHook;
            _netcodeHooks.OnNetworkDespawnHookEvent += OnNetworkDespawnHook;
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public static ulong ServerID { get; private set; }
        public static ulong ClientID { get; private set; }

        public bool IsOwner => _netcodeHooks.IsOwner;
        private ulong OwnerClientId => _netcodeHooks.OwnerClientId;
        
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<ulong> OnPlayerConnectedEvent = delegate { };
        public event Action<ulong> OnPlayerDisconnectedEvent = delegate { };
        
        private readonly INetcodeHooks _netcodeHooks;
        
        private NetworkManager _networkManager;
        private bool _initialSpawnDone;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Dispose()
        {
            _netcodeHooks.OnNetworkSpawnHookEvent -= OnNetworkSpawnHook;
            _netcodeHooks.OnNetworkDespawnHookEvent -= OnNetworkDespawnHook;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void InitAll()
        {
            _networkManager = NetworkManager.Singleton;
            ServerID = OwnerClientId;
            ClientID = _networkManager.LocalClientId;
            
            _networkManager.OnClientConnectedCallback += OnClientConnected;
            _networkManager.OnClientDisconnectCallback += OnClientDisconnect;
            //_networkManager.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
            //_networkManager.SceneManager.OnSynchronizeComplete += OnSynchronizeComplete;
        }

        private void DespawnAll()
        {
            _networkManager.OnClientConnectedCallback -= OnClientConnected;
            _networkManager.OnClientDisconnectCallback -= OnClientDisconnect;
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnNetworkSpawnHook() => InitAll();

        private void OnNetworkDespawnHook() => DespawnAll();

        private void OnClientConnected(ulong clientId)
        {
            Debug.Log($"Client #{clientId} connected.");
            
            OnPlayerConnectedEvent.Invoke(clientId);
        }

        private void OnClientDisconnect(ulong clientId)
        {
            Debug.Log($"Client #{clientId} disconnect.");
            
            OnPlayerDisconnectedEvent.Invoke(clientId);
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