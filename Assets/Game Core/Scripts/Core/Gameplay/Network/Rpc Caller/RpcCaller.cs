using System;
using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.Network
{
    // Owner Client ID всегда будет 0!
    public class RpcCaller : NetworkBehaviour
    {
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<CreateItemPreviewStaticData> OnCreateItemPreviewEvent;
        public event Action<int> OnDestroyItemPreviewEvent;
        public event Action<Vector3> OnTeleportPlayerWithOffsetEvent;

        private static RpcCaller _instance;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _instance = this;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void CreateItemPreview(int slotIndex, int itemID) =>
            CreateItemPreviewServerRpc(slotIndex, itemID);

        public void DestroyItemPreview(int slotIndex) =>
            DestroyItemPreviewServerRpc(slotIndex);

        public void TeleportPlayersWithOffset(Vector3 offset)
        {
            int x = (int)offset.x;
            int y = (int)offset.y;
            int z = (int)offset.z;
            
            TeleportPlayersWithOffsetServerRpc(x, y, z);
        }

        public static RpcCaller Get() => _instance;

        // RPC: -----------------------------------------------------------------------------------

        [ServerRpc(RequireOwnership = false)]
        private void CreateItemPreviewServerRpc(int slotIndex, int itemID, ServerRpcParams serverRpcParams = default)
        {
            ulong clientID = serverRpcParams.Receive.SenderClientId;

            CreateItemPreviewClientRpc(clientID, slotIndex, itemID);
        }

        [ClientRpc]
        private void CreateItemPreviewClientRpc(ulong clientID, int slotIndex, int itemID)
        {
            CreateItemPreviewStaticData data = new(clientID, slotIndex, itemID);

            OnCreateItemPreviewEvent?.Invoke(data);
        }

        [ServerRpc(RequireOwnership = false)]
        private void DestroyItemPreviewServerRpc(int slotIndex) =>
            DestroyItemPreviewClientRpc(slotIndex);

        [ClientRpc]
        private void DestroyItemPreviewClientRpc(int slotIndex) =>
            OnDestroyItemPreviewEvent?.Invoke(slotIndex);

        [ServerRpc(RequireOwnership = false)]
        private void TeleportPlayersWithOffsetServerRpc(int x, int y, int z) =>
            TeleportPlayersWithOffsetClientRpc(x, y, z);

        [ClientRpc]
        private void TeleportPlayersWithOffsetClientRpc(int x, int y, int z)
        {
            Vector3 offset = new(x, y, z);
            OnTeleportPlayerWithOffsetEvent?.Invoke(offset);
        }
    }
}