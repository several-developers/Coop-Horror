using System;

namespace GameCore.Observers.Gameplay.Level
{
    public class LevelObserver : ILevelObserver
    {
        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnLocationLoadedEvent = delegate { };
        public event Action OnLocationUnloadedEvent = delegate { };

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void LocationLoaded() =>
            OnLocationLoadedEvent.Invoke();

        public void LocationUnloaded() =>
            OnLocationUnloadedEvent.Invoke();
    }
}