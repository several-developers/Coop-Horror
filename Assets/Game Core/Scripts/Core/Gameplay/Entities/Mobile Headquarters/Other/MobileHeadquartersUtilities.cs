using System.Threading;
using Cinemachine;
using Cysharp.Threading.Tasks;
using GameCore.Gameplay.Locations;
using GameCore.Gameplay.Network;
using UnityEngine;

namespace GameCore.Gameplay.Entities.MobileHeadquarters
{
    public class MobileHeadquartersUtilities
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public MobileHeadquartersUtilities(IMobileHeadquartersEntity mobileHeadquartersEntity,
            CancellationTokenSource cancellationTokenSource)
        {
            _mobileHeadquartersEntity = mobileHeadquartersEntity;
            _cancellationTokenSource = cancellationTokenSource;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly IMobileHeadquartersEntity _mobileHeadquartersEntity;
        private readonly CancellationTokenSource _cancellationTokenSource;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public async UniTask MoveMobileHQToThePath(CinemachinePath path)
        {
            Transform mobileHQTransform = _mobileHeadquartersEntity.GetTransform();
            Vector3 mobileHQStartPosition = mobileHQTransform.position;

            _mobileHeadquartersEntity.ChangePath(path);

            bool isCanceled = await UniTask
                .DelayFrame(delayFrameCount: 4, cancellationToken: _cancellationTokenSource.Token)
                .SuppressCancellationThrow();

            if (isCanceled)
                return;
            
            Vector3 mobileHQNewPosition = mobileHQTransform.position;
            Vector3 offset = mobileHQNewPosition - mobileHQStartPosition;
            
            RpcCaller rpcCaller = RpcCaller.Get();
            rpcCaller.TeleportPlayersWithOffset(offset);
            
            Debug.Log($"Start Pos. ({mobileHQStartPosition}), new Pos. ({mobileHQNewPosition})");
        }
    }
}