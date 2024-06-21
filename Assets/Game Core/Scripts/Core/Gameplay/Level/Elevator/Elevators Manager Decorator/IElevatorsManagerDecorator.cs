using System;
using GameCore.Enums.Gameplay;
using UnityEngine;

namespace GameCore.Gameplay.Level.Elevator
{
    public interface IElevatorsManagerDecorator
    {
        event Action<ElevatorStaticData> OnElevatorStartedEvent;
        event Action<ElevatorStaticData> OnFloorChangedEvent;
        event Action<Floor> OnElevatorStoppedEvent;
        event Action<Floor> OnElevatorOpenedEvent;

        event Action<Floor> OnStartElevatorInnerEvent;
        event Action<Floor> OnOpenElevatorInnerEvent;
        event Action<Transform> OnTeleportLocalPlayerInnerEvent;
        event Func<Floor> GetCurrentFloorInnerEvent;
        event Func<bool> IsElevatorMovingInnerEvent;

        void ElevatorStarted(ElevatorStaticData data);
        void FloorChanged(ElevatorStaticData data);
        void ElevatorStopped(Floor floor);
        void ElevatorOpened(Floor floor);

        void StartElevator(Floor floor);
        void OpenElevator(Floor floor);
        void TeleportLocalPlayer(Transform elevatorTransform);
        Floor GetCurrentFloor();
        bool IsElevatorMoving();
    }
}