using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Dungeons;
using GameCore.Gameplay.Levels;
using GameCore.Gameplay.Levels.Elevator;

namespace GameCore.Observers.Gameplay.LevelManager
{
    public interface ILevelProviderObserver
    {
        event Action<ElevatorBase> OnRegisterElevatorEvent;
        event Action<SurfaceElevator> OnRegisterSurfaceElevatorEvent;
        event Action<Floor, FireExit> OnRegisterStairsFireExitEvent;
        event Action<Floor, FireExit> OnRegisterOtherFireExitEvent;
        event Action<DungeonWrapper> OnRegisterDungeonEvent;
        event Action<Floor, DungeonRoot> OnRegisterDungeonRootEvent; 

        void RegisterElevator(ElevatorBase elevatorBase);
        void RegisterSurfaceElevator(SurfaceElevator surfaceElevator);
        void RegisterStairsFireExit(Floor floor, FireExit fireExit);
        void RegisterOtherFireExit(Floor floor, FireExit fireExit);
        void RegisterDungeon(DungeonWrapper dungeonWrapper);
        void RegisterDungeonRoot(Floor floor, DungeonRoot dungeonRoot);
    }
}