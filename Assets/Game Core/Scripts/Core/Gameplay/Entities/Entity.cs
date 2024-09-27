using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Network;
using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.Entities
{
    [DisallowMultipleComponent]
    public abstract class Entity : NetcodeBehaviour, IEntity, IReParentable
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        protected readonly NetworkVariable<EntityLocation> CurrentLocation = new(writePerm: OwnerPermission);
        protected readonly NetworkVariable<Floor> CurrentFloor = new(writePerm: OwnerPermission);
        
        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void SetParent(NetworkObject newParent) => SetParentRpc(newParent);
        public void RemoveParent() => RemoveParentRpc();
        public void SetEntityLocation(EntityLocation entityLocation) => SetEntityLocationRpc(entityLocation);
        public void SetFloor(Floor floor) => SetFloorRpc(floor);
        public MonoBehaviour GetMonoBehaviour() => this;
        public Transform GetTransform() => transform;
        public NetworkObject GetNetworkObject() => NetworkObject;
        
        public EntityLocation GetCurrentLocation() =>
            CurrentLocation.Value;

        public Floor GetCurrentFloor() =>
            CurrentFloor.Value;

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

        [Rpc(target: SendTo.Owner)]
        private void SetEntityLocationRpc(EntityLocation entityLocation) =>
            CurrentLocation.Value = entityLocation;
        
        [Rpc(target: SendTo.Owner)]
        private void SetFloorRpc(Floor floor) =>
            CurrentFloor.Value = floor;
    }
}