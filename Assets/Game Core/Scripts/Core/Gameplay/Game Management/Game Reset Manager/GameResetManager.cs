using System;
using System.Collections.Generic;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.MobileHeadquarters;
using GameCore.Gameplay.Entities.Monsters.Beetle;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.GameTimeManagement;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Network.Other;
using GameCore.Gameplay.VisualManagement;
using Unity.Netcode;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameCore.Gameplay.GameManagement
{
    public class GameResetManager : IDisposable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public GameResetManager(IGameManagerDecorator gameManagerDecorator,
            IGameTimeManagerDecorator gameTimeManagerDecorator,
            IMobileHeadquartersEntity mobileHeadquartersEntity,
            IVisualManager visualManager)
        {
            _gameManagerDecorator = gameManagerDecorator;
            _gameTimeManagerDecorator = gameTimeManagerDecorator;
            _mobileHeadquartersEntity = mobileHeadquartersEntity;
            _visualManager = visualManager;

            _gameManagerDecorator.OnGameStateChangedEvent += OnGameStateChanged;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameManagerDecorator _gameManagerDecorator;
        private readonly IGameTimeManagerDecorator _gameTimeManagerDecorator;
        private readonly IMobileHeadquartersEntity _mobileHeadquartersEntity;
        private readonly IVisualManager _visualManager;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Dispose() =>
            _gameManagerDecorator.OnGameStateChangedEvent -= OnGameStateChanged;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void HandleGameState(GameState gameState)
        {
            switch (gameState)
            {
                case GameState.RestartGame:
                    bool isServer = NetworkHorror.IsTrueServer;

                    // Common logic.
                    ChangeVisualPreset();

                    if (isServer)
                    {
                        DestroyAllMonsters();
                        ResetGameTime();
                        ResetGold();
                        TeleportMobileHQToTheRoad();
                        RespawnPlayersAtMobileHQ();
                    }

                    // Common logic.
                    RevivePlayer();
                    break;
            }
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ResetGameTime()
        {
            _gameTimeManagerDecorator.ResetDay();
            _gameTimeManagerDecorator.SetMidnight();
        }

        private void ResetGold() =>
            _gameManagerDecorator.ResetPlayersGold();

        private void TeleportMobileHQToTheRoad() =>
            _mobileHeadquartersEntity.TeleportToTheRoad();

        private void RespawnPlayersAtMobileHQ()
        {
            IReadOnlyDictionary<ulong, PlayerEntity> allPlayers = PlayerEntity.GetAllPlayers();
            NetworkObject parent = _mobileHeadquartersEntity.GetNetworkObject();

            foreach (PlayerEntity playerEntity in allPlayers.Values)
            {
                bool isPlayerValid = playerEntity != null;

                if (!isPlayerValid)
                    continue;

                Vector3 spawnPosition = GetSpawnPosition();
                Vector3 scale = Vector3.one;
                Quaternion rotation = Quaternion.identity;

                playerEntity.Teleport(spawnPosition, rotation);
                playerEntity.NetworkObject.TrySetParent(parent, worldPositionStays: false);
            }

            // LOCAL METHODS: -----------------------------

            Vector3 GetSpawnPosition()
            {
                bool isSpawnPointFound = PlayerSpawnPoint.GetRandomSpawnPoint(out PlayerSpawnPoint spawnPoint);
                return isSpawnPointFound ? spawnPoint.GetRandomPosition(localPosition: true) : Vector3.zero;
            }
        }

        private static void RevivePlayer()
        {
            PlayerEntity localPlayer = PlayerEntity.GetLocalPlayer();
            localPlayer.EnterReviveState();
        }

        private static void DestroyAllMonsters()
        {
            IReadOnlyList<MonsterEntityBase> allMonsters = MonsterEntityBase.GetAllMonsters();

            foreach (MonsterEntityBase monsterEntity in allMonsters)
                Object.Destroy(monsterEntity.gameObject);
        }
        
        private void ChangeVisualPreset() =>
            _visualManager.ChangePreset(VisualPresetType.RoadLocation, instant: true);

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnGameStateChanged(GameState gameState) => HandleGameState(gameState);
    }
}