using System;
using System.Collections.Generic;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Dungeons;
using GameCore.Gameplay.Levels.Elevator;
using GameCore.Observers.Gameplay.Dungeons;
using UnityEngine;

namespace GameCore.Gameplay.Levels
{
    public class LevelManager : ILevelManager, IDisposable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public LevelManager(IDungeonsObserver dungeonsObserver)
        {
            _dungeonsObserver = dungeonsObserver;
            _dungeonReferences = new Dictionary<Floor, DungeonReferences>(capacity: 3);
            _elevatorsReferences = new Dictionary<Floor, ElevatorBase>(capacity: 4);
            _stairsFireExits = new Dictionary<Floor, FireExit>(capacity: 4);
            _otherFireExits = new Dictionary<Floor, FireExit>(capacity: 4);

            _dungeonsObserver.OnDungeonGenerationCompletedEvent += OnDungeonGenerationCompleted;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly IDungeonsObserver _dungeonsObserver;
        private readonly Dictionary<Floor, DungeonReferences> _dungeonReferences;
        private readonly Dictionary<Floor, ElevatorBase> _elevatorsReferences;
        private readonly Dictionary<Floor, FireExit> _stairsFireExits;
        private readonly Dictionary<Floor, FireExit> _otherFireExits;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Dispose() =>
            _dungeonsObserver.OnDungeonGenerationCompletedEvent -= OnDungeonGenerationCompleted;
        
        public void AddSurfaceElevator(SurfaceElevator surfaceElevator) =>
            _elevatorsReferences.TryAdd(Floor.Surface, surfaceElevator);

        public void AddStairsFireExit(Floor floor, FireExit fireExit) =>
            _stairsFireExits.Add(floor, fireExit);

        public void AddOtherFireExit(Floor floor, FireExit fireExit) =>
            _otherFireExits.Add(floor, fireExit);

        // TEMP
        public void Clear()
        {
            _dungeonReferences.Clear();
            
            _elevatorsReferences.Remove(Floor.One);
            _elevatorsReferences.Remove(Floor.Two);
            _elevatorsReferences.Remove(Floor.Three);
        }

        public bool TryGetElevator(Floor floor, out ElevatorBase elevator)
        {
            if (_elevatorsReferences.TryGetValue(floor, out elevator))
                return true;

            string errorLog = Log.HandleLog($"Elevator <gb>'{floor}'</gb> <rb>not found</rb>!");
            Debug.LogError(errorLog);
            
            return false;
        }

        public bool TryGetStairsFireExit(Floor floor, out FireExit fireExit)
        {
            if (_stairsFireExits.TryGetValue(floor, out fireExit))
                return true;

            string errorLog = Log.HandleLog($"Stairs Fire Exit <gb>'{floor}'</gb> <rb>not found</rb>!");
            Debug.LogError(errorLog);
            
            return false;
        }

        public bool TryGetOtherFireExit(Floor floor, out FireExit fireExit)
        {
            if (_otherFireExits.TryGetValue(floor, out fireExit))
                return true;

            string errorLog = Log.HandleLog($"Other Fire Exit <gb>'{floor}'</gb> <rb>not found</rb>!");
            Debug.LogError(errorLog);
            
            return false;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnDungeonGenerationCompleted(Floor floor, DungeonReferences dungeonReferences)
        {
            if (_dungeonReferences.ContainsKey(floor))
            {
                string errorLog = Log.HandleLog($"Dictionary <rb>already contains</rb> <gb>{floor}</gb> key!");
                Debug.LogError(errorLog);
                return;
            }
            
            _dungeonReferences.Add(floor, dungeonReferences);
            _elevatorsReferences.Add(floor, dungeonReferences.GetElevator());
        }
    }
}