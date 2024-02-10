using System;
using GameCore.Enums;
using GameCore.Gameplay.Dungeons;
using GameCore.Gameplay.HorrorStateMachineSpace;
using GameCore.Gameplay.Network.Other;
using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.Network
{
    // Owner Client ID всегда будет 0!
    public class RpcCaller : NetworkBehaviour
    {
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<CreateItemPreviewStaticData> OnCreateItemPreviewEvent = delegate {  };
        public event Action<int> OnDestroyItemPreviewEvent = delegate {  };
        public event Action<Vector3> OnTeleportPlayerWithOffsetEvent = delegate {  };
        public event Action OnLocationLoadedEvent = delegate {  };
        public event Action OnLeavingLocationEvent = delegate {  };
        public event Action OnLocationLeftEvent = delegate {  };
        public event Action<DungeonsSeedData> OnGenerateDungeonsEvent = delegate {  };

        private static RpcCaller _instance;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _instance = this;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void CreateItemPreview(int slotIndex, int itemID) =>
            CreateItemPreviewServerRpc(slotIndex, itemID);

        public void DestroyItemPreview(int slotIndex) =>
            DestroyItemPreviewServerRpc(slotIndex);

        public void TeleportPlayersWithOffset(Vector3 offset) =>
            TeleportPlayersWithOffsetServerRpc(offset.x, offset.y, offset.z);

        public void LoadLocation(SceneName sceneName)
        {
            int sceneNameIndex = (int)sceneName;
            LoadLocationServerRpc(sceneNameIndex);
        }

        public void StartLeavingLocation() => StartLeavingLocationServerRpc();

        public void LeaveLocation() => LeaveLocationServerRpc();
        
        public void SendLocationLoaded() => SendLocationLoadedServerRpc();

        public void SendLeftLocation() => SendLeftLocationServerRpc();

        public void SendGenerateDungeons(DungeonsSeedData data) => SendGenerateDungeonsServerRpc(data);

        public static RpcCaller Get() => _instance;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void LoadLocationLogic(int sceneNameIndex)
        {
            NetworkServiceLocator networkServiceLocator = NetworkServiceLocator.Get();
            IHorrorStateMachine horrorStateMachine = networkServiceLocator.GetHorrorStateMachine();

            if (IsOwner)
            {
                var sceneName = (SceneName)sceneNameIndex;
                horrorStateMachine.ChangeState<LoadLocationState, SceneName>(sceneName);
            }
        }

        private void LeaveLocationLogic()
        {
            if (!IsServer)
                return;
            
            NetworkServiceLocator networkServiceLocator = NetworkServiceLocator.Get();
            IHorrorStateMachine horrorStateMachine = networkServiceLocator.GetHorrorStateMachine();

            horrorStateMachine.ChangeState<LeaveLocationState>();
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
        private void TeleportPlayersWithOffsetServerRpc(float x, float y, float z) =>
            TeleportPlayersWithOffsetClientRpc(x, y, z);

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
        private void SendGenerateDungeonsServerRpc(DungeonsSeedData data) =>
            SendGenerateDungeonsClientRpc(data);

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
        private void TeleportPlayersWithOffsetClientRpc(float x, float y, float z)
        {
            Vector3 offset = new(x, y, z);
            OnTeleportPlayerWithOffsetEvent.Invoke(offset);
        }

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
        private void SendGenerateDungeonsClientRpc(DungeonsSeedData data) =>
            OnGenerateDungeonsEvent.Invoke(data);
    }
}