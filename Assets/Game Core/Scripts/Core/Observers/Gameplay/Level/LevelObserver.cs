using System;

namespace GameCore.Observers.Gameplay.Level
{
    public class LevelObserver : ILevelObserver
    {
        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnLocationLoadedEvent;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void LocationLoaded() =>
            OnLocationLoadedEvent?.Invoke();
    }
}