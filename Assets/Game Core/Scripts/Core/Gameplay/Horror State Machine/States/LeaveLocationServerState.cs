using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameCore.Gameplay.Levels.Locations;

namespace GameCore.Gameplay.HorrorStateMachineSpace
{
    public class LeaveLocationServerState : IEnterStateAsync, IDisposable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public LeaveLocationServerState(IHorrorStateMachine horrorStateMachine, ILocationsLoader locationsLoader)
        {
            _horrorStateMachine = horrorStateMachine;
            _locationsLoader = locationsLoader;
            _cancellationTokenSource = new CancellationTokenSource();
            
            horrorStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly IHorrorStateMachine _horrorStateMachine;
        private readonly ILocationsLoader _locationsLoader;
        private readonly CancellationTokenSource _cancellationTokenSource;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Dispose() =>
            _cancellationTokenSource?.Dispose();

        public async UniTaskVoid Enter()
        {
            // TEMP
            bool isCanceled = await UniTask
                .Delay(millisecondsDelay: 500, cancellationToken: _cancellationTokenSource.Token)
                .SuppressCancellationThrow();

            if (isCanceled)
                return;
            
            UnloadLastLocation();
            EnterLeaveLocationClientState();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void UnloadLastLocation() =>
            _locationsLoader.UnloadLastLocation();

        private void EnterLeaveLocationClientState() =>
            _horrorStateMachine.ChangeState<LeaveLocationClientState>();
    }
}