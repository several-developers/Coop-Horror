using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Dungeons;
using GameCore.Gameplay.Level;

namespace GameCore.Observers.Gameplay.LevelManager
{
    public class LevelProviderObserver : ILevelProviderObserver
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action<Floor, FireExit> OnRegisterStairsFireExitEvent;
        public event Action<Floor, FireExit> OnRegisterOtherFireExitEvent;
        public event Action<DungeonWrapper> OnRegisterDungeonEvent;
        public event Action<Floor, DungeonRoot> OnRegisterDungeonRootEvent;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void RegisterStairsFireExit(Floor floor, FireExit fireExit) =>
            OnRegisterStairsFireExitEvent?.Invoke(floor, fireExit);

        public void RegisterOtherFireExit(Floor floor, FireExit fireExit) =>
            OnRegisterOtherFireExitEvent?.Invoke(floor, fireExit);

        public void RegisterDungeon(DungeonWrapper dungeonWrapper) =>
            OnRegisterDungeonEvent?.Invoke(dungeonWrapper);

        public void RegisterDungeonRoot(Floor floor, DungeonRoot dungeonRoot) =>
            OnRegisterDungeonRootEvent?.Invoke(floor, dungeonRoot);
    }
}