﻿using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Dungeons;
using GameCore.Observers.Gameplay.Rpc;
using Unity.Netcode;
using Zenject;

namespace GameCore.Gameplay.Network
{
    public class RpcHandler : NetworkBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IRpcHandlerDecorator rpcHandlerDecorator, IRpcObserver rpcObserver)
        {
            _rpcHandlerDecorator = rpcHandlerDecorator;
            _rpcObserver = rpcObserver;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private IRpcHandlerDecorator _rpcHandlerDecorator;
        private IRpcObserver _rpcObserver;

        // RPC: -----------------------------------------------------------------------------------

        [ServerRpc(RequireOwnership = false)]
        private void GenerateDungeonsServerRpc(DungeonsSeedData data) => GenerateDungeonsClientRpc(data);

        [ServerRpc(RequireOwnership = false)]
        private void StartElevatorServerRpc(Floor floor) => StartElevatorClientRpc(floor);

        [ServerRpc(RequireOwnership = false)]
        private void OpenElevatorServerRpc(Floor floor) => OpenElevatorClientRpc(floor);

        [ClientRpc]
        private void GenerateDungeonsClientRpc(DungeonsSeedData data) =>
            _rpcObserver.GenerateDungeons(data);

        [ClientRpc]
        private void StartElevatorClientRpc(Floor floor) =>
            _rpcObserver.StartElevator(floor);

        [ClientRpc]
        private void OpenElevatorClientRpc(Floor floor) =>
            _rpcObserver.OpenElevator(floor);

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            _rpcHandlerDecorator.OnGenerateDungeonsInnerEvent += GenerateDungeonsServerRpc;
            _rpcHandlerDecorator.OnStartElevatorInnerEvent += StartElevatorServerRpc;
            _rpcHandlerDecorator.OnOpenElevatorInnerEvent += OpenElevatorServerRpc;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            
            _rpcHandlerDecorator.OnGenerateDungeonsInnerEvent -= GenerateDungeonsServerRpc;
            _rpcHandlerDecorator.OnStartElevatorInnerEvent -= StartElevatorServerRpc;
            _rpcHandlerDecorator.OnOpenElevatorInnerEvent -= OpenElevatorServerRpc;
        }
    }
}