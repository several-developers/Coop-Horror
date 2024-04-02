using System;
using GameCore.Enums.Gameplay;

namespace GameCore.Gameplay.Levels.Elevator
{
    public interface IElevatorsManagerDecorator
    {
        event Action<ElevatorStaticData> OnElevatorStartedEvent;
        event Action<ElevatorStaticData> OnFloorChangedEvent;
        event Action<Floor> OnElevatorStoppedEvent;

        event Func<Floor> OnGetCurrentFloorEvent;
        event Func<bool> OnIsElevatorMovingEvent;

        void ElevatorStarted(ElevatorStaticData data);
        void FloorChanged(ElevatorStaticData data);
        void ElevatorStopped(Floor floor);
        
        Floor GetCurrentFloor();
        bool IsElevatorMoving();
    }
}