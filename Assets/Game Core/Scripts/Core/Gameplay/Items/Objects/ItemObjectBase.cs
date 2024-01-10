using System;
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
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(EditorConstants.Debug)]
        [SerializeField]
        private bool _changeItemIDAtAwake;

        [SerializeField]
        [ShowIf(nameof(_changeItemIDAtAwake))]
        private int _startItemID;
        
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action OnInteractionStateChangedEvent;

        private Transform _transform;
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
            _transform = transform;
            _followParent = GetComponent<FollowParent>();
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            _child = transform.GetChild(0).gameObject;

            if (_changeItemIDAtAwake)
                _itemID = _startItemID;
        }

        protected virtual void Start()
        {
            //if (!IsSpawned)
                //NetworkObject.Spawn();
        }

        private void OnCollisionEnter(Collision collision) => IgnorePlayerCollision(collision);

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Setup(int itemID) =>
            _itemID = itemID;
        
        public virtual void Interact()
        {
            Debug.Log("Interacting with: " + name);
        }

        public void ToggleInteract(bool canInteract)
        {
        }

        public void PickUp(NetworkObject playerNetworkObject) =>
            PickUpServerRpc(playerNetworkObject);

        public void DropServer(bool randomPosition = false) => DropServerRpc(randomPosition);
        
        public void DropServer(Vector3 position, Quaternion rotation, bool randomPosition = false)
        {
            
        }

        // ПРОТЕСТИТЬ С ЛАГАМИ
        public void Drop(bool randomPosition = false)
        {
            if (!_isPickedUp)
                return;

            _isPickedUp = false;
            _rigidbody.isKinematic = false;
            _rigidbody.velocity = Vector3.zero;

            if (randomPosition)
                _transform.position = _transform.GetRandomPosition(radius: 0.5f);

            _collider.enabled = true;
            
            _followParent.RemoveTarget();
            Show();
        }

        public void ChangeFollowTarget(Transform followTarget) =>
            _followParent.SetTarget(followTarget);

        public void ShowServer() => ShowServerRpc();

        // ПРОТЕСТИТЬ С ЛАГАМИ
        public void Show() =>
            _child.SetActive(true);
        
        public void HideServer() => HideServerRpc();

        // ПРОТЕСТИТЬ С ЛАГАМИ
        public void Hide() =>
            _child.SetActive(false);

        public NetworkObject GetNetworkObject() => NetworkObject;

        public InteractionType GetInteractionType() =>
            InteractionType.PickUpItem;
        
        public int GetItemID() => _itemID;

        public bool CanInteract() => true;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void PickUp(Transform followTarget)
        {
            if (_isPickedUp)
                return;

            _isPickedUp = true;
            _rigidbody.isKinematic = true;
            _rigidbody.velocity = Vector3.zero;
            
            _collider.enabled = false;
            
            _followParent.SetTarget(followTarget);
        }

        private void IgnorePlayerCollision(Collision collision)
        {
            if (!collision.gameObject.CompareTag(Constants.PlayerTag))
                return;
            
            Physics.IgnoreCollision(collision.collider, _collider);
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

            Transform followTarget = playerEntity.GetItemFollowPivot();
            PickUp(followTarget);
        }

        [ServerRpc(RequireOwnership = false)]
        private void DropServerRpc(bool randomPosition) => DropClientRpc(randomPosition);

        [ClientRpc]
        private void DropClientRpc(bool randomPosition) => Drop(randomPosition);

        [ServerRpc(RequireOwnership = false)]
        private void ShowServerRpc(ServerRpcParams serverRpcParams = default)
        {
            ulong senderClientId = serverRpcParams.Receive.SenderClientId;
            ShowClientRpc(senderClientId);
        }

        [ClientRpc]
        private void ShowClientRpc(ulong senderClientID)
        {
            ulong receiverClientID = OwnerClientId;
            bool show = senderClientID != receiverClientID;
            
            if (!show)
                return;
            
            Show();
        }

        [ServerRpc(RequireOwnership = false)]
        private void HideServerRpc() => HideClientRpc();

        [ClientRpc]
        private void HideClientRpc() => Hide();
    }
}