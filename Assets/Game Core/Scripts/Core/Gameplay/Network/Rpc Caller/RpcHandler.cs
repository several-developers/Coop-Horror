using GameCore.Enums.Gameplay;
using GameCore.Enums.Global;
using GameCore.Gameplay.Dungeons;
using GameCore.Gameplay.HorrorStateMachineSpace;
using GameCore.Observers.Gameplay.Rpc;
using Unity.Netcode;
using Zenject;

namespace GameCore.Gameplay.Network
{
    public class RpcHandler : NetworkBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IHorrorStateMachine horrorStateMachine, IRpcHandlerDecorator rpcHandlerDecorator,
            IRpcObserver rpcObserver)
        {
            _horrorStateMachine = horrorStateMachine;
            _rpcHandlerDecorator = rpcHandlerDecorator;
            _rpcObserver = rpcObserver;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private IHorrorStateMachine _horrorStateMachine;
        private IRpcHandlerDecorator _rpcHandlerDecorator;
        private IRpcObserver _rpcObserver;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void LoadLocationLogic(int sceneNameIndex)
        {
            if (!IsOwner)
                return;
            
            var sceneName = (SceneName)sceneNameIndex;
            _horrorStateMachine.ChangeState<LoadLocationState, SceneName>(sceneName);
        }

        // RPC: -----------------------------------------------------------------------------------

        [ServerRpc(RequireOwnership = false)]
        private void CreateItemPreviewServerRpc(int slotIndex, int itemID, ServerRpcParams serverRpcParams = default)
        {
            ulong clientID = serverRpcParams.Receive.SenderClientId;

            CreateItemPreviewClientRpc(clientID, slotIndex, itemID);
        }

        [ServerRpc(RequireOwnership = false)]
        private void DestroyItemPreviewServerRpc(int slotIndex) => DestroyItemPreviewClientRpc(slotIndex);

        [ServerRpc(RequireOwnership = false)]
        private void LoadLocationServerRpc(int sceneNameIndex) => LoadLocationClientRpc(sceneNameIndex);

        [ServerRpc(RequireOwnership = false)]
        private void StartLeavingLocationServerRpc() => StartLeavingLocationClientRpc();
        
        [ServerRpc(RequireOwnership = false)]
        private void LeftLocationServerRpc() => LeftLocationClientRpc();

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
        private void CreateItemPreviewClientRpc(ulong clientID, int slotIndex, int itemID)
        {
            CreateItemPreviewStaticData data = new(clientID, slotIndex, itemID);
            _rpcObserver.CreateItemPreview(data);
        }

        [ClientRpc]
        private void DestroyItemPreviewClientRpc(int slotIndex) =>
            _rpcObserver.DestroyItemPreview(slotIndex);

        [ClientRpc]
        private void LoadLocationClientRpc(int sceneNameIndex) => LoadLocationLogic(sceneNameIndex);

        [ClientRpc]
        private void StartLeavingLocationClientRpc() =>
            _rpcObserver.StartLeavingLocation();

        [ClientRpc]
        private void LeftLocationClientRpc() =>
            _rpcObserver.LocationLeft();

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

            _rpcHandlerDecorator.OnCreateItemPreviewInnerEvent += OnCreateItemPreview;
            _rpcHandlerDecorator.OnDestroyItemPreviewInnerEvent += DestroyItemPreviewServerRpc;
            _rpcHandlerDecorator.OnLoadLocationInnerEvent += OnLoadLocation;
            _rpcHandlerDecorator.OnStartLeavingLocationInnerEvent += StartLeavingLocationServerRpc;
            _rpcHandlerDecorator.OnLocationLeftInnerEvent += LeftLocationServerRpc;
            _rpcHandlerDecorator.OnGenerateDungeonsInnerEvent += GenerateDungeonsServerRpc;
            _rpcHandlerDecorator.OnStartElevatorInnerEvent += StartElevatorServerRpc;
            _rpcHandlerDecorator.OnOpenElevatorInnerEvent += OpenElevatorServerRpc;
            _rpcHandlerDecorator.OnTogglePlayerInsideMobileHQInnerEvent += TogglePlayerInsideMobileHQServerRpc;
            _rpcHandlerDecorator.OnTeleportToFireExitInnerEvent += OnTeleportToFireExit;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            
            _rpcHandlerDecorator.OnCreateItemPreviewInnerEvent -= OnCreateItemPreview;
            _rpcHandlerDecorator.OnDestroyItemPreviewInnerEvent -= DestroyItemPreviewServerRpc;
            _rpcHandlerDecorator.OnLoadLocationInnerEvent -= OnLoadLocation;
            _rpcHandlerDecorator.OnStartLeavingLocationInnerEvent -= StartLeavingLocationServerRpc;
            _rpcHandlerDecorator.OnLocationLeftInnerEvent -= LeftLocationServerRpc;
            _rpcHandlerDecorator.OnGenerateDungeonsInnerEvent -= GenerateDungeonsServerRpc;
            _rpcHandlerDecorator.OnStartElevatorInnerEvent -= StartElevatorServerRpc;
            _rpcHandlerDecorator.OnOpenElevatorInnerEvent -= OpenElevatorServerRpc;
            _rpcHandlerDecorator.OnTogglePlayerInsideMobileHQInnerEvent -= TogglePlayerInsideMobileHQServerRpc;
            _rpcHandlerDecorator.OnTeleportToFireExitInnerEvent -= OnTeleportToFireExit;
        }
        
        private void OnCreateItemPreview(int slotIndex, int itemID) => CreateItemPreviewServerRpc(slotIndex, itemID);

        private void OnLoadLocation(SceneName sceneName)
        {
            int sceneNameIndex = (int)sceneName;
            LoadLocationServerRpc(sceneNameIndex);
        }

        private void OnTeleportToFireExit(Floor floor, bool isInStairsLocation) =>
            TeleportToFireExitServerRpc(floor, isInStairsLocation);
    }
}