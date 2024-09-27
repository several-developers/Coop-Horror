using System;

namespace GameCore.Observers.Gameplay.Level
{
    public interface ILevelObserver
    {
        event Action OnLocationLoadedEvent;
        event Action OnLocationUnloadedEvent;
        void LocationLoaded(); // Only local, not synced between clients.
        void LocationUnloaded(); // Only local, not synced between clients.
    }
}