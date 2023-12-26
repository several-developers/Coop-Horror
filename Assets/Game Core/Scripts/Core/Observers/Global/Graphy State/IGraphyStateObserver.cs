using System;

namespace GameCore.Observers.Global.Graphy
{
    public interface IGraphyStateObserver
    {
        event Action<bool> OnStateChangedEvent;
        void SendChangeState(bool isEnabled);
        bool GetState();
    }
}