using System;
using System.Collections.Generic;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Levels.Elevator;
using GameCore.Observers.Gameplay.LevelManager;

namespace GameCore.Gameplay.Levels
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

            _levelProviderObserver.OnRegisterElevatorEvent += OnRegisterElevator;
            _levelProviderObserver.OnRegisterSurfaceElevatorEvent += OnRegisterSurfaceElevator;
            _levelProviderObserver.OnRegisterStairsFireExitEvent += OnRegisterStairsFireExit;
            _levelProviderObserver.OnRegisterOtherFireExitEvent += OnRegisterOtherFireExit;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly ILevelProviderObserver _levelProviderObserver;
        private readonly Dictionary<Floor, ElevatorBase> _elevatorsReferences;
        private readonly Dictionary<Floor, FireExit> _stairsFireExits;
        private readonly Dictionary<Floor, FireExit> _otherFireExits;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Dispose()
        {
            _levelProviderObserver.OnRegisterElevatorEvent -= OnRegisterElevator;
            _levelProviderObserver.OnRegisterSurfaceElevatorEvent -= OnRegisterSurfaceElevator;
            _levelProviderObserver.OnRegisterStairsFireExitEvent -= OnRegisterStairsFireExit;
            _levelProviderObserver.OnRegisterOtherFireExitEvent -= OnRegisterOtherFireExit;
        }

        public void Clear()
        {
            _elevatorsReferences.Clear();
            _stairsFireExits.Clear();
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

            Log.PrintError(log: "Stairs Fire Exit is already added!");
        }
        
        private void OnRegisterOtherFireExit(Floor floor, FireExit fireExit)
        {
            bool isSuccessfulAdded = _otherFireExits.TryAdd(floor, fireExit);
            
            if (isSuccessfulAdded)
                return;

            Log.PrintError(log: "Other Fire Exit is already added!");
        }
    }
}