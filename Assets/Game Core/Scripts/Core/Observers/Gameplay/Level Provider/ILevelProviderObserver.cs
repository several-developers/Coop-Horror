using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Dungeons;
using GameCore.Gameplay.Level;

namespace GameCore.Observers.Gameplay.LevelManager
{
    public interface ILevelProviderObserver
    {
        event Action<Floor, FireExit> OnRegisterStairsFireExitEvent;
        event Action<Floor, FireExit> OnRegisterOtherFireExitEvent;
        event Action<DungeonWrapper> OnRegisterDungeonEvent;
        event Action<Floor, DungeonRoot> OnRegisterDungeonRootEvent; 

        void RegisterStairsFireExit(Floor floor, FireExit fireExit);
        void RegisterOtherFireExit(Floor floor, FireExit fireExit);
        void RegisterDungeon(DungeonWrapper dungeonWrapper);
        void RegisterDungeonRoot(Floor floor, DungeonRoot dungeonRoot);
    }
}