using System;
using System.Collections.Generic;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Dungeons;
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
            _dungeonReferences = new Dictionary<DungeonIndex, DungeonReferences>(capacity: 3);

            _dungeonsObserver.OnDungeonGenerationCompletedEvent += OnDungeonGenerationCompleted;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly IDungeonsObserver _dungeonsObserver;
        private readonly Dictionary<DungeonIndex, DungeonReferences> _dungeonReferences;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Dispose() =>
            _dungeonsObserver.OnDungeonGenerationCompletedEvent -= OnDungeonGenerationCompleted;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnDungeonGenerationCompleted(DungeonIndex dungeonIndex, DungeonReferences dungeonReferences)
        {
            if (_dungeonReferences.ContainsKey(dungeonIndex))
            {
                string errorLog = Log.HandleLog($"Dictionary <rb>already contains</rb> <gb>{dungeonIndex}</gb> key!");
                Debug.LogError(errorLog);
                return;
            }
            
            _dungeonReferences.Add(dungeonIndex, dungeonReferences);
        }
    }
}