using GameCore.Enums.Gameplay;
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
        private void CreateItemPreviewServerRpc(int slotIndex, int itemID) =>
            CreateItemPreviewClientRpc(slotIndex, itemID);

        [ServerRpc(RequireOwnership = false)]
        private void DestroyItemPreviewServerRpc(int slotIndex) => DestroyItemPreviewClientRpc(slotIndex);

        [ServerRpc(RequireOwnership = false)]
        private void GenerateDungeonsServerRpc(DungeonsSeedData data) => GenerateDungeonsClientRpc(data);

        [ServerRpc(RequireOwnership = false)]
        private void StartElevatorServerRpc(Floor floor) => StartElevatorClientRpc(floor);

        [ServerRpc(RequireOwnership = false)]
        private void OpenElevatorServerRpc(Floor floor) => OpenElevatorClientRpc(floor);

        [ServerRpc(RequireOwnership = false)]
        private void TogglePlayerInsideMobileHQServerRpc(ulong clientID, bool isInside) =>
            TogglePlayerInsideMobileHQClientRpc(clientID, isInside);

        [ServerRpc(RequireOwnership = false)]
        private void TeleportToFireExitServerRpc(Floor floor, bool isInStairsLocation,
            ServerRpcParams serverRpcParams = default)
        {
            ulong clientID = serverRpcParams.Receive.SenderClientId;
            TeleportToFireExitClientRpc(clientID, floor, isInStairsLocation);
        }

        [ClientRpc]
        private void CreateItemPreviewClientRpc(int slotIndex, int itemID)
        {
            CreateItemPreviewStaticData data = new(slotIndex, itemID);
            _rpcObserver.CreateItemPreview(data);
        }

        [ClientRpc]
        private void DestroyItemPreviewClientRpc(int slotIndex) =>
            _rpcObserver.DestroyItemPreview(slotIndex);
        
        [ClientRpc]
        private void GenerateDungeonsClientRpc(DungeonsSeedData data) =>
            _rpcObserver.GenerateDungeons(data);

        [ClientRpc]
        private void StartElevatorClientRpc(Floor floor) =>
            _rpcObserver.StartElevator(floor);

        [ClientRpc]
        private void OpenElevatorClientRpc(Floor floor) =>
            _rpcObserver.OpenElevator(floor);

        [ClientRpc]
        private void TogglePlayerInsideMobileHQClientRpc(ulong clientID, bool isInside) =>
            _rpcObserver.TogglePlayerInsideMobileHQ(clientID, isInside);

        [ClientRpc]
        private void TeleportToFireExitClientRpc(ulong clientID, Floor floor, bool isInStairsLocation) =>
            _rpcObserver.TeleportToFireExit(clientID, floor, isInStairsLocation);

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            _rpcHandlerDecorator.OnCreateItemPreviewInnerEvent += CreateItemPreviewServerRpc;
            _rpcHandlerDecorator.OnDestroyItemPreviewInnerEvent += DestroyItemPreviewServerRpc;
            _rpcHandlerDecorator.OnGenerateDungeonsInnerEvent += GenerateDungeonsServerRpc;
            _rpcHandlerDecorator.OnStartElevatorInnerEvent += StartElevatorServerRpc;
            _rpcHandlerDecorator.OnOpenElevatorInnerEvent += OpenElevatorServerRpc;
            _rpcHandlerDecorator.OnTogglePlayerInsideMobileHQInnerEvent += TogglePlayerInsideMobileHQServerRpc;
            _rpcHandlerDecorator.OnTeleportToFireExitInnerEvent += OnTeleportToFireExit;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            
            _rpcHandlerDecorator.OnCreateItemPreviewInnerEvent -= CreateItemPreviewServerRpc;
            _rpcHandlerDecorator.OnDestroyItemPreviewInnerEvent -= DestroyItemPreviewServerRpc;
            _rpcHandlerDecorator.OnGenerateDungeonsInnerEvent -= GenerateDungeonsServerRpc;
            _rpcHandlerDecorator.OnStartElevatorInnerEvent -= StartElevatorServerRpc;
            _rpcHandlerDecorator.OnOpenElevatorInnerEvent -= OpenElevatorServerRpc;
            _rpcHandlerDecorator.OnTogglePlayerInsideMobileHQInnerEvent -= TogglePlayerInsideMobileHQServerRpc;
            _rpcHandlerDecorator.OnTeleportToFireExitInnerEvent -= OnTeleportToFireExit;
        }
        
        private void OnTeleportToFireExit(Floor floor, bool isInStairsLocation) =>
            TeleportToFireExitServerRpc(floor, isInStairsLocation);
    }
}