﻿using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.CamerasManagement;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Entities.Train;
using GameCore.Gameplay.Factories.Entities;
using GameCore.Gameplay.Network.ConnectionManagement;
using GameCore.Gameplay.Network.SessionManagement;
using GameCore.Gameplay.Network.Utilities;
using GameCore.Gameplay.Utilities;
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
        private void Construct(
            IEntitiesFactory entitiesFactory,
            ITrainEntity trainEntity,
            ICamerasManager camerasManager
        )
        {
            _entitiesFactory = entitiesFactory;
            _trainEntity = trainEntity;
            _camerasManager = camerasManager;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private const bool SpawnAsChild = true;

        private IEntitiesFactory _entitiesFactory;
        private ITrainEntity _trainEntity;
        private ICamerasManager _camerasManager;

        private NetworkManager _networkManager;
        private bool _initialSpawnDone;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async UniTaskVoid LoadAndCreatePlayer(ulong clientID, bool lateJoin)
        {
            bool isCanceled = await UniTask
                .DelayFrame(delayFrameCount: 1, cancellationToken: this.GetCancellationTokenOnDestroy())
                .SuppressCancellationThrow();

            if (isCanceled)
                return;

            while (!NetworkPrefabsRegistrar.IsPrefabsRegistered)
            {
                isCanceled = await UniTask
                    .Delay(millisecondsDelay: 50, cancellationToken: this.GetCancellationTokenOnDestroy())
                    .SuppressCancellationThrow();

                if (isCanceled)
                    return;
            }
            
            isCanceled = await UniTask
                .Delay(millisecondsDelay: 5000, cancellationToken: this.GetCancellationTokenOnDestroy())
                .SuppressCancellationThrow();

            if (isCanceled)
                return;
            
            Vector3 spawnPosition = Vector3.zero; // TEMP ?

            var spawnParams = new EntitySpawnParams<PlayerEntity>.Builder()
                .SetSpawnPosition(spawnPosition)
                .SetOwnerID(clientID)
                .SetSuccessCallback(playerEntity => { SetupPlayer(playerEntity, clientID, lateJoin); })
                .Build();

            await _entitiesFactory.CreateEntity(spawnParams);
        }

        private void SetupPlayer(PlayerEntity playerEntity, ulong clientID, bool lateJoin)
        {
            NetworkObject playerNetworkObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientID);

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
                    playerEntity.transform.SetPositionAndRotation(position, rotation);
                }
            }

            if (SpawnAsChild)
            {
                NetworkObject parent = _trainEntity.GetNetworkObject();
                playerEntity.NetworkObject.TrySetParent(parent, worldPositionStays: false);
            }

            SetCameraFirstPersonStatus(); // TEMP ?

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

                    LoadAndCreatePlayer(pair.Key, lateJoin: false);
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
            LoadAndCreatePlayer(clientId, lateJoin: true);
        }
    }
}