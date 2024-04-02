using System;

namespace GameCore.Observers.Gameplay.Level
{
    public interface ILevelObserver
    {
        event Action OnLocationLoadedEvent;
        void LocationLoaded();
    }
}