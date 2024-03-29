using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Levels;
using GameCore.Gameplay.Levels.Elevator;

namespace GameCore.Observers.Gameplay.LevelManager
{
    public class LevelProviderObserver : ILevelProviderObserver
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action<ElevatorBase> OnRegisterElevatorEvent;
        public event Action<SurfaceElevator> OnRegisterSurfaceElevatorEvent;
        public event Action<Floor, FireExit> OnRegisterStairsFireExitEvent;
        public event Action<Floor, FireExit> OnRegisterOtherFireExitEvent;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void RegisterSurfaceElevator(SurfaceElevator surfaceElevator) =>
            OnRegisterSurfaceElevatorEvent?.Invoke(surfaceElevator);

        public void RegisterElevator(ElevatorBase elevatorBase) =>
            OnRegisterElevatorEvent?.Invoke(elevatorBase);

        public void RegisterStairsFireExit(Floor floor, FireExit fireExit) =>
            OnRegisterStairsFireExitEvent?.Invoke(floor, fireExit);

        public void RegisterOtherFireExit(Floor floor, FireExit fireExit) =>
            OnRegisterOtherFireExitEvent?.Invoke(floor, fireExit);
    }
}