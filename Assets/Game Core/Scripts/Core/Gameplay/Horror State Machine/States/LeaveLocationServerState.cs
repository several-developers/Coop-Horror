using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Levels;
using GameCore.Gameplay.Levels.Elevator;
using GameCore.Gameplay.Levels.Locations;

namespace GameCore.Gameplay.HorrorStateMachineSpace
{
    public class LeaveLocationServerState : IEnterStateAsync, IDisposable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public LeaveLocationServerState(IHorrorStateMachine horrorStateMachine, ILocationsLoader locationsLoader,
            ILevelProvider levelProvider)
        {
            _horrorStateMachine = horrorStateMachine;
            _locationsLoader = locationsLoader;
            _levelProvider = levelProvider;
            _cancellationTokenSource = new CancellationTokenSource();
            
            horrorStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly IHorrorStateMachine _horrorStateMachine;
        private readonly ILocationsLoader _locationsLoader;
        private readonly ILevelProvider _levelProvider;
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
            
            ClearDungeonElevators();
            UnloadLastLocation();
            EnterLeaveLocationClientState();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ClearDungeonElevators()
        {
            List<Floor> floors = new() { Floor.One, Floor.Two, Floor.Three };
            
            foreach (Floor floor in floors)
            {
                bool isElevatorFound = TryGetElevator(floor, out ElevatorBase elevatorBase);
                
                if (!isElevatorFound)
                    continue;
                
                UnityEngine.Object.Destroy(elevatorBase.gameObject);
            }

            // LOCAL METHODS: -----------------------------

            bool TryGetElevator(Floor floor, out ElevatorBase result) =>
                _levelProvider.TryGetElevator(floor, out result);
        }

        private void UnloadLastLocation() =>
            _locationsLoader.UnloadLastLocation();

        private void EnterLeaveLocationClientState() =>
            _horrorStateMachine.ChangeState<LeaveLocationClientState>();
    }
}