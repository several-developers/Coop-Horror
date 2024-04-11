using System;
using System.Collections.Generic;
using GameCore.Gameplay.Network.Utilities;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameCore.Gameplay.Network
{
    public class NetworkHorror : INetworkHorror, IDisposable, INetcodeInitBehaviour, INetcodeDespawnBehaviour
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

        private readonly INetcodeHooks _netcodeHooks;
        
        private NetworkManager _networkManager;
        private bool _initialSpawnDone;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Dispose()
        {
            _netcodeHooks.OnNetworkSpawnHookEvent -= OnNetworkSpawnHook;
            _netcodeHooks.OnNetworkDespawnHookEvent -= OnNetworkDespawnHook;
        }
        
        public void InitServerAndClient()
        {
            ServerID = OwnerClientId;
            _networkManager = NetworkManager.Singleton;
            
            _networkManager.OnClientConnectedCallback += OnClientConnected;
            _networkManager.OnClientDisconnectCallback += OnClientDisconnect;
            //_networkManager.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
            //_networkManager.SceneManager.OnSynchronizeComplete += OnSynchronizeComplete;
        }

        public void InitServer()
        {
            if (!IsOwner)
                return;
            
            ClientID = _netcodeHooks.OwnerClientId;
        }

        public void InitClient()
        {
            if (IsOwner)
                return;
            
            ClientID = _networkManager.LocalClientId;
        }

        public void DespawnServerAndClient()
        {
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

        private bool IsNetworkOwner() => IsOwner;

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnNetworkSpawnHook()
        {
            InitServerAndClient();
            InitServer();
            InitClient();
        }

        private void OnNetworkDespawnHook()
        {
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