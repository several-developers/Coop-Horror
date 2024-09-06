using System;
using System.Collections.Generic;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.CamerasManagement;
using GameCore.Gameplay.Dungeons;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Entities.Train;
using GameCore.Gameplay.GameTimeManagement;
using GameCore.Gameplay.MonstersGeneration;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.PubSub;
using GameCore.Gameplay.PubSub.Messages;
using GameCore.Gameplay.Systems.Quests;
using GameCore.Gameplay.VisualManagement;
using GameCore.Observers.Gameplay.Game;
using UnityEngine;

namespace GameCore.Gameplay.GameManagement
{
    public class GameController : IDisposable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public GameController(
            IGameManagerDecorator gameManagerDecorator,
            IGameTimeManagerDecorator gameTimeManagerDecorator,
            IQuestsManagerDecorator questsManagerDecorator,
            IGameObserver gameObserver,
            ICamerasManager camerasManager,
            IGameResetManager gameResetManager,
            IMonstersGenerator monstersGenerator,
            IDungeonsManager dungeonsManager,
            IVisualManager visualManager,
            ITrainEntity trainEntity,
            IPublisher<UIEventMessage> uiEventMessagePublisher)
        {
            _gameManagerDecorator = gameManagerDecorator;
            _gameTimeManagerDecorator = gameTimeManagerDecorator;
            _questsManagerDecorator = questsManagerDecorator;
            _gameObserver = gameObserver;
            _camerasManager = camerasManager;
            _gameResetManager = gameResetManager;
            _monstersGenerator = monstersGenerator;
            _dungeonsManager = dungeonsManager;
            _visualManager = visualManager;
            _trainEntity = trainEntity;
            _uiEventMessagePublisher = uiEventMessagePublisher;

            _gameManagerDecorator.OnGameStateChangedEvent += HandleGameState;

            SubscribeToLogs();

            _gameObserver.OnTrainArrivedAtBaseEvent += TrainArrivedAtBase;
            _gameObserver.OnTrainLeavingBaseEvent += TrainLeavingBase;
            _gameObserver.OnTrainArrivedAtLocationEvent += TrainArrivedAtLocation;
            _gameObserver.OnTrainStoppedAtLocationEvent += TrainStoppedAtLocation;
            _gameObserver.OnTrainLeavingLocationEvent += TrainLeavingLocation;

            // LOCAL METHODS: -----------------------------

            void SubscribeToLogs()
            {
                _gameObserver.OnTrainArrivedAtBaseEvent += location =>
                {
                    string log = Log.HandleLog($"--> Train arrived at <gb>Base</gb> from <gb>{location}</gb>.");
                    Debug.Log(log);
                };

                _gameObserver.OnTrainLeavingBaseEvent += () =>
                {
                    string log = Log.HandleLog("--> Train leaving <gb>Base</gb>.");
                    Debug.Log(log);
                };

                _gameObserver.OnTrainArrivedAtLocationEvent += () =>
                {
                    LocationName currentLocation = GetCurrentLocation();
                    string log = Log.HandleLog($"--> Train arrived at location <gb>{currentLocation}</gb>.");
                    Debug.Log(log);
                };

                _gameObserver.OnTrainStoppedAtLocationEvent += () =>
                {
                    LocationName currentLocation = GetCurrentLocation();
                    string log = Log.HandleLog($"--> Train stopped at location <gb>{currentLocation}</gb>.");
                    Debug.Log(log);
                };

                _gameObserver.OnTrainLeavingLocationEvent += () => { Debug.Log("--> Train Leaving location."); };
            }
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        private static bool IsServer => NetworkHorror.IsTrueServer;

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameManagerDecorator _gameManagerDecorator;
        private readonly IGameTimeManagerDecorator _gameTimeManagerDecorator;
        private readonly IQuestsManagerDecorator _questsManagerDecorator;
        private readonly IGameObserver _gameObserver;
        private readonly ICamerasManager _camerasManager;
        private readonly IGameResetManager _gameResetManager;
        private readonly IMonstersGenerator _monstersGenerator;
        private readonly IDungeonsManager _dungeonsManager;
        private readonly IVisualManager _visualManager;
        private readonly ITrainEntity _trainEntity;
        private readonly IPublisher<UIEventMessage> _uiEventMessagePublisher;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Dispose()
        {
            _gameManagerDecorator.OnGameStateChangedEvent -= HandleGameState;

            _gameObserver.OnTrainArrivedAtBaseEvent -= TrainArrivedAtBase;
            _gameObserver.OnTrainLeavingBaseEvent -= TrainLeavingBase;
            _gameObserver.OnTrainArrivedAtLocationEvent -= TrainArrivedAtLocation;
            _gameObserver.OnTrainStoppedAtLocationEvent -= TrainStoppedAtLocation;
            _gameObserver.OnTrainLeavingLocationEvent -= TrainLeavingLocation;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        #region Main Logic

        private void HandleGameState(GameState gameState)
        {
            switch (gameState)
            {
                case GameState.GameOver:
                    HandleGameOver();
                    break;

                case GameState.RestartGame:
                    HandleRestartGame();
                    break;

                case GameState.WaitingForPlayers:
                    break;

                case GameState.Gameplay:
                    break;

                case GameState.QuestsRewarding:
                    HandleQuestsRewarding();
                    break;

                case GameState.KillPlayersByMetroMonster:
                    break;
            }

            // LOCAL METHODS: -----------------------------

            void HandleGameOver()
            {
                _camerasManager.SetCameraStatus(CameraStatus.OutsideTrain);
                PublishUIEvent(UIEventType.HideGameplayHUD);

                if (!IsServer)
                    return;

                _gameManagerDecorator.StartGameRestartTimer();
                _monstersGenerator.Stop();
            }

            void HandleRestartGame()
            {
                PublishUIEvent(UIEventType.ShowGameplayHUD);
                _gameResetManager.Reset();

                if (!IsServer)
                    return;

                _gameManagerDecorator.ChangeGameStateWhenAllPlayersReady(newState: GameState.Gameplay,
                    previousState: GameState.RestartGame);
            }

            void HandleQuestsRewarding()
            {
                _questsManagerDecorator.CalculateReward();
                // Create quests
            }
        }

        private void TrainArrivedAtBase(LocationName previousLocationName)
        {
            if (IsServer)
            {
                bool arrivedFromMarket = previousLocationName == LocationName.Market;

                _gameTimeManagerDecorator.SetMidnight();
                _trainEntity.TeleportToTheRoad();

                ToggleTrainMainLever(isEnabled: true);
                SetTrainMovementBehaviour(TrainEntity.MovementBehaviour.InfiniteMovement);
                SetPlayersEntityLocation(EntityLocation.Base);
                PublishUIEvent(UIEventType.HideGameTimer);
                PublishUIEvent(UIEventType.UpdateGameMap);
                _monstersGenerator.Stop();

                if (!arrivedFromMarket)
                    _questsManagerDecorator.DecreaseQuestsDays();

                ChangeVisualPresetForAll(VisualPresetType.Metro);
            }
        }

        private void TrainLeavingBase()
        {
            if (IsServer)
            {
                _dungeonsManager.GenerateDungeonsOnAllClients();
            }
        }

        private void TrainArrivedAtLocation()
        {
            LocationName currentLocation = GetCurrentLocation();
            bool isGameplayLocation = currentLocation != LocationName.Market;

            if (IsServer)
            {
                if (isGameplayLocation)
                {
                    _gameTimeManagerDecorator.SetSunrise();
                    _gameTimeManagerDecorator.IncreaseDay();
                }

                _trainEntity.TeleportToTheMetroPlatform();
                SetTrainMovementBehaviour(TrainEntity.MovementBehaviour.StopAtPathEnd);
                SetPlayersEntityLocation(EntityLocation.Surface);
                _trainEntity.PlaySound(TrainEntity.SFXType.Arrival);
                _monstersGenerator.Start();
            }

            if (isGameplayLocation)
                _visualManager.SetLocationPresetForAll();
        }

        private void TrainStoppedAtLocation()
        {
            if (IsServer)
            {
                LocationName currentLocation = GetCurrentLocation();

                if (currentLocation != LocationName.Market)
                {
                    PublishUIEvent(UIEventType.ShowGameTimer);
                }

                ToggleTrainMainLever(isEnabled: true);
                ToggleTrainDoor(isOpened: true);
                SetTrainMovementBehaviour(TrainEntity.MovementBehaviour.LeaveAtPathEnd);
                _gameManagerDecorator
                    .SelectLocation(LocationName.Base); // TEMP, возможно данжи не успеют сгенерироваться
                RemovePlayersParent();
                _trainEntity.ToggleStoppedAtSectorState(true);
                _trainEntity.StopSound(TrainEntity.SFXType.MovementLoop);
                _trainEntity.StopSound(TrainEntity.SFXType.Arrival);
            }
        }

        private void TrainLeavingLocation()
        {
            if (IsServer)
            {
                ToggleTrainMainLever(isEnabled: false);
                ToggleTrainDoor(isOpened: false);
                _trainEntity.ToggleStoppedAtSectorState(false);
                _trainEntity.PlaySound(TrainEntity.SFXType.Departure);
                _trainEntity.PlaySound(TrainEntity.SFXType.MovementLoop);
            }
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
            IReadOnlyDictionary<ulong, PlayerEntity> allPlayers = GetAllPlayers();

            foreach (PlayerEntity playerEntity in allPlayers.Values)
            {
                bool isDead = playerEntity.IsDead();

                if (isDead)
                    continue;

                playerEntity.RemoveParent();
            }
        }

        private static void SetPlayersEntityLocation(EntityLocation entityLocation)
        {
            IReadOnlyDictionary<ulong, PlayerEntity> allPlayers = GetAllPlayers();

            foreach (PlayerEntity playerEntity in allPlayers.Values)
                playerEntity.SetEntityLocation(entityLocation);
        }
        
        private void ChangeVisualPresetForAll(VisualPresetType presetType) =>
            _visualManager.ChangePresetForAll(presetType, instant: true);

        private static IReadOnlyDictionary<ulong, PlayerEntity> GetAllPlayers() =>
            PlayerEntity.GetAllPlayers();

        private LocationName GetCurrentLocation() =>
            _gameManagerDecorator.GetCurrentLocation();

        #endregion
    }
}