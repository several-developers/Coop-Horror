using System;
using GameCore.Enums.Gameplay;

namespace GameCore.Observers.Gameplay.Elevator
{
    public interface IElevatorObserver
    {
        event Action<DungeonIndex> OnDungeonFloorClickedEvent;
        event Action OnSurfaceFloorClickedEvent;
    }
}