using System.Collections.Generic;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Monsters;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Entities.Train;
using GameCore.Gameplay.GameTimeManagement;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Systems.Quests;
using GameCore.Gameplay.VisualManagement;
using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.GameManagement
{
    public class GameResetManager : IGameResetManager
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public GameResetManager(
            IGameManagerDecorator gameManagerDecorator,
            IGameTimeManagerDecorator gameTimeManagerDecorator,
            IQuestsManagerDecorator questsManagerDecorator,
            ITrainEntity trainEntity,
            IVisualManager visualManager
            )
        {
            _gameManagerDecorator = gameManagerDecorator;
            _gameTimeManagerDecorator = gameTimeManagerDecorator;
            _questsManagerDecorator = questsManagerDecorator;
            _trainEntity = trainEntity;
            _visualManager = visualManager;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameManagerDecorator _gameManagerDecorator;
        private readonly IGameTimeManagerDecorator _gameTimeManagerDecorator;
        private readonly IQuestsManagerDecorator _questsManagerDecorator;
        private readonly ITrainEntity _trainEntity;
        private readonly IVisualManager _visualManager;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void Reset()
        {
            bool isServer = NetworkHorror.IsTrueServer;

            // Common logic.
            ChangeVisualPreset();

            if (isServer)
            {
                DestroyAllMonsters();
                ResetGameTime();
                ResetGold();
                TeleportTrainToTheRoad();
                ResetTrain();
                RespawnPlayersAtTrain();
                TeleportAllPlayersToRandomSeats();
                ResetQuests();
                _gameManagerDecorator.SelectLocation(LocationName.Base);
                _trainEntity.SendLeaveLocation();
            }

            // Common logic.
            RevivePlayer();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ResetGameTime()
        {
            _gameTimeManagerDecorator.ResetDay();
            _gameTimeManagerDecorator.SetMidnight();
        }

        private void ResetGold() =>
            _gameManagerDecorator.ResetPlayersGold();

        private void TeleportTrainToTheRoad() =>
            _trainEntity.TeleportToTheRoad();

        private void ResetTrain()
        {
            _trainEntity.ToggleMainLeverState(isEnabled: true);
            _trainEntity.ToggleDoorState(isOpened: false);
            _trainEntity.ToggleStoppedAtSectorState(false);
            _trainEntity.SetMovementBehaviour(TrainEntity.MovementBehaviour.InfiniteMovement);
            _trainEntity.PlaySound(TrainEntity.SFXType.MovementLoop); // TEMP
        }
        
        private void RespawnPlayersAtTrain()
        {
            IReadOnlyDictionary<ulong, PlayerEntity> allPlayers = PlayerEntity.GetAllPlayers();
            NetworkObject parent = _trainEntity.GetNetworkObject();

            foreach (PlayerEntity playerEntity in allPlayers.Values)
            {
                bool isPlayerValid = playerEntity != null;

                if (!isPlayerValid)
                    continue;

                Vector3 spawnPosition = GetSpawnPosition();
                Quaternion rotation = Quaternion.identity;

                playerEntity.Teleport(spawnPosition, rotation, resetVelocity: true);
                playerEntity.NetworkObject.TrySetParent(parent, worldPositionStays: false);
            }

            // LOCAL METHODS: -----------------------------

            Vector3 GetSpawnPosition()
            {
                bool isSpawnPointFound =
                    VehicleSeatSpawnPoint.GetRandomSpawnPoint(out VehicleSeatSpawnPoint spawnPoint);
                
                return isSpawnPointFound ? spawnPoint.GetSpawnPosition() : Vector3.zero;
            }
        }

        private void TeleportAllPlayersToRandomSeats() =>
            _trainEntity.TeleportAllPlayersToRandomSeats(ignoreChecks: true);

        private void ResetQuests() =>
            _questsManagerDecorator.ResetQuests();

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
            
            MonsterEntityBase.ClearAllMonsters();
        }

        private void ChangeVisualPreset() =>
            _visualManager.ChangePreset(VisualPresetType.Metro, instant: true);
    }
}