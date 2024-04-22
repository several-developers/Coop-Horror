using System;

namespace GameCore.Observers.Gameplay.Level
{
    public class LevelObserver : ILevelObserver
    {
        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnLocationLoadedEvent = delegate { };
        public event Action OnLocationLeftEvent = delegate { };

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void LocationLoaded() =>
            OnLocationLoadedEvent.Invoke();

        public void LocationLeft() =>
            OnLocationLeftEvent.Invoke();
    }
}