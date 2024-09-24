using GameCore.Gameplay.Network;
using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.Entities
{
    [DisallowMultipleComponent]
    public abstract class Entity : NetcodeBehaviour, IEntity
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void SetParent(NetworkObject newParent) => SetParentRpc(newParent);
        
        public void RemoveParent() => RemoveParentRpc();
        
        public MonoBehaviour GetMonoBehaviour() => this;
        public Transform GetTransform() => transform;
        public NetworkObject GetNetworkObject() => NetworkObject;

        // RPC: -----------------------------------------------------------------------------------
        
        [Rpc(target: SendTo.Server)]
        private void SetParentRpc(NetworkObjectReference newParent)
        {
            bool isNetworkObjectFound = newParent.TryGet(out NetworkObject networkObject);

            if (!isNetworkObjectFound)
                return;
            
            NetworkObject.TrySetParent(networkObject);
        }
        
        [Rpc(target: SendTo.Server)]
        private void RemoveParentRpc() =>
            NetworkObject.TryRemoveParent();
    }
}