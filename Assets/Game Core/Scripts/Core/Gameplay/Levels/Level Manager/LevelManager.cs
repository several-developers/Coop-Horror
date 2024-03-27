using System;
using System.Collections.Generic;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Levels.Elevator;
using GameCore.Observers.Gameplay.Dungeons;

namespace GameCore.Gameplay.Levels
{
    public class LevelManager : ILevelManager, IDisposable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public LevelManager(IDungeonsObserver dungeonsObserver)
        {
            _dungeonsObserver = dungeonsObserver;
            _elevatorsReferences = new Dictionary<Floor, ElevatorBase>(capacity: 4);
            _stairsFireExits = new Dictionary<Floor, FireExit>(capacity: 4);
            _otherFireExits = new Dictionary<Floor, FireExit>(capacity: 4);

            _dungeonsObserver.OnRegisterElevatorEvent += OnRegisterElevator;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly IDungeonsObserver _dungeonsObserver;
        private readonly Dictionary<Floor, ElevatorBase> _elevatorsReferences;
        private readonly Dictionary<Floor, FireExit> _stairsFireExits;
        private readonly Dictionary<Floor, FireExit> _otherFireExits;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Dispose() =>
            _dungeonsObserver.OnRegisterElevatorEvent -= OnRegisterElevator;

        public void AddSurfaceElevator(SurfaceElevator surfaceElevator) =>
            _elevatorsReferences.TryAdd(Floor.Surface, surfaceElevator);

        public void AddStairsFireExit(Floor floor, FireExit fireExit) =>
            _stairsFireExits.Add(floor, fireExit);

        public void AddOtherFireExit(Floor floor, FireExit fireExit) =>
            _otherFireExits.Add(floor, fireExit);

        // TEMP
        public void Clear()
        {
            _elevatorsReferences.Remove(Floor.One);
            _elevatorsReferences.Remove(Floor.Two);
            _elevatorsReferences.Remove(Floor.Three);
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

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnRegisterElevator(ElevatorBase elevatorBase)
        {
            Floor floor = elevatorBase.GetElevatorFloor();
            bool isSuccessfulAdded = _elevatorsReferences.TryAdd(floor, elevatorBase);

            if (isSuccessfulAdded)
                return;

            Log.PrintError(log: $"Elevator with floor <gb>{floor}</gb> is already added!");
        }
    }
}