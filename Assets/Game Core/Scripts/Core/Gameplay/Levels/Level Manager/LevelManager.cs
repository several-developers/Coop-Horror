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
            _dungeonReferences = new Dictionary<ElevatorFloor, DungeonReferences>(capacity: 3);
            _elevatorsReferences = new Dictionary<ElevatorFloor, ElevatorBase>(capacity: 4);

            _dungeonsObserver.OnDungeonGenerationCompletedEvent += OnDungeonGenerationCompleted;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly IDungeonsObserver _dungeonsObserver;
        private readonly Dictionary<ElevatorFloor, DungeonReferences> _dungeonReferences;
        private readonly Dictionary<ElevatorFloor, ElevatorBase> _elevatorsReferences;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Dispose() =>
            _dungeonsObserver.OnDungeonGenerationCompletedEvent -= OnDungeonGenerationCompleted;
        
        public void AddSurfaceElevator(SurfaceElevator surfaceElevator) =>
            _elevatorsReferences.TryAdd(ElevatorFloor.Surface, surfaceElevator);

        // TEMP
        public void Clear()
        {
            _dungeonReferences.Clear();
            
            _elevatorsReferences.Remove(ElevatorFloor.One);
            _elevatorsReferences.Remove(ElevatorFloor.Two);
            _elevatorsReferences.Remove(ElevatorFloor.Three);
        }

        public bool TryGetElevator(ElevatorFloor elevatorFloor, out ElevatorBase elevator)
        {
            if (_elevatorsReferences.TryGetValue(elevatorFloor, out elevator))
                return true;

            string errorLog = Log.HandleLog($"Elevator <gb>'{elevatorFloor}'</gb> <rb>not found</rb>!");
            Debug.LogError(errorLog);
            
            return false;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnDungeonGenerationCompleted(ElevatorFloor elevatorFloor, DungeonReferences dungeonReferences)
        {
            if (_dungeonReferences.ContainsKey(elevatorFloor))
            {
                string errorLog = Log.HandleLog($"Dictionary <rb>already contains</rb> <gb>{elevatorFloor}</gb> key!");
                Debug.LogError(errorLog);
                return;
            }
            
            _dungeonReferences.Add(elevatorFloor, dungeonReferences);
            _elevatorsReferences.Add(elevatorFloor, dungeonReferences.GetElevator());
        }
    }
}