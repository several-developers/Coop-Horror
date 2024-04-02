using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Network;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Levels
{
    public class FireExitsManager : IInitializable, IDisposable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public FireExitsManager(ILevelProvider levelProvider) =>
            _levelProvider = levelProvider;
        
        // FIELDS: --------------------------------------------------------------------------------

        private readonly ILevelProvider _levelProvider;
        
        private RpcCaller _rpcCaller;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Initialize()
        {
            _rpcCaller = RpcCaller.Get();
            
            _rpcCaller.OnTeleportToFireExitEvent += OnTeleportToFireExit;
        }
        
        public void Dispose() =>
            _rpcCaller.OnTeleportToFireExitEvent -= OnTeleportToFireExit;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void TeleportToFireExit(ulong clientID, Floor floor, bool isInStairsLocation)
        {
            PlayerEntity playerEntity = PlayerEntity.GetLocalPlayer();
            bool isPlayerMatches = playerEntity.OwnerClientId == clientID;

            if (!isPlayerMatches)
                return;
            
            FireExit fireExit;

            // Reversed
            bool isFireExitFound = isInStairsLocation
                ? _levelProvider.TryGetOtherFireExit(floor, out fireExit)
                : _levelProvider.TryGetStairsFireExit(floor, out fireExit);

            if (!isFireExitFound)
                return;
            
            Transform teleportPoint = fireExit.GetTeleportPoint();
            Vector3 position = teleportPoint.position;
            Quaternion rotation = teleportPoint.rotation;
            
            playerEntity.TeleportPlayer(position, rotation);
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnTeleportToFireExit(ulong clientID, Floor floor, bool isInStairsLocation) =>
            TeleportToFireExit(clientID, floor, isInStairsLocation);
    }
}