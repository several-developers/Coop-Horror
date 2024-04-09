using System;

namespace GameCore.Observers.Gameplay.Level
{
    public class LevelObserver : ILevelObserver
    {
        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnLocationLoadedEvent = delegate { };

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void LocationLoaded() =>
            OnLocationLoadedEvent.Invoke();
    }
}