using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Player;
using GameCore.Observers.Gameplay.Rpc;
using UnityEngine;

namespace GameCore.Gameplay.Levels
{
    public class FireExitsManager : IDisposable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public FireExitsManager(ILevelProvider levelProvider, IRpcObserver rpcObserver)
        {
            _levelProvider = levelProvider;
            _rpcObserver = rpcObserver;
            
            _rpcObserver.OnTeleportToFireExitEvent += OnTeleportToFireExit;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly ILevelProvider _levelProvider;
        private readonly IRpcObserver _rpcObserver;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void Dispose() =>
            _rpcObserver.OnTeleportToFireExitEvent -= OnTeleportToFireExit;

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

            playerEntity.References.Rigidbody.velocity = Vector3.zero;
            playerEntity.TeleportPlayer(position, rotation);
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnTeleportToFireExit(ulong clientID, Floor floor, bool isInStairsLocation) =>
            TeleportToFireExit(clientID, floor, isInStairsLocation);
    }
}