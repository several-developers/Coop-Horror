using System;

namespace GameCore.Observers.Gameplay.Level
{
    public interface ILevelObserver
    {
        event Action OnLocationLoadedEvent;
        event Action OnLocationLeftEvent;
        void LocationLoaded();
        void LocationLeft();
    }
}