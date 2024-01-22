using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameCore.Gameplay.Entities.MobileHeadquarters;
using GameCore.Gameplay.Network;
using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.HorrorStateMachineSpace
{
    public class PrepareGameState : IEnterStateAsync, IDisposable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public PrepareGameState(IHorrorStateMachine horrorStateMachine,
            IMobileHeadquartersEntity mobileHeadquartersEntity)
        {
            _horrorStateMachine = horrorStateMachine;
            _mobileHeadquartersEntity = mobileHeadquartersEntity;
            _cancellationTokenSource = new CancellationTokenSource();

            horrorStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IHorrorStateMachine _horrorStateMachine;
        private readonly IMobileHeadquartersEntity _mobileHeadquartersEntity;
        private readonly CancellationTokenSource _cancellationTokenSource;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public async UniTaskVoid Enter()
        {
            int iterations = 0;

            while (iterations < 1000)
            {
                iterations++;
                
                bool isCanceled = await UniTask
                    .DelayFrame(1)
                    .SuppressCancellationThrow();

                if (isCanceled)
                    return;

                NetworkObject networkObject = _mobileHeadquartersEntity.GetNetworkObject();
                bool isSpawned = networkObject.IsSpawned;

                if (!isSpawned)
                    continue;
                
                //RpcCaller rpcCaller = RpcCaller.Get();
                //rpcCaller.SendRoadLocationLoaded();
                
                _mobileHeadquartersEntity.ArriveAtRoadLocation();
                
                TheNetworkHorror networkHorror = TheNetworkHorror.Get(); // TEMP
                networkHorror.SetRoadLocationLoaded(); // TEMP

                break;
            }
            
            Debug.Log($"Prepared with {iterations} iterations.");

            EnterGameLoopState();
        }

        public void Dispose() =>
            _cancellationTokenSource?.Dispose();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void EnterGameLoopState() =>
            _horrorStateMachine.ChangeState<GameLoopState>();
    }
}