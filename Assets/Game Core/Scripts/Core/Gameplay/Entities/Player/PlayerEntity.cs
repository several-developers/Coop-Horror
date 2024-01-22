using System;
using EFPController;
using GameCore.Gameplay.Entities.Inventory;
using GameCore.Gameplay.Entities.MobileHeadquarters;
using GameCore.Gameplay.Entities.Player.Interaction;
using GameCore.Gameplay.Entities.Player.Other;
using GameCore.Gameplay.Factories.ItemsPreview;
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

        [Title("TEMP ------------------")]
        [SerializeField, Required]
        private EFPController.Player _player;

        [SerializeField, Required]
        private PlayerMovement _playerMovement;

        [SerializeField, Required]
        private PlayerFootsteps _playerFootsteps;

        private CameraBobAnims _cameraBobAnims;

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
        private OwnerNetworkAnimator _networkAnimator;

        [SerializeField, Required]
        private NetworkObject _networkObject;

        [SerializeField, Required]
        private Rigidbody _rigidbody;
        
        [SerializeField, Required]
        private GameObject _playerModel;

        [SerializeField, Required]
        private Transform _cameraPoint;

        [SerializeField, Required]
        private Transform _playerItemPivot;

        [SerializeField, Required]
        private Transform _headLookObject;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action<Vector2> OnMovementVectorChangedEvent;

        private const NetworkVariableWritePermission OwnerPermission = NetworkVariableWritePermission.Owner;
        
        private readonly NetworkVariable<Vector3> _lookAtPosition = new(writePerm: OwnerPermission);
        private readonly NetworkVariable<int> _currentSelectedSlotIndex = new(writePerm: OwnerPermission);

        private TheNetworkHorror _networkHorror;
        private RpcCaller _rpcCaller;
        private PlayerCamera _playerCamera;

        private PlayerInventoryManager _inventoryManager;
        private PlayerInventory _inventory;
        private InteractionChecker _interactionChecker;
        private InteractionHandler _interactionHandler;
        private Transform _cameraItemPivot;
        private Transform _lookAtObject;

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

        private void FixedUpdate() => FixedUpdateOwner();

        private void LateUpdate() => LateUpdateOwner();

        public override void OnDestroy()
        {
            if (_isInitialized)
            {
                if (IsOwner)
                {
                    _inventory.OnSelectedSlotChangedEvent -= OnOwnerSelectedSlotChanged;
                }
                
                if (!IsOwner)
                {
                    _currentSelectedSlotIndex.OnValueChanged -= OnClientSelectedSlotChanged;
                }
                
                _rpcCaller.OnCreateItemPreviewEvent -= OnCreateItemPreview;
                _rpcCaller.OnDestroyItemPreviewEvent -= OnDestroyItemPreview;
                _rpcCaller.OnTeleportPlayerWithOffsetEvent -= OnTeleportPlayerWithOffset;
            }

            base.OnDestroy();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
        }

        public override void OnNetworkDespawn()
        {
            _networkHorror.OnDisconnectEvent -= OnDisconnect;

            DespawnOwner();
            base.OnNetworkDespawn();
        }

        public void Setup()
        {
            //float reloadTime = 1.5f;
            //float reloadAnimationTime = _animator.GetAnimationTime(AnimatorHashes.ReloadingAnimation);
            //float reloadTimeMultiplier = reloadAnimationTime / reloadTime; // 1, 0.5, 1 / 0.5

            //_animator.SetFloat(id: AnimatorHashes.ReloadMultiplier, reloadTimeMultiplier);

            if (_isInitialized)
            {
                Debug.Log($"Player #{OwnerClientId} already initialized.");
                return;
            }

            _networkHorror = TheNetworkHorror.Get();
            _networkHorror.OnDisconnectEvent += OnDisconnect;

            CommonInit();
            InitOwner();
            InitNotOwner();

            _isInitialized = true;
        }

        public void TakeDamage(IEntity source, float damage)
        {
            //_animator.SetTrigger(id: AnimatorHashes.HitReaction);
        }

        public Transform GetTransform() => transform;

        public Transform GetCameraItemPivot() => _cameraItemPivot;

        public Transform GetPlayerItemPivot() => _playerItemPivot;

        public Animator GetAnimator() => _animator;

        public NetworkObject GetNetworkObject() => _networkObject;

        public PlayerInventory GetInventory() => _inventory;

        public bool IsDead() => _isDead;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CommonInit()
        {
            _rpcCaller = RpcCaller.Get();
            _playerCamera = PlayerCamera.Get();
            
            _cameraItemPivot = _playerCamera.ItemPivot;

            PlayerSetupHelper playerSetupHelper = PlayerSetupHelper.Get();
            IItemsPreviewFactory itemsPreviewFactory = playerSetupHelper.GetItemsPreviewFactory();

            _inventory = new PlayerInventory();
            _inventoryManager = new PlayerInventoryManager(playerEntity: this, _rpcCaller, itemsPreviewFactory);

            _rpcCaller.OnCreateItemPreviewEvent += OnCreateItemPreview;
            _rpcCaller.OnDestroyItemPreviewEvent += OnDestroyItemPreview;
            _rpcCaller.OnTeleportPlayerWithOffsetEvent += OnTeleportPlayerWithOffset;
        }

        private void InitOwner()
        {
            if (!IsOwner)
                return;

            Debug.Log($"Player #{OwnerClientId} setup Owner.");

            BasicInit();

            _playerModel.SetActive(false);

            InputSystemManager.OnMoveEvent += OnMove;
            InputSystemManager.OnScrollEvent += OnScroll;
            InputSystemManager.OnInteractEvent += OnInteract;
            InputSystemManager.OnDropItemEvent += OnDropItem;

            // LOCAL METHODS: -----------------------------

            void BasicInit()
            {
                IPlayerInteractionObserver playerInteractionObserver = NetworkSpawner.Get().PlayerInteractionObserver;

                var playerInput = GetComponent<PlayerInput>();
                InputSystemManager.Init(playerInput);
                
                _playerMovement.Init(_player, _playerCamera.Camera.transform);
                _lookAtObject = _playerCamera.LookAtObject;

                SmoothLook smoothLook = _playerCamera.SmoothLook;
                smoothLook.Init(_player);

                CameraRootAnimations cameraRootAnimations = _playerCamera.CameraRootAnimations;

                CameraControl cameraControl = _playerCamera.CameraControl;
                cameraControl.Init(_player, cameraRootAnimations);

                _player.Init(smoothLook, cameraControl, _playerCamera.Camera, _playerCamera.WeaponRoot,
                    _playerCamera.CameraRoot, _playerCamera.CameraAnimator);

                CameraBobAnims cameraBobAnims = _player.cameraBobAnims;
                cameraBobAnims.Init(_player, cameraRootAnimations);
                _cameraBobAnims = cameraBobAnims;
                
                _interactionChecker = new InteractionChecker(playerInteractionObserver, transform, _playerCamera.Camera,
                    _interactionMaxDistance, interactionLM: _interactionLM, _interactionObstaclesLM);

                _interactionHandler = new InteractionHandler(_inventoryManager, playerInteractionObserver);

                _mobileHeadquartersEntity = MobileHeadquartersEntity.Get();

                InventoryHUD.Get().Init(playerEntity: this); // TEMP

                _rigidbody.isKinematic = true;
                _rigidbody.isKinematic = false;

                _playerMovement.OnJump += () => { _networkAnimator.SetTrigger(AnimatorHashes.Jump); };

                _inventory.OnSelectedSlotChangedEvent += OnOwnerSelectedSlotChanged;
            }
        }

        private void InitNotOwner()
        {
            if (IsOwner)
                return;

            _currentSelectedSlotIndex.OnValueChanged += OnClientSelectedSlotChanged;

            Debug.Log($"Player #{OwnerClientId} setup Client.");
        }

        private void UpdateOwner()
        {
            if (!IsOwner)
                return;

            _interactionChecker.Check(true);
            _player.UpdateLogic();
            _playerMovement.UpdateLogic();
            _playerFootsteps.UpdateLogic();
            _cameraBobAnims.UpdateLogic();

            Vector2 movementInput = _playerMovement.InputManager.GetMovementInput();
            bool isMovingByPlayer = movementInput.magnitude > 0.05f;
            bool isSprinting = _playerMovement.SprintComponent.IsSprinting;
            bool isGoingBackwards = _playerMovement.InputY < 0f;
            float maxClamp = isSprinting ? 1f : 0.5f;
            float movementSpeed = _playerMovement.Velocity.magnitude;
            float modifiedMovementSpeed = movementSpeed * (isGoingBackwards ? -1f : 1f);

            float divider = isSprinting ? 6f : 3f;
            float blend = Mathf.Clamp(value: modifiedMovementSpeed / divider, -1f, maxClamp);

            bool canMove = isMovingByPlayer && movementSpeed > 0.05f;

            _animator.SetBool(id: AnimatorHashes.CanMove, canMove);
            _animator.SetFloat(AnimatorHashes.MoveSpeedBlend, blend);
        }

        private void UpdateNotOwner()
        {
            if (IsOwner)
                return;

            _headLookObject.position = _lookAtPosition.Value;
        }

        private void FixedUpdateOwner()
        {
            if (!IsOwner)
                return;

            _playerMovement.FixedUpdateLogic();
        }

        private void LateUpdateOwner()
        {
            if (!IsOwner || !_isInitialized)
                return;

            _playerMovement.LateUpdateLogic();

            _lookAtPosition.Value = _lookAtObject.position;
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

        private void OnCreateItemPreview(CreateItemPreviewStaticData data)
        {
            bool isItemOwner = OwnerClientId == data.ClientID;

            if (!isItemOwner)
                return;

            _inventoryManager.CreateItemPreview(data.SlotIndex, data.ItemID, cameraPivot: false);
        }

        private void OnDestroyItemPreview(int slotIndex) =>
            _inventoryManager.DestroyItemPreview(slotIndex);
        
        private void OnTeleportPlayerWithOffset(Vector3 offset)
        {
            Vector3 currentPosition = transform.position;
            Vector3 newPosition = currentPosition + offset;
            transform.position = newPosition;
        }

        private void OnOwnerSelectedSlotChanged(int slotIndex) =>
            _currentSelectedSlotIndex.Value = slotIndex;
        
        private void OnClientSelectedSlotChanged(int previousValue, int newValue)
        {
            _inventory.SetSelectedSlotIndex(newValue);
            _inventoryManager.ToggleItemsState();
        }
    }
}