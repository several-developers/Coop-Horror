using System;
using Unity.Netcode;

namespace GameCore.Gameplay.Network
{
    public class RpcCaller : NetworkBehaviour
    {
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<int, int> OnCreateItemPreviewEvent;
        public event Action<int> OnDestroyItemPreviewEvent;
        
        private static RpcCaller _instance;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _instance = this;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void CreateItemPreview(int slotIndex, int itemID) =>
            CreateItemPreviewServerRpc(slotIndex, itemID);

        public void DestroyItemPreview(int slotIndex) =>
            DestroyItemPreviewServerRpc(slotIndex);

        public static RpcCaller Get() => _instance;

        // RPC: -----------------------------------------------------------------------------------
        
        [ServerRpc(RequireOwnership = false)]
        private void CreateItemPreviewServerRpc(int slotIndex, int itemID) =>
            CreateItemPreviewClientRpc(slotIndex, itemID);

        [ClientRpc]
        private void CreateItemPreviewClientRpc(int slotIndex, int itemID) =>
            OnCreateItemPreviewEvent?.Invoke(slotIndex, itemID);
        
        [ServerRpc(RequireOwnership = false)]
        private void DestroyItemPreviewServerRpc(int slotIndex) =>
            DestroyItemPreviewClientRpc(slotIndex);

        [ClientRpc]
        private void DestroyItemPreviewClientRpc(int slotIndex) =>
            OnDestroyItemPreviewEvent?.Invoke(slotIndex);
    }
}