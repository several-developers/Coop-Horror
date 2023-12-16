using System;

namespace GameCore.Gameplay.Observers
{
    public interface IGraphyStateObserver
    {
        event Action<bool> OnStateChangedEvent;
        void SendChangeState(bool isEnabled);
        bool GetState();
    }
}