using System;
using GameCore.Gameplay.Entities.Inventory;
using GameCore.Gameplay.Entities.MobileHeadquarters;
using GameCore.Gameplay.Entities.Player.Interaction;
using GameCore.Gameplay.Managers;
using GameCore.Gameplay.Network;
using GameCore.Observers.Gameplay.PlayerInteraction;
using GameCore.UI.Gameplay.Inventory;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameCore.Gameplay.Entities.Player
{
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerEntity : NetworkBehaviour, IPlayerEntity
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private bool _isDead;

        [SerializeField, Min(0)]
        private float _interactionMaxDistance = 2f;

        [SerializeField]
        private LayerMask _interactionLM;

        [SerializeField]
        private LayerMask _interactionObstaclesLM;

        [Title(Constants.References)]
        [SerializeField, Required]
        private Animator _animator;

        [SerializeField, Required]
        private NetworkObject _networkObject;
        
        [SerializeField, Required]
        private Transform _itemHoldPivot;

        [SerializeField, Required]
        private Transform _cameraPoint;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action<Vector2> OnMovementVectorChangedEvent;

        private PlayerInventoryManager _inventoryManager;
        private PlayerInventory _inventory;
        private InteractionChecker _interactionChecker;
        private InteractionHandler _interactionHandler;
        private bool _isInitialized;

        private MobileHeadquartersEntity _mobileHeadquartersEntity;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Update()
        {
            if (!_isInitialized)
                return;
            
            UpdateOwner();
            UpdateNotOwner();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void OnNetworkSpawn()
        {
            TheNetworkHorror networkHorror = TheNetworkHorror.Get();
            networkHorror.OnDisconnectEvent -= OnDisconnect;
            
            base.OnNetworkSpawn();
        }

        public override void OnNetworkDespawn()
        {
            DespawnOwner();
            base.OnNetworkDespawn();
        }

        public void Setup()
        {
            //float reloadTime = 1.5f;
            //float reloadAnimationTime = _animator.GetAnimationTime(AnimatorHashes.ReloadingAnimation);
            //float reloadTimeMultiplier = reloadAnimationTime / reloadTime; // 1, 0.5, 1 / 0.5

            //_animator.SetFloat(id: AnimatorHashes.ReloadMultiplier, reloadTimeMultiplier);
            
            TheNetworkHorror networkHorror = TheNetworkHorror.Get();
            networkHorror.OnDisconnectEvent += OnDisconnect;
            
            InitOwner();
            InitNotOwner();
            
            _isInitialized = true;
        }

        public void TakeDamage(IEntity source, float damage)
        {
            //_animator.SetTrigger(id: AnimatorHashes.HitReaction);
        }

        public Transform GetTransform() => transform;

        public Transform GetItemHoldPivot() => _itemHoldPivot;

        public Animator GetAnimator() => _animator;

        public NetworkObject GetNetworkObject() => _networkObject;

        public PlayerInventory GetInventory() => _inventory;

        public bool IsDead() => _isDead;

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void InitOwner()
        {
            if (!IsOwner)
                return;

            Debug.Log($"Player #{OwnerClientId} setup.");
            IPlayerInteractionObserver playerInteractionObserver = NetworkSpawner.Get().PlayerInteractionObserver;
            
            var playerInput = GetComponent<PlayerInput>();
            InputSystemManager.Init(playerInput);
            
            PlayerCamera playerCamera = PlayerCamera.Get();
            playerCamera.SetTarget(transform, _cameraPoint);

            _inventory = new PlayerInventory();
            _inventoryManager = new PlayerInventoryManager(playerEntity: this);
            _interactionChecker = new InteractionChecker(playerInteractionObserver, transform, playerCamera.Camera,
                _interactionMaxDistance, interactionLM: _interactionLM, _interactionObstaclesLM);

            _interactionHandler = new InteractionHandler(_inventoryManager, playerInteractionObserver);

            _mobileHeadquartersEntity = MobileHeadquartersEntity.Get();

            InventoryHUD.Get().Init(playerEntity: this); // TEMP

            InputSystemManager.OnMoveEvent += OnMove;
            InputSystemManager.OnScrollEvent += OnScroll;
            InputSystemManager.OnInteractEvent += OnInteract;
            InputSystemManager.OnDropItemEvent += OnDropItem;
        }

        private void InitNotOwner()
        {
            if (IsOwner)
                return;
        }
        
        private void UpdateOwner()
        {
            if (!IsOwner)
                return;

            _interactionChecker.Check();
        }

        private void UpdateNotOwner()
        {
            if (IsOwner)
                return;
        }

        private void DespawnOwner()
        {
            if (!IsOwner)
                return;

            _inventoryManager.Dispose();
            _interactionHandler.Dispose();
            PlayerCamera.Get().RemoveTarget();

            InputSystemManager.OnMoveEvent -= OnMove;
            InputSystemManager.OnScrollEvent -= OnScroll;
            InputSystemManager.OnInteractEvent -= OnInteract;
            InputSystemManager.OnDropItemEvent -= OnDropItem;
        }

        private void Interact() =>
            _interactionHandler.Interact();

        private void DropItem() =>
            _inventory.DropItem();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnDisconnect() =>
            _inventory.DropAllItems();

        private void OnMove(Vector2 movementVector) =>
            OnMovementVectorChangedEvent?.Invoke(movementVector);

        private void OnScroll(float scrollValue)
        {
            bool switchToNextSlot = scrollValue <= 0;

            if (switchToNextSlot)
                _inventory.SwitchToNextSlot();
            else
                _inventory.SwitchToPreviousSlot();
        }

        private void OnInteract() => Interact();

        private void OnDropItem() => DropItem();
    }
}