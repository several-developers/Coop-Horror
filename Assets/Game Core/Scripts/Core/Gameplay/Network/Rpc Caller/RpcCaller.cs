﻿using System;
using GameCore.Enums.Gameplay;
using GameCore.Enums.Global;
using GameCore.Gameplay.Dungeons;
using GameCore.Gameplay.HorrorStateMachineSpace;
using Unity.Netcode;
using Zenject;

namespace GameCore.Gameplay.Network
{
    // Owner Client ID всегда будет 0!
    public class RpcCaller : NetworkBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IHorrorStateMachine horrorStateMachine) =>
            _horrorStateMachine = horrorStateMachine;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action<CreateItemPreviewStaticData> OnCreateItemPreviewEvent = delegate { };
        public event Action<int> OnDestroyItemPreviewEvent = delegate { };
        public event Action OnLocationLoadedEvent = delegate { };
        public event Action OnLeavingLocationEvent = delegate { };
        public event Action OnLocationLeftEvent = delegate { };
        public event Action<DungeonsSeedData> OnGenerateDungeonsEvent = delegate { };
        public event Action<Floor> OnStartElevatorEvent = delegate { };
        public event Action<Floor> OnOpenElevatorEvent = delegate { };
        public event Action<ulong, bool> OnTogglePlayerInsideMobileHQEvent = delegate { };
        public event Action<ulong, Floor, bool> OnTeleportToFireExitEvent = delegate { };

        private static RpcCaller _instance;
        
        private IHorrorStateMachine _horrorStateMachine;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() => _instance = this;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void CreateItemPreview(int slotIndex, int itemID) =>
            CreateItemPreviewServerRpc(slotIndex, itemID);

        public void DestroyItemPreview(int slotIndex) =>
            DestroyItemPreviewServerRpc(slotIndex);

        public void LoadLocation(SceneName sceneName)
        {
            int sceneNameIndex = (int)sceneName;
            LoadLocationServerRpc(sceneNameIndex);
        }

        public void StartLeavingLocation() => StartLeavingLocationServerRpc();

        public void LeaveLocation() => LeaveLocationServerRpc();

        public void SendLocationLoaded() => SendLocationLoadedServerRpc();

        public void SendLeftLocation() => SendLeftLocationServerRpc();

        public void GenerateDungeons(DungeonsSeedData data) => GenerateDungeonsServerRpc(data);

        public void StartElevator(Floor floor) => StartElevatorServerRpc(floor);

        public void OpenElevator(Floor floor) => OpenElevatorServerRpc(floor);

        public void TogglePlayerInsideMobileHQEvent(ulong clientID, bool isInside) =>
            TogglePlayerInsideMobileHQServerRpc(clientID, isInside);

        public void TeleportToFireExit(Floor floor, bool isInStairsLocation) =>
            TeleportToFireExitServerRpc(floor, isInStairsLocation);

        public static RpcCaller Get() => _instance;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void LoadLocationLogic(int sceneNameIndex)
        {
            if (!IsOwner)
                return;
            
            var sceneName = (SceneName)sceneNameIndex;
            _horrorStateMachine.ChangeState<LoadLocationState, SceneName>(sceneName);
        }

        private void LeaveLocationLogic()
        {
            if (!IsOwner)
                return;
            
            _horrorStateMachine.ChangeState<LeaveLocationState>();
        }

        // RPC: -----------------------------------------------------------------------------------

        [ServerRpc(RequireOwnership = false)]
        private void CreateItemPreviewServerRpc(int slotIndex, int itemID, ServerRpcParams serverRpcParams = default)
        {
            ulong clientID = serverRpcParams.Receive.SenderClientId;

            CreateItemPreviewClientRpc(clientID, slotIndex, itemID);
        }

        [ServerRpc(RequireOwnership = false)]
        private void DestroyItemPreviewServerRpc(int slotIndex) =>
            DestroyItemPreviewClientRpc(slotIndex);

        [ServerRpc(RequireOwnership = false)]
        private void LoadLocationServerRpc(int sceneNameIndex) =>
            LoadLocationClientRpc(sceneNameIndex);

        [ServerRpc(RequireOwnership = false)]
        private void StartLeavingLocationServerRpc() => StartLeavingLocationClientRpc();

        [ServerRpc(RequireOwnership = false)]
        private void LeaveLocationServerRpc() => LeaveLocationClientRpc();

        [ServerRpc(RequireOwnership = false)]
        private void SendLocationLoadedServerRpc() => SendLocationLoadedClientRpc();

        [ServerRpc(RequireOwnership = false)]
        private void SendLeftLocationServerRpc() => SendLeftLocationClientRpc();

        [ServerRpc(RequireOwnership = false)]
        private void GenerateDungeonsServerRpc(DungeonsSeedData data) =>
            GenerateDungeonsClientRpc(data);

        [ServerRpc(RequireOwnership = false)]
        private void StartElevatorServerRpc(Floor floor) =>
            StartElevatorClientRpc(floor);

        [ServerRpc(RequireOwnership = false)]
        private void OpenElevatorServerRpc(Floor floor) =>
            OpenElevatorClientRpc(floor);

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
            OnCreateItemPreviewEvent.Invoke(data);
        }

        [ClientRpc]
        private void DestroyItemPreviewClientRpc(int slotIndex) =>
            OnDestroyItemPreviewEvent.Invoke(slotIndex);

        [ClientRpc]
        private void LoadLocationClientRpc(int sceneNameIndex) => LoadLocationLogic(sceneNameIndex);

        [ClientRpc]
        private void StartLeavingLocationClientRpc() =>
            OnLeavingLocationEvent.Invoke();

        [ClientRpc]
        private void LeaveLocationClientRpc() => LeaveLocationLogic();

        [ClientRpc]
        private void SendLocationLoadedClientRpc() =>
            OnLocationLoadedEvent.Invoke();

        [ClientRpc]
        private void SendLeftLocationClientRpc() =>
            OnLocationLeftEvent.Invoke();

        [ClientRpc]
        private void GenerateDungeonsClientRpc(DungeonsSeedData data) =>
            OnGenerateDungeonsEvent.Invoke(data);

        [ClientRpc]
        private void StartElevatorClientRpc(Floor floor) =>
            OnStartElevatorEvent.Invoke(floor);

        [ClientRpc]
        private void OpenElevatorClientRpc(Floor floor) =>
            OnOpenElevatorEvent.Invoke(floor);

        [ClientRpc]
        private void TogglePlayerInsideMobileHQClientRpc(ulong clientID, bool isInside) =>
            OnTogglePlayerInsideMobileHQEvent.Invoke(clientID, isInside);

        [ClientRpc]
        private void TeleportToFireExitClientRpc(ulong clientID, Floor floor, bool isInStairsLocation) =>
            OnTeleportToFireExitEvent.Invoke(clientID, floor, isInStairsLocation);
    }
}