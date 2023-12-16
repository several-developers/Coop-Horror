using System;

namespace GameCore.Gameplay.Observers
{
    public interface IGameStateMachineObserver
    {
        event Action OnStateChangedEvent;
        void SendStateChanged();
    }
}