using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameCore.Enums.Global;
using GameCore.Gameplay.Levels.Locations;
using GameCore.Gameplay.Network;
using GameCore.Infrastructure.Services.Global;
using UnityEngine;

namespace GameCore.Gameplay.HorrorStateMachineSpace
{
    public class LoadLocationState : IEnterState<SceneName>, IExitState, IDisposable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public LoadLocationState(IHorrorStateMachine horrorStateMachine, ILocationsLoader locationsLoader,
            IScenesLoaderService scenesLoaderService)
        {
            _horrorStateMachine = horrorStateMachine;
            _locationsLoader = locationsLoader;
            _scenesLoaderService = scenesLoaderService;
            _cancellationTokenSource = new CancellationTokenSource();

            horrorStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IHorrorStateMachine _horrorStateMachine;
        private readonly ILocationsLoader _locationsLoader;
        private readonly IScenesLoaderService _scenesLoaderService;
        private readonly CancellationTokenSource _cancellationTokenSource;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Dispose() =>
            _cancellationTokenSource?.Dispose();

        public void Enter(SceneName sceneName)
        {
            _scenesLoaderService.OnSceneLoadedEvent += OnSceneLoaded;

            LoadLocation(sceneName);
        }

        public void Exit() =>
            _scenesLoaderService.OnSceneLoadedEvent -= OnSceneLoaded;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void LoadLocation(SceneName sceneName) =>
            _locationsLoader.LoadLocationNetwork(sceneName);

#warning CHANGE TO OBSERVER
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

                Debug.Log("Find Location Manager iterations amount: " + iterations);

                RpcCaller rpcCaller = RpcCaller.Get();
                rpcCaller.SendLocationLoaded();

                EnterGenerateDungeonsState();
                break;
            }
        }

        private void EnterGenerateDungeonsState() =>
            _horrorStateMachine.ChangeState<GenerateDungeonsState>();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnSceneLoaded() => FindLocationManager();
    }
}