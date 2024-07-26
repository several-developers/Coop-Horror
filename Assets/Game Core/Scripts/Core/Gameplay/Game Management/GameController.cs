using System;
using System.Collections.Generic;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.CamerasManagement;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Entities.Train;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.PubSub;
using GameCore.Gameplay.PubSub.Messages;
using GameCore.Observers.Gameplay.Game;
using UnityEngine;

namespace GameCore.Gameplay.GameManagement
{
    public class GameController : IDisposable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public GameController(
            IGameManagerDecorator gameManagerDecorator,
            IGameObserver gameObserver,
            ICamerasManager camerasManager,
            ITrainEntity trainEntity,
            IPublisher<UIEventMessage> uiEventMessagePublisher)
        {
            _gameManagerDecorator = gameManagerDecorator;
            _gameObserver = gameObserver;
            _camerasManager = camerasManager;
            _trainEntity = trainEntity;
            _uiEventMessagePublisher = uiEventMessagePublisher;
            
            _gameManagerDecorator.OnGameStateChangedEvent += HandleGameState;

            if (!IsServer)
                return;

            _gameObserver.OnTrainArrivedAtBaseEvent += TrainArrivedAtBase;
            _gameObserver.OnTrainLeavingBaseEvent += TrainLeavingBase;
            _gameObserver.OnTrainArrivedAtSectorEvent += TrainArrivedAtSector;
            _gameObserver.OnTrainStoppedAtSectorEvent += TrainStoppedAtSector;
            _gameObserver.OnTrainLeavingSectorEvent += TrainLeavingSector;
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        private bool IsServer => NetworkHorror.IsTrueServer;

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameManagerDecorator _gameManagerDecorator;
        private readonly IGameObserver _gameObserver;
        private readonly ICamerasManager _camerasManager;
        private readonly ITrainEntity _trainEntity;
        private readonly IPublisher<UIEventMessage> _uiEventMessagePublisher;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Dispose()
        {
            _gameManagerDecorator.OnGameStateChangedEvent -= HandleGameState;

            if (!IsServer)
                return;

            _gameObserver.OnTrainArrivedAtBaseEvent -= TrainArrivedAtBase;
            _gameObserver.OnTrainLeavingBaseEvent -= TrainLeavingBase;
            _gameObserver.OnTrainArrivedAtSectorEvent -= TrainArrivedAtSector;
            _gameObserver.OnTrainStoppedAtSectorEvent -= TrainStoppedAtSector;
            _gameObserver.OnTrainLeavingSectorEvent -= TrainLeavingSector;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        #region Main Logic

        private void HandleGameState(GameState gameState)
        {
            switch (gameState)
            {
                case GameState.GameOver:
                    _camerasManager.SetCameraStatus(CameraStatus.OutsideTrain);
                    PublishUIEvent(UIEventType.HideGameplayHUD);

                    if (IsServer)
                        _gameManagerDecorator.StartGameRestartTimer();
                    break;

                case GameState.RestartGame:
                    PublishUIEvent(UIEventType.ShowGameplayHUD);
                    break;

                case GameState.WaitingForPlayers:
                    break;

                case GameState.Gameplay:
                    break;

                case GameState.QuestsRewarding:
                    break;

                case GameState.KillPlayersByMetroMonster:
                    break;
            }
        }

        private void TrainArrivedAtBase()
        {
            Debug.Log("--> Train Arrived At Base.");

            _trainEntity.TeleportToTheRoad();
            ToggleTrainMainLever(isEnabled: true);
            SetTrainMovementBehaviour(TrainEntity.MovementBehaviour.InfiniteMovement);
            SetPlayersEntityLocation(EntityLocation.Base);
            PublishUIEvent(UIEventType.HideGameTimer);
            PublishUIEvent(UIEventType.UpdateGameMap);
        }

        private void TrainLeavingBase()
        {
            Debug.Log("--> Train Leaving Base.");
        }

        private void TrainArrivedAtSector()
        {
            Debug.Log("--> Train Arrived At Sector.");

            _gameManagerDecorator.SelectLocation(LocationName.Base);
            SetTrainMovementBehaviour(TrainEntity.MovementBehaviour.StopAtPathEnd);
            SetPlayersEntityLocation(EntityLocation.Sector);
        }

        private void TrainStoppedAtSector()
        {
            Debug.Log("--> Train Stopped At Sector.");

            ToggleTrainMainLever(isEnabled: true);
            ToggleTrainDoor(isOpened: true);
            SetTrainMovementBehaviour(TrainEntity.MovementBehaviour.LeaveAtPathEnd);
            PublishUIEvent(UIEventType.ShowGameTimer);
            RemovePlayersParent();
        }

        private void TrainLeavingSector()
        {
            Debug.Log("--> Train Leaving Sector.");

            ToggleTrainMainLever(isEnabled: false);
            ToggleTrainDoor(isOpened: false);
        }

        #endregion

        #region Helper Methods

        private void PublishUIEvent(UIEventType eventType)
        {
            UIEventMessage eventMessage = new()
            {
                UIEventType = eventType
            };

            _uiEventMessagePublisher.Publish(eventMessage);
        }
        
        private void SetTrainMovementBehaviour(TrainEntity.MovementBehaviour movementBehaviour) =>
            _trainEntity.SetMovementBehaviour(movementBehaviour);

        private void ToggleTrainMainLever(bool isEnabled) =>
            _trainEntity.ToggleMainLeverState(isEnabled);

        private void ToggleTrainDoor(bool isOpened) =>
            _trainEntity.ToggleDoorState(isOpened);

        private static void RemovePlayersParent()
        {
            IReadOnlyDictionary<ulong, PlayerEntity> allPlayers = PlayerEntity.GetAllPlayers();

            foreach (PlayerEntity playerEntity in allPlayers.Values)
            {
                bool isDead = playerEntity.IsDead();

                if (isDead)
                    continue;

                playerEntity.TryRemoveParent();
            }
        }

        private static void SetPlayersEntityLocation(EntityLocation entityLocation)
        {
            PlayerEntity localPlayer = PlayerEntity.GetLocalPlayer();
            
            if (localPlayer != null)
                localPlayer.SetEntityLocation(entityLocation);

            // IReadOnlyDictionary<ulong, PlayerEntity> allPlayers = PlayerEntity.GetAllPlayers();
            //
            // foreach (PlayerEntity playerEntity in allPlayers.Values)
            //     playerEntity.SetEntityLocation(entityLocation);
        }
        
        #endregion
    }
}