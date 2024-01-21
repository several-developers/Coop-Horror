using System;
using System.Threading;
using Cinemachine;
using Cysharp.Threading.Tasks;
using GameCore.Gameplay.Entities.MobileHeadquarters;
using GameCore.Gameplay.Locations;

namespace GameCore.Gameplay.HorrorStateMachineSpace
{
    public class LeaveLocationState : IEnterStateAsync, IDisposable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public LeaveLocationState(IHorrorStateMachine horrorStateMachine, ILocationsLoader locationsLoader,
            IRoadLocationManager roadLocationManager, IMobileHeadquartersEntity mobileHeadquartersEntity)
        {
            _horrorStateMachine = horrorStateMachine;
            _locationsLoader = locationsLoader;
            _roadLocationManager = roadLocationManager;
            _cancellationTokenSource = new CancellationTokenSource();

            _mobileHeadquartersUtilities = new MobileHeadquartersUtilities(mobileHeadquartersEntity,
                _cancellationTokenSource);

            horrorStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly IHorrorStateMachine _horrorStateMachine;
        private readonly ILocationsLoader _locationsLoader;
        private readonly IRoadLocationManager _roadLocationManager;
        private readonly MobileHeadquartersUtilities _mobileHeadquartersUtilities;
        private readonly CancellationTokenSource _cancellationTokenSource;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Dispose() =>
            _cancellationTokenSource?.Dispose();
        
        public async UniTaskVoid Enter()
        {
            await MoveMobileHQToTheRoad();
            UnloadLastLocation();
            EnterGameLoopState();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void UnloadLastLocation() =>
            _locationsLoader.UnloadLastLocationNetwork();

        private async UniTask MoveMobileHQToTheRoad()
        {
            CinemachinePath path = _roadLocationManager.GetPath();
            await _mobileHeadquartersUtilities.MoveMobileHQToThePath(path);
        }

        private void EnterGameLoopState() =>
            _horrorStateMachine.ChangeState<GameLoopState>();
    }
}