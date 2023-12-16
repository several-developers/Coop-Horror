using System;

namespace GameCore.Gameplay.Observers.Taps
{
    public class TapsObserver : ITapsObserver
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action OnTapDownEvent;
        public event Action OnTapUpEvent;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void SendTapDownEvent() =>
            OnTapDownEvent?.Invoke();

        public void SendTapUpEvent() =>
            OnTapUpEvent?.Invoke();
    }
}