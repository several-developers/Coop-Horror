using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities;
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
    public abstract class ItemObjectBase : NetcodeBehaviour, IInteractableItem
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

        [SerializeField, ReadOnly]
        [LabelText("Item ID")]
        private int _itemIDDebug;

        [SerializeField, ReadOnly]
        [LabelText("Unique Item ID")]
        private int _uniqueItemIDDebug;

        // PROPERTIES: ----------------------------------------------------------------------------

        public int ItemID { get; private set; }
        public int UniqueItemID => _uniqueItemID.Value;
        public bool DestroyOnSceneUnload { get; private set; } = true;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnInteractionStateChangedEvent;

        private static int maxUniqueItemID;

        private readonly NetworkVariable<int> _uniqueItemID = new(writePerm: Constants.OwnerPermission);
        private readonly NetworkVariable<bool> _isPickedUp = new(writePerm: Constants.OwnerPermission);

        private IItemsProvider _itemsProvider;
        private FollowParent _followParent;
        private ClientNetworkTransform _networkTransform;
        private Rigidbody _rigidbody;
        private Collider _collider;
        private GameObject _child; // Model container

        private bool _isPickedUpLocal;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected virtual void Awake()
        {
            _networkTransform = GetComponent<ClientNetworkTransform>();
            _followParent = GetComponent<FollowParent>();
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            _child = transform.GetChild(0).gameObject;

            if (!_changeItemIDAtAwake)
                return;
                
            ItemID = _startItemID;
            _itemIDDebug = _startItemID;
        }

        private void OnCollisionEnter(Collision collision) => IgnorePlayerCollision(collision);

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Setup(int itemID, float scale) => SetupRpc(itemID, scale);

        public virtual void InteractionStarted(IEntity entity = null)
        {
        }

        public virtual void InteractionEnded(IEntity entity = null)
        {
        }

        public virtual void Interact(IEntity entity = null)
        {
            Debug.Log("Interacting with: " + name);
        }

        public void ToggleInteract(bool canInteract)
        {
        }

        public void ToggleDestroyOnSceneUnload(bool destroy) =>
            DestroyOnSceneUnload = destroy;

        public void PickUp()
        {
            if (_isPickedUp.Value)
                return;

            PickUpRpc(NetworkObject);
        }

        public void Drop(Vector3 position, Quaternion rotation, bool randomPosition = false, bool destroy = false)
        {
            if (!_isPickedUp.Value)
                return;
            
            DropRpc(position, rotation, randomPosition, destroy);
        }

        public void ShowServer() => ShowServerRpc();

        public void ShowClient() =>
            _child.SetActive(true);

        public void HideServer() => HideRpc();

        public void HideClient() =>
            _child.SetActive(false);

        public InteractionType GetInteractionType() =>
            InteractionType.PickUpItem;

        public bool CanInteract() =>
            !_isPickedUp.Value;

        public bool IsPickedUp() =>
            _isPickedUp.Value;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void InitServerOnly()
        {
            SetupItemUniqueID();

            // LOCAL METHODS: -----------------------------
            
            void SetupItemUniqueID()
            {
                maxUniqueItemID++;
                _uniqueItemIDDebug = maxUniqueItemID;
                _uniqueItemID.Value = maxUniqueItemID;
            }
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

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

        // RPC: -----------------------------------------------------------------------------------

        [Rpc(target: SendTo.Everyone)]
        private void SetupRpc(int itemID, float itemScale)
        {
            ItemID = itemID;
            transform.localScale *= itemScale;
            
            _itemsProvider.RegisterItem(item: this);
        }

        [Rpc(target: SendTo.Everyone, RequireOwnership = false)]
        private void PickUpRpc(NetworkObjectReference itemNetworkObjectReference)
        {
            if (IsOwner)
                _isPickedUp.Value = true;
            
            /*if (IsServerOnly)
            {
                bool isItemNetworkObjectExists = itemNetworkObjectReference.TryGet(out NetworkObject itemNetworkObject);

                if (!isItemNetworkObjectExists)
                    return;

                _isPickedUp.Value = true;

                ulong senderClientID = serverRpcParams.Receive.SenderClientId;
                bool changeOwnership = itemNetworkObject.OwnerClientId != senderClientID;

                if (changeOwnership)
                    itemNetworkObject.ChangeOwnership(senderClientID);
            }*/

            if (_isPickedUpLocal)
                return;

            _isPickedUpLocal = true;

            TogglePhysics(isEnabled: false);
            HideClient();
        }

        [Rpc(target: SendTo.Everyone, RequireOwnership = false)]
        private void DropRpc(Vector3 position, Quaternion rotation, bool randomPosition, bool destroy)
        {
            if (IsOwner)
            {
                if (destroy)
                {
                    _itemsProvider.RemoveItem(UniqueItemID);
                    Destroy(gameObject);
                    return;
                }

                if (randomPosition)
                    position = position.GetRandomPosition(radius: 0.5f);

                Vector3 scale = _networkTransform.transform.localScale;
                _networkTransform.Teleport(position, rotation, scale);

                _isPickedUp.Value = false;
            }

            if (!_isPickedUpLocal)
                return;

            _isPickedUpLocal = false;

            TogglePhysics(isEnabled: true);
            _followParent.RemoveTarget();
            ShowClient();
        }

        [ServerRpc(RequireOwnership = false)]
        private void ShowServerRpc(ServerRpcParams serverRpcParams = default)
        {
            ulong senderClientId = serverRpcParams.Receive.SenderClientId;
            ShowClientRpc(senderClientId);
        }

        [Rpc(target: SendTo.Everyone)]
        private void HideRpc() => HideClient();

        [ClientRpc]
        private void ShowClientRpc(ulong senderClientID)
        {
            bool show = senderClientID != OwnerClientId;

            if (!show)
                return;

            ShowClient();
        }
    }
}