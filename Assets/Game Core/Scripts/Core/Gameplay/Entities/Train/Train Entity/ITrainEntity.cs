using System;
using Cinemachine;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Train
{
    public interface ITrainEntity : IEntity, INetworkObject
    {
        event Action OnMovementStoppedEvent;
        event Action OnMovementStartedEvent;
        event Action OnLeaveLocationEvent;
        event Action OnOpenQuestsSelectionMenuEvent;
        event Action OnOpenGameOverWarningMenuEvent;
        event Action OnOpenGameMapEvent;

        void ChangePath(CinemachinePath path, float startDistancePercent = 0f, bool stayAtSamePosition = false);
        void SetMovementBehaviour(TrainEntity.MovementBehaviour movementBehaviour);
        void TeleportToTheRoad();
        void TeleportToTheMetroPlatform();
        void TeleportAllPlayersToRandomSeats(bool ignoreChecks = false);
        void TeleportLocalPlayerToTrainSeat(int seatIndex);
        void ToggleMainLeverState(bool isEnabled);
        void ToggleDoorState(bool isOpened);
        void ToggleStoppedAtSectorState(bool isStoppedAtSector);
        void PlaySound(TrainEntity.SFXType sfxType, bool onlyLocal = false);
        void StopSound(TrainEntity.SFXType sfxType);
        void SendLeaveLocation();
        Camera GetOutsideCamera();
    }
}