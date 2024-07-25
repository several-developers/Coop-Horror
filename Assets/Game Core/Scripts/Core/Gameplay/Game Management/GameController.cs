using System;
using System.Collections.Generic;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Entities.Train;
using GameCore.Observers.Gameplay.Game;
using UnityEngine;

namespace GameCore.Gameplay.GameManagement
{
    public class GameController : IDisposable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public GameController(
            IGameObserver gameObserver,
            IGameManagerDecorator gameManagerDecorator,
            ITrainEntity trainEntity
        )
        {
            _gameObserver = gameObserver;
            _gameManagerDecorator = gameManagerDecorator;
            _trainEntity = trainEntity;

            _gameObserver.OnTrainArrivedAtBaseEvent += TrainArrivedAtBase;
            _gameObserver.OnTrainLeavingBaseEvent += TrainLeavingBase;
            _gameObserver.OnTrainArrivedAtSectorEvent += TrainArrivedAtSector;
            _gameObserver.OnTrainStoppedAtSectorEvent += TrainStoppedAtSector;
            _gameObserver.OnTrainLeavingSectorEvent += TrainLeavingSector;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameObserver _gameObserver;
        private readonly IGameManagerDecorator _gameManagerDecorator;
        private readonly ITrainEntity _trainEntity;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Dispose()
        {
            _gameObserver.OnTrainArrivedAtBaseEvent -= TrainArrivedAtBase;
            _gameObserver.OnTrainLeavingBaseEvent -= TrainLeavingBase;
            _gameObserver.OnTrainArrivedAtSectorEvent -= TrainArrivedAtSector;
            _gameObserver.OnTrainStoppedAtSectorEvent -= TrainStoppedAtSector;
            _gameObserver.OnTrainLeavingSectorEvent -= TrainLeavingSector;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void TrainArrivedAtBase()
        {
            Debug.Log("--> Train Arrived At Base.");

            ToggleTrainMainLever(isEnabled: true);
            SetTrainMovementBehaviour(TrainEntity.MovementBehaviour.InfiniteMovement);
        }

        private void TrainLeavingBase()
        {
            Debug.Log("--> Train Leaving Base.");

            ToggleTrainMainLever(isEnabled: false);
        }

        private void TrainArrivedAtSector()
        {
            Debug.Log("--> Train Arrived At Sector.");

            _gameManagerDecorator.SelectLocation(LocationName.Base);
            SetTrainMovementBehaviour(TrainEntity.MovementBehaviour.StopAtPathEnd);
        }

        private void TrainStoppedAtSector()
        {
            Debug.Log("--> Train Stopped At Sector.");

            ToggleTrainMainLever(isEnabled: true);
            ToggleTrainDoor(isOpened: true);
            SetTrainMovementBehaviour(TrainEntity.MovementBehaviour.LeaveAtPathEnd);

            RemovePlayersParent();
        }

        private void TrainLeavingSector()
        {
            Debug.Log("--> Train Leaving Sector.");

            ToggleTrainMainLever(isEnabled: false);
            ToggleTrainDoor(isOpened: false);
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
    }
}