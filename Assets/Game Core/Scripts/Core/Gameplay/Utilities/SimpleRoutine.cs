using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameCore.Utilities;
using UnityEngine;

namespace GameCore.Gameplay.Utilities
{
    public class SimpleRoutine
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public SimpleRoutine(CancellationTokenSource cts) =>
            _cts = cts;

        public SimpleRoutine(CancellationTokenSource cts, float interval) : this(cts) =>
            _interval = interval;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnActionEvent = delegate { };
        
        private readonly CancellationTokenSource _cts;
        private readonly float _interval = 0.25f;

        private bool _isActive;
        private bool _forceStop;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Start()
        {
            if (_isActive)
            {
                Debug.LogError(message: "Routine is active!");
                return;
            }
            
            _isActive = true;
            _forceStop = false;
            
            Cycle();
        }

        public void Stop()
        {
            _forceStop = true;
            _cts.Cancel();
        }
        
        public void FullStop()
        {
            _forceStop = true;
            _cts.Cancel();
            _cts?.Dispose();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async void Cycle()
        {
            while (!_forceStop)
            {
                int delay = _interval.ConvertToMilliseconds();
                
                bool isCanceled = await UniTask
                    .Delay(delay, cancellationToken: _cts.Token)
                    .SuppressCancellationThrow();

                if (isCanceled)
                    break;

                if (_forceStop)
                    break;
                
                OnActionEvent.Invoke();
            }

            _isActive = false;
        }
    }
}