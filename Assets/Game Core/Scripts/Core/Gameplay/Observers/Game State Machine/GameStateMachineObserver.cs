using System;

namespace GameCore.Gameplay.Observers
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