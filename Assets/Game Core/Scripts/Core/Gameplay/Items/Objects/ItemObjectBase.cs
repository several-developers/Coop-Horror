using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Network.Other;
using GameCore.Infrastructure.Providers.Gameplay.Items;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Items
{
    [RequireComponent(typeof(FollowParent))]
    [RequireComponent(typeof(Rigidbody))]
    public abstract class ItemObjectBase : NetworkBehaviour, IInteractableItem
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IItemsProvider itemsProvider) =>
            _itemsProvider = itemsProvider;

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(EditorConstants.Debug)]
        [SerializeField]
        private bool _changeItemIDAtAwake;

        [SerializeField]
        [ShowIf(nameof(_changeItemIDAtAwake))]
        private int _startItemID;

        // PROPERTIES: ----------------------------------------------------------------------------

        public int ItemID => _itemID;
        public int UniqueItemID => _uniqueItemID;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnInteractionStateChangedEvent;

        private static int maxUniqueItemID;

        private readonly NetworkVariable<bool> _isPickedUp = new();

        private IItemsProvider _itemsProvider;
        private FollowParent _followParent;
        private ClientNetworkTransform _networkTransform;
        private Rigidbody _rigidbody;
        private Collider _collider;
        private GameObject _child; // Model container
        private bool _isPickedUpLocal;

        [ShowInInspector]
        private int _itemID;

        [ShowInInspector]
        private int _uniqueItemID;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected virtual void Awake()
        {
            _networkTransform = GetComponent<ClientNetworkTransform>();
            _followParent = GetComponent<FollowParent>();
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            _child = transform.GetChild(0).gameObject;

            if (_changeItemIDAtAwake)
                _itemID = _startItemID;

            SetupItemUniqueID();
            RegisterItem();
        }

        private void OnCollisionEnter(Collision collision) => IgnorePlayerCollision(collision);

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Setup(int itemID) =>
            _itemID = itemID;

        public virtual void InteractionStarted()
        {
        }

        public virtual void InteractionEnded()
        {
        }

        public virtual void Interact(PlayerEntity playerEntity = null)
        {
            Debug.Log("Interacting with: " + name);
        }

        public void ToggleInteract(bool canInteract)
        {
        }

        public void PickUp()
        {
            if (_isPickedUp.Value)
                return;
            
            PickUpClientLogic();
            PickUpServerRpc(NetworkObject);
        }

        public void Drop(Vector3 position, Quaternion rotation, bool randomPosition = false, bool destroy = false)
        {
            if (!_isPickedUp.Value)
                return;

            if (!destroy)
            {
                if (randomPosition)
                    position = position.GetRandomPosition(radius: 0.5f);

                _networkTransform.Teleport(position, rotation, newScale: Vector3.one);

                DropClientLogic();
            }

            DropServerRpc(destroy);
        }
        
        public void ShowServer() => ShowServerRpc();

        public void ShowClient() =>
            _child.SetActive(true);

        public void HideServer() => HideServerRpc();

        public void HideClient() =>
            _child.SetActive(false);

        public InteractionType GetInteractionType() =>
            InteractionType.PickUpItem;

        public bool CanInteract() =>
            !_isPickedUp.Value;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void PickUpClientLogic()
        {
            if (_isPickedUpLocal)
                return;

            _isPickedUpLocal = true;
            
            TogglePhysics(isEnabled: false);
            HideClient();
        }

        private void DropClientLogic()
        {
            if (!_isPickedUpLocal)
                return;

            _isPickedUpLocal = false;
            
            TogglePhysics(isEnabled: true);
            _followParent.RemoveTarget();
            ShowClient();
        }
        
        private void TogglePhysics(bool isEnabled)
        {
            _rigidbody.isKinematic = !isEnabled;
            _rigidbody.velocity = Vector3.zero;

            _collider.enabled = isEnabled;
        }

        private void IgnorePlayerCollision(Collision collision)
        {
            if (!collision.gameObject.CompareTag(Constants.PlayerTag))
                return;

            Physics.IgnoreCollision(collision.collider, _collider);
        }

        private void SetupItemUniqueID()
        {
            maxUniqueItemID++;
            _uniqueItemID = maxUniqueItemID;
        }

        private void RegisterItem() =>
            _itemsProvider.RegisterItem(item: this);

        // RPC: -----------------------------------------------------------------------------------
        
        [ServerRpc(RequireOwnership = false)]
        private void PickUpServerRpc(NetworkObjectReference itemNetworkObjectReference,
            ServerRpcParams serverRpcParams = default)
        {
            bool isItemNetworkObjectExists = itemNetworkObjectReference.TryGet(out NetworkObject itemNetworkObject);

            if (!isItemNetworkObjectExists)
                return;
            
            _isPickedUp.Value = true;

            ulong senderClientID = serverRpcParams.Receive.SenderClientId;
            bool changeOwnership = itemNetworkObject.OwnerClientId != senderClientID;
            
            if (changeOwnership)
                itemNetworkObject.ChangeOwnership(senderClientID);
            
            PickUpClientRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void DropServerRpc(bool destroy)
        {
            if (destroy)
            {
                _itemsProvider.RemoveItem(_uniqueItemID);
                Destroy(gameObject);
                return;
            }
            
            _isPickedUp.Value = false;
            
            DropClientRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void ShowServerRpc(ServerRpcParams serverRpcParams = default)
        {
            ulong senderClientId = serverRpcParams.Receive.SenderClientId;
            ShowClientRpc(senderClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        private void HideServerRpc() => HideClientRpc();

        [ClientRpc]
        private void PickUpClientRpc() => PickUpClientLogic();

        [ClientRpc]
        private void DropClientRpc() => DropClientLogic();

        [ClientRpc]
        private void ShowClientRpc(ulong senderClientID)
        {
            bool show = senderClientID != OwnerClientId;

            if (!show)
                return;

            ShowClient();
        }

        [ClientRpc]
        private void HideClientRpc() => HideClient();
    }
}