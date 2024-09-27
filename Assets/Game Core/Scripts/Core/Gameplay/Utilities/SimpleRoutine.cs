using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameCore.Utilities;
using UnityEngine;

namespace GameCore.Gameplay.Utilities
{
#warning КРИВАЯ ХУЙНЯ, НЕ УМИРАЮТ АСИНКИ
    public class SimpleRoutine
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public SimpleRoutine(CancellationTokenSource cts) =>
            _cts = cts;

        public SimpleRoutine(CancellationTokenSource cts, float interval) : this(cts) =>
            GetTickIntervalEvent = () => interval;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnActionEvent = delegate { };
        public event Func<float> GetTickIntervalEvent = () => BasicInterval;

        private const float BasicInterval = 0.25f;
        
        private readonly CancellationTokenSource _cts;

        private bool _isActive;
        private bool _forceStop;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Start()
        {
            Stop();
            
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
            _isActive = false;
            _cts.Cancel();
        }

        public void FullStop()
        {
            Stop();
            _cts?.Dispose();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async void Cycle()
        {
            while (!_forceStop)
            {
                float interval = GetTickIntervalEvent.Invoke();
                int delay = interval.ConvertToMilliseconds();

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