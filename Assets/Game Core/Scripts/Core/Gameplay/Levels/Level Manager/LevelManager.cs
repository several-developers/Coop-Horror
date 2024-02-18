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

            _dungeonsObserver.OnDungeonGenerationCompletedEvent += OnDungeonGenerationCompleted;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly IDungeonsObserver _dungeonsObserver;
        private readonly Dictionary<Floor, DungeonReferences> _dungeonReferences;
        private readonly Dictionary<Floor, ElevatorBase> _elevatorsReferences;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Dispose() =>
            _dungeonsObserver.OnDungeonGenerationCompletedEvent -= OnDungeonGenerationCompleted;
        
        public void AddSurfaceElevator(SurfaceElevator surfaceElevator) =>
            _elevatorsReferences.TryAdd(Floor.Surface, surfaceElevator);

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