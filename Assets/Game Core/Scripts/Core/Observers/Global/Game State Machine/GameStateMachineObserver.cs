using System;

namespace GameCore.Observers.Global.StateMachine
{
    public class GameStateMachineObserver : IGameStateMachineObserver
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action OnStateChangedEvent;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void SendStateChanged() =>
            OnStateChangedEvent?.Invoke();
    }
}