using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameCore.Gameplay.Dungeons;
using GameCore.Gameplay.Levels;
using GameCore.Gameplay.Levels.Locations;
using GameCore.Gameplay.Network;
using Unity.VisualScripting;

namespace GameCore.Gameplay.HorrorStateMachineSpace
{
    public class LeaveLocationState : IEnterStateAsync, IDisposable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public LeaveLocationState(IHorrorStateMachine horrorStateMachine, ILocationsLoader locationsLoader,
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

        public async UniTaskVoid Enter()
        {
            RpcCaller rpcCaller = RpcCaller.Get();
            rpcCaller.SendLeftLocation();

            // TEMP
            bool isCanceled = await UniTask
                .Delay(millisecondsDelay: 500, cancellationToken: _cancellationTokenSource.Token)
                .SuppressCancellationThrow();

            if (isCanceled)
                return;
            
            UnloadLastLocation();
            ClearDungeons();
            _levelProvider.Clear();
            EnterGameLoopState();
        }
        
        public void Dispose() =>
            _cancellationTokenSource?.Dispose();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void UnloadLastLocation() =>
            _locationsLoader.UnloadLastLocation();

        private static void ClearDungeons()
        {
            DungeonsManager dungeonsManager = DungeonsManager.Get();
            dungeonsManager.ClearDungeons();
        }
        
        private void EnterGameLoopState() =>
            _horrorStateMachine.ChangeState<GameLoopState>();
    }
}