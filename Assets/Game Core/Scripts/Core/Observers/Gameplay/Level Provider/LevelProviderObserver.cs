using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Dungeons;
using GameCore.Gameplay.Level;
using GameCore.Gameplay.Level.Elevator;
using GameCore.Gameplay.Level.Locations;

namespace GameCore.Observers.Gameplay.LevelManager
{
    public class LevelProviderObserver : ILevelProviderObserver
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action<ElevatorBase> OnRegisterElevatorEvent;
        public event Action<SurfaceElevator> OnRegisterSurfaceElevatorEvent;
        public event Action<Floor, FireExit> OnRegisterStairsFireExitEvent;
        public event Action<Floor, FireExit> OnRegisterOtherFireExitEvent;
        public event Action<DungeonWrapper> OnRegisterDungeonEvent;
        public event Action<Floor, DungeonRoot> OnRegisterDungeonRootEvent;
        public event Action<MetroDoor, bool> OnRegisterMetroDoorEvent;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void RegisterSurfaceElevator(SurfaceElevator surfaceElevator) =>
            OnRegisterSurfaceElevatorEvent?.Invoke(surfaceElevator);

        public void RegisterElevator(ElevatorBase elevatorBase) =>
            OnRegisterElevatorEvent?.Invoke(elevatorBase);

        public void RegisterStairsFireExit(Floor floor, FireExit fireExit) =>
            OnRegisterStairsFireExitEvent?.Invoke(floor, fireExit);

        public void RegisterOtherFireExit(Floor floor, FireExit fireExit) =>
            OnRegisterOtherFireExitEvent?.Invoke(floor, fireExit);

        public void RegisterDungeon(DungeonWrapper dungeonWrapper) =>
            OnRegisterDungeonEvent?.Invoke(dungeonWrapper);

        public void RegisterDungeonRoot(Floor floor, DungeonRoot dungeonRoot) =>
            OnRegisterDungeonRootEvent?.Invoke(floor, dungeonRoot);

        public void RegisterMetroDoor(MetroDoor metroDoor, bool placedAtSurface) =>
            OnRegisterMetroDoorEvent?.Invoke(metroDoor, placedAtSurface);
    }
}