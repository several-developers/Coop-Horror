using System.Collections;
using System.Collections.Generic;
using GameCore.Configs.Gameplay.Player;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Network.ConnectionManagement;
using GameCore.Gameplay.Network.Other;
using GameCore.Gameplay.Network.Session_Manager;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using GameCore.Utilities;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using Zenject;

namespace GameCore.Gameplay.GameManagement
{
    public class PlayerSpawner : NetworkBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IGameplayConfigsProvider gameplayConfigsProvider) =>
            _playerConfig = gameplayConfigsProvider.GetPlayerConfig();

        // FIELDS: --------------------------------------------------------------------------------

        private PlayerConfigMeta _playerConfig;
        private NetworkManager _networkManager;
        private bool _initialSpawnDone;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SpawnPlayer(ulong clientId, bool lateJoin)
        {
            NetworkObject playerNetworkObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);
            
            Vector3 spawnPosition = GetSpawnPosition();
            PlayerEntity newPlayer = Instantiate(_playerConfig.PlayerPrefab, spawnPosition, Quaternion.identity);
            
            bool persistentPlayerExists = playerNetworkObject.TryGetComponent(out PersistentPlayer persistentPlayer);

            Assert.IsTrue(persistentPlayerExists,
                $"Matching persistent PersistentPlayer for client {clientId} not found!");

            // If reconnecting, set the player's position and rotation to its previous state.
            if (lateJoin)
            {
                SessionPlayerData? sessionPlayerData = SessionManager<SessionPlayerData>.Instance
                    .GetPlayerData(clientId);

                if (sessionPlayerData is { HasCharacterSpawned: true })
                {
                    Vector3 position = sessionPlayerData.Value.PlayerPosition;
                    Quaternion rotation = sessionPlayerData.Value.PlayerRotation;
                    newPlayer.transform.SetPositionAndRotation(position, rotation);
                }
            }

            // Spawn players characters with 'destroyWithScene = true'.
            newPlayer.NetworkObject.SpawnWithOwnership(clientId, destroyWithScene: true);
        }

        private IEnumerator WaitToCheckForGameOver()
        {
            // Wait until next frame so that the client's player character has despawned.
            
            yield return null;
            //CheckForGameOver();
            
            Debug.Log("Game Over");
        }

        private Vector3 GetSpawnPosition()
        {
            bool isSpawnPointFound = PlayerSpawnPoint.GetRandomSpawnPoint(out PlayerSpawnPoint spawnPoint);
            return isSpawnPointFound ? spawnPoint.GetRandomPosition() : transform.GetRandomPosition();
        }
        
        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            _networkManager = NetworkManager.Singleton;

            _networkManager.OnClientDisconnectCallback += OnClientDisconnect;
            _networkManager.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
            _networkManager.SceneManager.OnSynchronizeComplete += OnSynchronizeComplete;

            SessionManager<SessionPlayerData>.Instance.OnSessionStarted();
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            _networkManager.OnClientDisconnectCallback -= OnClientDisconnect;
            _networkManager.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
            _networkManager.SceneManager.OnSynchronizeComplete -= OnSynchronizeComplete;
        }

        private void OnClientConnected(ulong clientId)
        {
            Debug.Log($"Client #{clientId} connected.");
        }

        private void OnClientDisconnect(ulong clientId)
        {
            Debug.Log($"Client #{clientId} disconnect.");
            
            if (clientId != NetworkManager.Singleton.LocalClientId)
            {
                // If a client disconnects, check for game over in case all other players are already down.
                StartCoroutine(WaitToCheckForGameOver());
            }
        }

        private void OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted,
            List<ulong> clientsTimedOut)
        {
            Debug.Log($"Load event completed.");

            if (!_initialSpawnDone && loadSceneMode == LoadSceneMode.Single)
            {
                _initialSpawnDone = true;
                IReadOnlyDictionary<ulong, NetworkClient> connectedClients = _networkManager.ConnectedClients;

                foreach (var pair in connectedClients)
                {
                    Debug.Log($"Player #{pair.Key} ready");

                    SpawnPlayer(pair.Key, lateJoin: false);
                }
            }
        }

        private void OnSynchronizeComplete(ulong clientId)
        {
            Debug.Log($"Client ${clientId} synchronize complete.");

            if (!_initialSpawnDone)
                return;

            Debug.Log($"Late join Player #{clientId} ready");

            // Somebody joined after the initial spawn. This is a Late Join scenario. This player may have issues
            // (either because multiple people are late-joining at once, or because some dynamic entities are
            // getting spawned while joining. But that's not something we can fully address by changes in
            // this script.
            SpawnPlayer(clientId, lateJoin: true);
        }
    }
}