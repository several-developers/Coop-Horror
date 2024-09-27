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
            _elevatorMovePoints = new Dictionary<Floor, ElevatorMovePoint>(capacity: 4);
            _stairsFireExits = new Dictionary<Floor, FireExit>(capacity: 4);
            _otherFireExits = new Dictionary<Floor, FireExit>(capacity: 4);
            _dungeons = new Dictionary<Floor, DungeonWrapper>(capacity: 3);
            _dungeonRoots = new Dictionary<Floor, DungeonRoot>(capacity: 3);

            _levelProviderObserver.OnRegisterElevatorMovePointEvent += OnRegisterElevatorMovePoint;
            _levelProviderObserver.OnRegisterStairsFireExitEvent += OnRegisterStairsFireExit;
            _levelProviderObserver.OnRegisterOtherFireExitEvent += OnRegisterOtherFireExit;
            _levelProviderObserver.OnRegisterDungeonEvent += OnRegisterDungeon;
            _levelProviderObserver.OnRegisterDungeonRootEvent += OnRegisterDungeonRoot;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly ILevelProviderObserver _levelProviderObserver;
        private readonly Dictionary<Floor, ElevatorMovePoint> _elevatorMovePoints;
        private readonly Dictionary<Floor, FireExit> _stairsFireExits;
        private readonly Dictionary<Floor, FireExit> _otherFireExits;
        private readonly Dictionary<Floor, DungeonWrapper> _dungeons;
        private readonly Dictionary<Floor, DungeonRoot> _dungeonRoots;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Dispose()
        {
            _levelProviderObserver.OnRegisterElevatorMovePointEvent -= OnRegisterElevatorMovePoint;
            _levelProviderObserver.OnRegisterStairsFireExitEvent -= OnRegisterStairsFireExit;
            _levelProviderObserver.OnRegisterOtherFireExitEvent -= OnRegisterOtherFireExit;
            _levelProviderObserver.OnRegisterDungeonEvent -= OnRegisterDungeon;
            _levelProviderObserver.OnRegisterDungeonRootEvent -= OnRegisterDungeonRoot;
        }

        public void ClearLevel()
        {
            _elevatorMovePoints.Clear();
            _otherFireExits.Clear();
        }

        public bool TryGetElevatorMovePoint(Floor floor, out ElevatorMovePoint elevatorMovePoint)
        {
            if (_elevatorMovePoints.TryGetValue(floor, out elevatorMovePoint))
                return true;

            Log.PrintError(log: $"Elevator Move Point <gb>'{floor}'</gb> <rb>not found</rb>!");
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

        private void OnRegisterElevatorMovePoint(ElevatorMovePoint elevatorMovePoint)
        {
            Floor floor = elevatorMovePoint.GetFloor();
            bool isSuccessfulAdded = _elevatorMovePoints.TryAdd(floor, elevatorMovePoint);
            
            if (isSuccessfulAdded)
                return;

            Log.PrintError(log: $"Elevator Move Point <gb>{floor}</gb> is already added!");
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