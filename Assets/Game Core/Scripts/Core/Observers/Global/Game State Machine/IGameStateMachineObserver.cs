using System;

namespace GameCore.Observers.Global.StateMachine
{
    public interface IGameStateMachineObserver
    {
        event Action OnStateChangedEvent;
        void SendStateChanged();
    }
}