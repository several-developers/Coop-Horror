using System;

namespace GameCore.Observers.Gameplay.Level
{
    public interface ILevelObserver
    {
        event Action OnLocationLoadedEvent;
        event Action OnLocationUnloadedEvent;
        void LocationLoaded();
        void LocationUnloaded();
    }
}