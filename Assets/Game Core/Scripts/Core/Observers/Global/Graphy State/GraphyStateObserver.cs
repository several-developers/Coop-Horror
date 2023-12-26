using System;

namespace GameCore.Observers.Global.Graphy
{
    public class GraphyStateObserver : IGraphyStateObserver
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action<bool> OnStateChangedEvent;

        private bool _isEnabled;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void SendChangeState(bool isEnabled)
        {
            _isEnabled = isEnabled;
            OnStateChangedEvent?.Invoke(isEnabled);
        }

        public bool GetState() => _isEnabled;
    }
}