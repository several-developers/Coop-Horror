using GameCore.Enums;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Network.Other;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.Items
{
    [RequireComponent(typeof(FollowParent))]
    [RequireComponent(typeof(Rigidbody))]
    public abstract class ItemObjectBase : NetworkBehaviour, IInteractableItem
    {
        // FIELDS: --------------------------------------------------------------------------------

        private FollowParent _followParent;
        private Rigidbody _rigidbody;
        private Collider _collider;
        private GameObject _child; // Model container
        
        private bool _isPickedUp;
        
        [ShowInInspector]
        private int _itemID;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected virtual void Awake()
        {
            _followParent = GetComponent<FollowParent>();
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            _child = transform.GetChild(0).gameObject;
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

        public void DropServer(bool randomPosition = false) => DropServerRpc(randomPosition);
        
        public void Drop(bool randomPosition = false)
        {
            if (!_isPickedUp)
                return;

            _isPickedUp = false;
            _rigidbody.isKinematic = false;
            _rigidbody.velocity = Vector3.zero;

            if (randomPosition)
                transform.position = transform.GetRandomPosition(radius: 0.5f);

            _collider.enabled = true;
            
            _followParent.RemoveTarget();
        }

        public void ShowServer() => ShowServerRpc();

        public void Show() =>
            _child.SetActive(true);
        
        public void HideServer() => HideServerRpc();

        public void Hide() =>
            _child.SetActive(false);

        public NetworkObject GetNetworkObject() => NetworkObject;

        public InteractionType GetInteractionType() =>
            InteractionType.PickUpItem;
        
        public int GetItemID() => _itemID;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void PickUp(Transform target)
        {
            if (_isPickedUp)
                return;

            _isPickedUp = true;
            _rigidbody.isKinematic = true;
            _rigidbody.velocity = Vector3.zero;
            
            _collider.enabled = false;
            
            _followParent.SetTarget(target);
        }

        // RPC: -----------------------------------------------------------------------------------
        
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
        private void DropServerRpc(bool randomPosition) => DropClientRpc(randomPosition);

        [ClientRpc]
        private void DropClientRpc(bool randomPosition) => Drop(randomPosition);

        [ServerRpc(RequireOwnership = false)]
        private void ShowServerRpc() => ShowClientRpc();

        [ClientRpc]
        private void ShowClientRpc() => Show();

        [ServerRpc(RequireOwnership = false)]
        private void HideServerRpc() => HideClientRpc();

        [ClientRpc]
        private void HideClientRpc() => Hide();
    }
}