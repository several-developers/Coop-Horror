using GameCore.Enums.Gameplay;
using GameCore.Enums.Global;
using GameCore.Gameplay.Dungeons;
using GameCore.Gameplay.HorrorStateMachineSpace;
using GameCore.Observers.Gameplay.Rpc;
using Unity.Netcode;
using UnityEngine;
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

            _rpcHandlerDecorator.OnCreateItemPreviewEvent += OnCreateItemPreview;
            _rpcHandlerDecorator.OnDestroyItemPreviewEvent += OnDestroyItemPreview;
            _rpcHandlerDecorator.OnLoadLocationEvent += OnLoadLocation;
            _rpcHandlerDecorator.OnStartLeavingLocationEvent += OnStartLeavingLocation;
            _rpcHandlerDecorator.OnLocationLeftEvent += LocationLeft;
            _rpcHandlerDecorator.OnGenerateDungeonsEvent += OnGenerateDungeons;
            _rpcHandlerDecorator.OnStartElevatorEvent += OnStartElevator;
            _rpcHandlerDecorator.OnOpenElevatorEvent += OnOpenElevator;
            _rpcHandlerDecorator.OnTogglePlayerInsideMobileHQEvent += OnTogglePlayerInsideMobileHQ;
            _rpcHandlerDecorator.OnTeleportToFireExitEvent += OnTeleportToFireExit;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            
            _rpcHandlerDecorator.OnCreateItemPreviewEvent -= OnCreateItemPreview;
            _rpcHandlerDecorator.OnDestroyItemPreviewEvent -= OnDestroyItemPreview;
            _rpcHandlerDecorator.OnLoadLocationEvent -= OnLoadLocation;
            _rpcHandlerDecorator.OnStartLeavingLocationEvent -= OnStartLeavingLocation;
            _rpcHandlerDecorator.OnLocationLeftEvent -= LocationLeft;
            _rpcHandlerDecorator.OnGenerateDungeonsEvent -= OnGenerateDungeons;
            _rpcHandlerDecorator.OnStartElevatorEvent -= OnStartElevator;
            _rpcHandlerDecorator.OnOpenElevatorEvent -= OnOpenElevator;
            _rpcHandlerDecorator.OnTogglePlayerInsideMobileHQEvent -= OnTogglePlayerInsideMobileHQ;
            _rpcHandlerDecorator.OnTeleportToFireExitEvent -= OnTeleportToFireExit;
        }
        
        private void OnCreateItemPreview(int slotIndex, int itemID) => CreateItemPreviewServerRpc(slotIndex, itemID);
        
        private void OnDestroyItemPreview(int slotIndex) =>
            DestroyItemPreviewServerRpc(slotIndex);

        private void OnLoadLocation(SceneName sceneName)
        {
            int sceneNameIndex = (int)sceneName;
            LoadLocationServerRpc(sceneNameIndex);
        }

        private void OnStartLeavingLocation() => StartLeavingLocationServerRpc();
        
        private void LocationLeft() => LeftLocationServerRpc();

        private void OnGenerateDungeons(DungeonsSeedData data) => GenerateDungeonsServerRpc(data);

        private void OnStartElevator(Floor floor) => StartElevatorServerRpc(floor);

        private void OnOpenElevator(Floor floor) => OpenElevatorServerRpc(floor);

        private void OnTogglePlayerInsideMobileHQ(ulong clientID, bool isInside) =>
            TogglePlayerInsideMobileHQServerRpc(clientID, isInside);

        private void OnTeleportToFireExit(Floor floor, bool isInStairsLocation) =>
            TeleportToFireExitServerRpc(floor, isInStairsLocation);
    }
}