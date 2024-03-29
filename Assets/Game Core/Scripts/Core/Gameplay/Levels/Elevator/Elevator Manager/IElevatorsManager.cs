using System;
using GameCore.Enums.Gameplay;

namespace GameCore.Gameplay.Levels.Elevator
{
    public interface IElevatorsManager
    {
        event Action<ElevatorStaticData> OnElevatorStartedEvent;
        event Action<ElevatorStaticData> OnFloorChangedEvent;
        event Action<Floor> OnElevatorStoppedEvent;
        Floor GetCurrentFloor();
        bool IsElevatorMoving();
    }
}