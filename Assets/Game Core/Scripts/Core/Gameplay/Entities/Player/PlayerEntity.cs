using System;
using GameCore.Gameplay.Entities.Inventory;
using GameCore.Gameplay.Entities.Player.Interaction;
using GameCore.Gameplay.Entities.Player.Movement;
using GameCore.Gameplay.Managers;
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
        private PlayerMovement _playerMovement;

        [SerializeField, Required]
        private Transform _itemHoldPivot;

        [SerializeField, Required]
        private Transform _cameraPoint;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action<Vector2> OnMovementVectorChangedEvent;

        private IPlayerInteractionObserver _playerInteractionObserver;

        private Inventory<ItemData> _inventory;
        private InteractionChecker _interactionChecker;
        private InteractionHandler _interactionHandler;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Update()
        {
            UpdateOwner();
            UpdateNotOwner();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void OnNetworkSpawn()
        {
            Init();
            base.OnNetworkSpawn();
        }

        public override void OnNetworkDespawn()
        {
            DespawnOwner();
            base.OnNetworkDespawn();
        }

        public void Setup(IPlayerInteractionObserver playerInteractionObserver)
        {
            _playerInteractionObserver = playerInteractionObserver;

            //float reloadTime = 1.5f;
            //float reloadAnimationTime = _animator.GetAnimationTime(AnimatorHashes.ReloadingAnimation);
            //float reloadTimeMultiplier = reloadAnimationTime / reloadTime; // 1, 0.5, 1 / 0.5

            //_animator.SetFloat(id: AnimatorHashes.ReloadMultiplier, reloadTimeMultiplier);
        }

        public void TakeDamage(IEntity source, float damage)
        {
            //_animator.SetTrigger(id: AnimatorHashes.HitReaction);
        }

        public Transform GetTransform() => transform;

        public Animator GetAnimator() => _animator;

        public NetworkObject GetNetworkObject() => _networkObject;

        public Inventory<ItemData> GetInventory() => _inventory;

        public bool IsDead() => _isDead;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void Init()
        {
            InitOwner();
        }

        private void InitOwner()
        {
            if (!IsOwner)
                return;

            var playerInput = GetComponent<PlayerInput>();
            InputSystemManager.Init(playerInput);

            PlayerCamera playerCamera = PlayerCamera.Get();
            playerCamera.SetTarget(transform);

            _inventory = new Inventory<ItemData>(Constants.PlayerInventorySize);
            _interactionChecker = new InteractionChecker(_playerInteractionObserver, transform, playerCamera.Camera,
                _interactionMaxDistance, interactionLM: _interactionLM, _interactionObstaclesLM);

            _interactionHandler =
                new InteractionHandler(playerEntity: this, _itemHoldPivot, _playerInteractionObserver);
            
            InventoryHUD.Get().Init(playerEntity: this); // TEMP

            InputSystemManager.OnMoveEvent += OnMove;
            InputSystemManager.OnScrollEvent += OnScroll;
            InputSystemManager.OnInteractEvent += OnInteract;
            InputSystemManager.OnDropItemEvent += OnDropItem;
        }

        private void UpdateOwner()
        {
            if (!IsOwner)
                return;

            _playerMovement.HandleMovement();
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

            _interactionHandler.Dispose();
            PlayerCamera.Get().RemoveTarget();

            InputSystemManager.OnMoveEvent -= OnMove;
            InputSystemManager.OnScrollEvent -= OnScroll;
            InputSystemManager.OnInteractEvent -= OnInteract;
            InputSystemManager.OnDropItemEvent -= OnDropItem;
        }

        private void Interact() =>
            _interactionHandler.Interact();

        private void DropItem()
        {
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnMove(Vector2 movementVector) =>
            OnMovementVectorChangedEvent?.Invoke(movementVector);

        private void OnScroll(float scrollValue)
        {
            bool switchToNextSlot = scrollValue <= 0;

            if (switchToNextSlot)
                _inventory.SwitchToNextSlot();
            else
                _inventory.SwitchToLastSlot();
        }

        private void OnInteract() => Interact();

        private void OnDropItem() => DropItem();
    }
}