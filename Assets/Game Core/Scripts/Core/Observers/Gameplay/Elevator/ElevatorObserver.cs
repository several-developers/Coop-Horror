using System;
using GameCore.Enums.Gameplay;

namespace GameCore.Observers.Gameplay.Elevator
{
    public class ElevatorObserver : IElevatorObserver
    {
        public event Action<DungeonIndex> OnDungeonFloorClickedEvent;
        public event Action OnSurfaceFloorClickedEvent;
    }
}