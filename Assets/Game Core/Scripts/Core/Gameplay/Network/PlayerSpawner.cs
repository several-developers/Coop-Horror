using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Configs.Gameplay.Player;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.CamerasManagement;
using GameCore.Gameplay.Entities.Train;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Network.ConnectionManagement;
using GameCore.Gameplay.Network.Session_Manager;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using GameCore.Utilities;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using Zenject;

namespace GameCore.Gameplay.Network
{
    public class PlayerSpawner : NetworkBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(ITrainEntity trainEntity, ICamerasManager camerasManager,
            IGameplayConfigsProvider gameplayConfigsProvider)
        {
            _trainEntity = trainEntity;
            _camerasManager = camerasManager;
            _playerConfig = gameplayConfigsProvider.GetPlayerConfig();
        }

        // FIELDS: --------------------------------------------------------------------------------

        private const bool SpawnAsChild = true;

        private ITrainEntity _trainEntity;
        private ICamerasManager _camerasManager;
        private PlayerConfigMeta _playerConfig;
        private NetworkManager _networkManager;
        private bool _initialSpawnDone;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async void SpawnPlayer(ulong clientID, bool lateJoin)
        {
            bool isCanceled = await UniTask
                .DelayFrame(delayFrameCount: 1, cancellationToken: this.GetCancellationTokenOnDestroy())
                .SuppressCancellationThrow();

            if (isCanceled)
                return;

            SpawnPlayerLogic(clientID, lateJoin);
            SetCameraFirstPersonStatus();
        }

        private void SpawnPlayerLogic(ulong clientID, bool lateJoin)
        {
            NetworkObject playerNetworkObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientID);

            Vector3 spawnPosition = GetSpawnPosition();
            PlayerEntity playerEntityPrefab = _playerConfig.PlayerPrefab;
            NetworkObject playerNetworkObjectPrefab = playerEntityPrefab.GetComponent<NetworkObject>();

            //PlayerEntity playerInstance = Instantiate(_playerConfig.PlayerPrefab, spawnPosition, Quaternion.identity);
            NetworkObject playerNetworkObjectInstance = _networkManager.SpawnManager
                .InstantiateAndSpawn(playerNetworkObjectPrefab, clientID, position: spawnPosition);

            bool isPlayerEntityFound = playerNetworkObjectInstance.TryGetComponent(out PlayerEntity playerInstance);

            if (!isPlayerEntityFound)
            {
                Log.PrintError(log: $"<gb>Player Entity</gb> <rb>not found</rb>!");
                return;
            }

            bool persistentPlayerExists = playerNetworkObject.TryGetComponent(out PersistentPlayer _);

            Assert.IsTrue(persistentPlayerExists,
                $"Matching persistent PersistentPlayer for client {clientID} not found!");

            // If reconnecting, set the player's position and rotation to its previous state.
            if (lateJoin)
            {
                SessionPlayerData? sessionPlayerData = SessionManager<SessionPlayerData>.Instance
                    .GetPlayerData(clientID);

                if (sessionPlayerData is { HasCharacterSpawned: true })
                {
                    Vector3 position = sessionPlayerData.Value.PlayerPosition;
                    Quaternion rotation = sessionPlayerData.Value.PlayerRotation;
                    playerInstance.transform.SetPositionAndRotation(position, rotation);
                }
            }

            if (SpawnAsChild)
            {
                NetworkObject parent = _trainEntity.GetNetworkObject();
                playerInstance.NetworkObject.TrySetParent(parent, worldPositionStays: false);
            }

            // Spawn players characters with 'destroyWithScene = true'.
            //playerInstance.NetworkObject.SpawnWithOwnership(clientId, destroyWithScene: true);
        }

        private void SetCameraFirstPersonStatus() =>
            _camerasManager.SetCameraStatus(CameraStatus.FirstPerson);

        private Vector3 GetSpawnPosition()
        {
            bool isSpawnPointFound = VehicleSeatSpawnPoint.GetRandomSpawnPoint(out VehicleSeatSpawnPoint spawnPoint);
            return isSpawnPointFound ? spawnPoint.GetSpawnPosition() : transform.GetRandomPosition();
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

            if (!NetworkHorror.IsTrueServer)
                return;

            // If a client disconnects, check for game over in case all other players are already down.
            //StartCoroutine(WaitToCheckForGameOver());
        }

#warning Перенести в отдельный скрипт
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