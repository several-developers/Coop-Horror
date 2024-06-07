using System;
using System.Collections.Generic;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Dungeons;
using GameCore.Gameplay.Level.Elevator;
using GameCore.Observers.Gameplay.LevelManager;

namespace GameCore.Gameplay.Level
{
    public class LevelProvider : ILevelProvider, IDisposable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public LevelProvider(ILevelProviderObserver levelProviderObserver)
        {
            _levelProviderObserver = levelProviderObserver;
            _elevatorsReferences = new Dictionary<Floor, ElevatorBase>(capacity: 4);
            _stairsFireExits = new Dictionary<Floor, FireExit>(capacity: 4);
            _otherFireExits = new Dictionary<Floor, FireExit>(capacity: 4);
            _dungeons = new Dictionary<Floor, DungeonWrapper>(capacity: 3);
            _dungeonRoots = new Dictionary<Floor, DungeonRoot>(capacity: 3);

            _levelProviderObserver.OnRegisterElevatorEvent += OnRegisterElevator;
            _levelProviderObserver.OnRegisterSurfaceElevatorEvent += OnRegisterSurfaceElevator;
            _levelProviderObserver.OnRegisterStairsFireExitEvent += OnRegisterStairsFireExit;
            _levelProviderObserver.OnRegisterOtherFireExitEvent += OnRegisterOtherFireExit;
            _levelProviderObserver.OnRegisterDungeonEvent += OnRegisterDungeon;
            _levelProviderObserver.OnRegisterDungeonRootEvent += OnRegisterDungeonRoot;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly ILevelProviderObserver _levelProviderObserver;
        private readonly Dictionary<Floor, ElevatorBase> _elevatorsReferences;
        private readonly Dictionary<Floor, FireExit> _stairsFireExits;
        private readonly Dictionary<Floor, FireExit> _otherFireExits;
        private readonly Dictionary<Floor, DungeonWrapper> _dungeons;
        private readonly Dictionary<Floor, DungeonRoot> _dungeonRoots;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Dispose()
        {
            _levelProviderObserver.OnRegisterElevatorEvent -= OnRegisterElevator;
            _levelProviderObserver.OnRegisterSurfaceElevatorEvent -= OnRegisterSurfaceElevator;
            _levelProviderObserver.OnRegisterStairsFireExitEvent -= OnRegisterStairsFireExit;
            _levelProviderObserver.OnRegisterOtherFireExitEvent -= OnRegisterOtherFireExit;
            _levelProviderObserver.OnRegisterDungeonEvent -= OnRegisterDungeon;
            _levelProviderObserver.OnRegisterDungeonRootEvent -= OnRegisterDungeonRoot;
        }

        public void Clear()
        {
            _elevatorsReferences.Clear();
            _otherFireExits.Clear();
        }

        public bool TryGetElevator(Floor floor, out ElevatorBase elevator)
        {
            if (_elevatorsReferences.TryGetValue(floor, out elevator))
                return true;

            Log.PrintError(log: $"Elevator <gb>'{floor}'</gb> <rb>not found</rb>!");
            return false;
        }

        public bool TryGetStairsFireExit(Floor floor, out FireExit fireExit)
        {
            if (_stairsFireExits.TryGetValue(floor, out fireExit))
                return true;

            Log.PrintError(log: $"Stairs Fire Exit <gb>'{floor}'</gb> <rb>not found</rb>!");
            return false;
        }

        public bool TryGetOtherFireExit(Floor floor, out FireExit fireExit)
        {
            if (_otherFireExits.TryGetValue(floor, out fireExit))
                return true;

            Log.PrintError(log: $"Other Fire Exit <gb>'{floor}'</gb> <rb>not found</rb>!");
            return false;
        }

        public bool TryGetDungeon(Floor floor, out DungeonWrapper dungeonWrapper)
        {
            if (_dungeons.TryGetValue(floor, out dungeonWrapper))
                return true;

            Log.PrintError(log: $"Dungeon <gb>'{floor}'</gb> <rb>not found</rb>!");
            return false;
        }

        public bool TryGetDungeonRoot(Floor floor, out DungeonRoot dungeonRoot)
        {
            if (_dungeonRoots.TryGetValue(floor, out dungeonRoot))
                return true;

            Log.PrintError(log: $"Dungeon Root <gb>'{floor}'</gb> <rb>not found</rb>!");
            return false;
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnRegisterElevator(ElevatorBase elevatorBase)
        {
            Floor floor = elevatorBase.GetElevatorFloor();
            bool isSuccessfulAdded = _elevatorsReferences.TryAdd(floor, elevatorBase);

            if (isSuccessfulAdded)
                return;

            Log.PrintError(log: $"Elevator with floor <gb>{floor}</gb> is already added!");
        }

        private void OnRegisterSurfaceElevator(SurfaceElevator surfaceElevator)
        {
            bool isSuccessfulAdded = _elevatorsReferences.TryAdd(Floor.Surface, surfaceElevator);
            
            if (isSuccessfulAdded)
                return;

            Log.PrintError(log: "Surface Elevator is already added!");
        }

        private void OnRegisterStairsFireExit(Floor floor, FireExit fireExit)
        {
            bool isSuccessfulAdded = _stairsFireExits.TryAdd(floor, fireExit);
            
            if (isSuccessfulAdded)
                return;

            Log.PrintError(log: $"Stairs Fire Exit <gb>{floor}</gb> is already added!");
        }
        
        private void OnRegisterOtherFireExit(Floor floor, FireExit fireExit)
        {
            bool isSuccessfulAdded = _otherFireExits.TryAdd(floor, fireExit);
            
            if (isSuccessfulAdded)
                return;

            Log.PrintError(log: $"Other Fire Exit <gb>{floor}</gb> is already added!");
        }
        
        private void OnRegisterDungeon(DungeonWrapper dungeonWrapper)
        {
            Floor floor = dungeonWrapper.GetFloor();
            bool isSuccessfulAdded = _dungeons.TryAdd(floor, dungeonWrapper);
            
            if (isSuccessfulAdded)
                return;

            Log.PrintError(log: $"Dungeon <gb>{floor}</gb> is already added!");
        }
        
        private void OnRegisterDungeonRoot(Floor floor, DungeonRoot dungeonRoot)
        {
            bool isSuccessfulAdded = _dungeonRoots.TryAdd(floor, dungeonRoot);
            
            if (isSuccessfulAdded)
                return;

            Log.PrintError(log: $"Dungeon Root <gb>{floor}</gb> is already added!");
        }
    }
}