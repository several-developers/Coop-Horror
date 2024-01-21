using System;
using System.Threading;
using Cinemachine;
using Cysharp.Threading.Tasks;
using GameCore.Enums;
using GameCore.Gameplay.Entities.MobileHeadquarters;
using GameCore.Gameplay.Locations;
using GameCore.Gameplay.Network;
using UnityEngine;

namespace GameCore.Gameplay.HorrorStateMachineSpace
{
    public class LoadLocationState : IEnterState<SceneName>, IDisposable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public LoadLocationState(IHorrorStateMachine horrorStateMachine, ILocationsLoader locationsLoader,
            IMobileHeadquartersEntity mobileHeadquartersEntity)
        {
            _horrorStateMachine = horrorStateMachine;
            _locationsLoader = locationsLoader;
            _cancellationTokenSource = new CancellationTokenSource();
            
            _mobileHeadquartersUtilities = new MobileHeadquartersUtilities(mobileHeadquartersEntity,
                _cancellationTokenSource);

            horrorStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IHorrorStateMachine _horrorStateMachine;
        private readonly ILocationsLoader _locationsLoader;
        private readonly MobileHeadquartersUtilities _mobileHeadquartersUtilities;
        private readonly CancellationTokenSource _cancellationTokenSource;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Dispose() =>
            _cancellationTokenSource?.Dispose();

        public void Enter(SceneName sceneName) => LoadLocation(sceneName);

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void LoadLocation(SceneName sceneName) =>
            _locationsLoader.LoadLocationNetwork(sceneName, OnLocationLoaded);

        private async void FindLocationManager()
        {
            int iterations = 0;

            while (true)
            {
                bool isCanceled = await UniTask
                    .DelayFrame(delayFrameCount: 1, cancellationToken: _cancellationTokenSource.Token)
                    .SuppressCancellationThrow();

                if (isCanceled)
                    return;

                LocationManager locationManager = LocationManager.Get();
                bool isExists = locationManager != null;

                iterations++;

                if (iterations > 1000)
                {
                    Debug.LogError("Infinity Loop!");
                    break;
                }

                if (!isExists)
                    continue;

                Debug.Log("Iterations amount: " + iterations);

                await MoveMobileHQToTheRoad(locationManager);
                EnterGameLoopState();
                break;
            }
        }

        private async UniTask MoveMobileHQToTheRoad(LocationManager locationManager)
        {
            CinemachinePath path = locationManager.GetPath();
            await _mobileHeadquartersUtilities.MoveMobileHQToThePath(path);
        }

        private void EnterGameLoopState() =>
            _horrorStateMachine.ChangeState<GameLoopState>();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnLocationLoaded() => FindLocationManager();
    }
}