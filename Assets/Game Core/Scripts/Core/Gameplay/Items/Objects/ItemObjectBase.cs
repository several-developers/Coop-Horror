using GameCore.Enums;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Network.Other;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.Items
{
    [RequireComponent(typeof(FollowParent))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public abstract class ItemObjectBase : NetworkBehaviour, IInteractableItem
    {
        // FIELDS: --------------------------------------------------------------------------------

        private FollowParent _followParent;
        private Rigidbody _rigidbody;
        private Collider _collider;
        
        [ShowInInspector]
        private int _itemID;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected virtual void Awake()
        {
            _followParent = GetComponent<FollowParent>();
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
        }

        protected virtual void Start()
        {
            //if (!IsSpawned)
                //NetworkObject.Spawn();
        }
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Setup(int itemID) =>
            _itemID = itemID;

        public virtual void Interact()
        {
            Debug.Log("Interacting with: " + name);
        }

        public void PickUp(NetworkObject playerNetworkObject) =>
            PickUpServerRpc(playerNetworkObject);

        public void Drop() => DropItemServerRpc();

        public NetworkObject GetNetworkObject() => NetworkObject;

        public InteractionType GetInteractionType() =>
            InteractionType.PickUpItem;
        
        public int GetItemID() => _itemID;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void PickUp(Transform target)
        {
            _rigidbody.isKinematic = true;
            _rigidbody.velocity = Vector3.zero;
            
            _collider.enabled = false;
            
            _followParent.SetTarget(target);
        }

        private void DropItem()
        {
            _rigidbody.isKinematic = false;
            _rigidbody.velocity = Vector3.zero;

            _collider.enabled = true;
            
            _followParent.RemoveTarget();
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void PickUpServerRpc(NetworkObjectReference playerNetworkObjectReference) =>
            PickUpClientRpc(playerNetworkObjectReference);

        [ClientRpc]
        private void PickUpClientRpc(NetworkObjectReference playerNetworkObjectReference)
        {
            bool isNetworkObjectFound = playerNetworkObjectReference.TryGet(out NetworkObject playerNetworkObject);

            if (!isNetworkObjectFound)
                return;

            bool isPlayerEntityFound = playerNetworkObject.TryGetComponent(out PlayerEntity playerEntity);

            if (!isPlayerEntityFound)
                return;

            Transform target = playerEntity.GetItemHoldPivot();
            PickUp(target);
        }

        [ServerRpc(RequireOwnership = false)]
        private void DropItemServerRpc() => DropItemClientRpc();

        [ClientRpc]
        private void DropItemClientRpc() => DropItem();
    }
}