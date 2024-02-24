using System;
using GameCore.Gameplay.Entities.Inventory;
using GameCore.Gameplay.Entities.MobileHeadquarters;
using GameCore.Gameplay.Entities.Player.Interaction;
using GameCore.Gameplay.Entities.Player.Movement;
using GameCore.Gameplay.Entities.Player.Other;
using GameCore.Gameplay.Factories.ItemsPreview;
using GameCore.Gameplay.InputHandlerTEMP;
using GameCore.Gameplay.Network;
using GameCore.Observers.Gameplay.PlayerInteraction;
using GameCore.UI.Gameplay.Inventory;
using GameCore.Utilities;
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
        [SerializeField]
        private PlayerReferences _references;

        [SerializeField, Required]
        private NetworkObject _networkObject;

        [SerializeField, Required]
        private ClientNetworkTransform _networkTransform;

        [SerializeField, Required]
        private Transform _playerItemPivot;

        [SerializeField, Required]
        private Transform _headLookObject;

        // PROPERTIES: ----------------------------------------------------------------------------

        public PlayerReferences References => _references;
        public bool IsInsideMobileHQ => _isInsideMobileHQ;

        // FIELDS: --------------------------------------------------------------------------------

        public static PlayerEntity Instance; // TEMP, move

        public event Action<Vector2> OnMovementVectorChangedEvent;

        private const NetworkVariableWritePermission OwnerPermission = NetworkVariableWritePermission.Owner;

        private readonly NetworkVariable<Vector3> _lookAtPosition = new(writePerm: OwnerPermission);
        private readonly NetworkVariable<int> _currentSelectedSlotIndex = new(writePerm: OwnerPermission);

        private TheNetworkHorror _networkHorror;
        private RpcCaller _rpcCaller;
        private Gameplay.PlayerCamera.CameraController _cameraController;

        private PlayerInventoryManager _inventoryManager;
        private PlayerInventory _inventory;
        private IMovementBehaviour _movementBehaviour;
        private InteractionChecker _interactionChecker;
        private InteractionHandler _interactionHandler;
        private Transform _cameraItemPivot;
        private Transform _lookAtObject;

        private bool _isInitialized;
        private bool _isInsideMobileHQ;

        private MobileHeadquartersEntity _mobileHeadquartersEntity;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Update()
        {
            if (!_isInitialized)
                return;

            UpdateServer();
            UpdateClient();
        }

        private void FixedUpdate()
        {
            if (!_isInitialized)
                return;

            FixedUpdateServer();
        }

        private void LateUpdate()
        {
            if (!_isInitialized)
                return;

            LateUpdateServer();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
        }

        public override void OnNetworkDespawn()
        {
            Despawn();
            base.OnNetworkDespawn();
        }

        public void Init()
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

            InventoryHUD inventoryHUD = InventoryHUD.Get(); // TEMP

            if (inventoryHUD == null)
            {
                _isInitialized = false;
                return;
            }

            _networkHorror = TheNetworkHorror.Get();
            _networkHorror.OnDisconnectEvent += OnDisconnect;

            InitServerAndClient();
            InitServer();
            InitClient();

            _isInitialized = true;
        }

        public void TakeDamage(IEntity source, float damage)
        {
            //_animator.SetTrigger(id: AnimatorHashes.HitReaction);
        }

        public void TeleportPlayer(Vector3 position, Quaternion rotation)
        {
            _cameraController.ToggleSnap();
            _networkTransform.Teleport(position, rotation, transform.localScale);
        }

        public void ToggleInsideMobileHQ(bool isInside)
        {
            if (isInside)
                SetMobileHQAsParent();
            else
                RemoveParent();
        }

        public Transform GetTransform() => transform;

        public Transform GetCameraItemPivot() => _cameraItemPivot;

        public Transform GetPlayerItemPivot() => _playerItemPivot;

        public NetworkObject GetNetworkObject() => _networkObject;

        public PlayerInventory GetInventory() => _inventory;

        public bool IsDead() => _isDead;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void InitServerAndClient()
        {
            _rpcCaller = RpcCaller.Get();
            _cameraController = Gameplay.PlayerCamera.CameraController.Get();
            _mobileHeadquartersEntity = MobileHeadquartersEntity.Get();

            _cameraItemPivot = _cameraController.CameraReferences.ItemPivot;

            PlayerSetupHelper playerSetupHelper = PlayerSetupHelper.Get();
            IItemsPreviewFactory itemsPreviewFactory = playerSetupHelper.GetItemsPreviewFactory();

            _inventory = new PlayerInventory();
            _inventoryManager = new PlayerInventoryManager(playerEntity: this, _rpcCaller, itemsPreviewFactory);

            _rpcCaller.OnCreateItemPreviewEvent += OnCreateItemPreview;
            _rpcCaller.OnDestroyItemPreviewEvent += OnDestroyItemPreview;
            _rpcCaller.OnTeleportPlayerWithOffsetEvent += OnTeleportPlayerWithOffset;
            _rpcCaller.OnTogglePlayerInsideMobileHQEvent += OnTogglePlayerInsideMobileHQ;
        }

        private void InitServer()
        {
            if (!IsOwner)
                return;

            Debug.Log($"Player #{OwnerClientId} setup Owner.");

            Instance = this;

            InitMovement();
            BasicInit();

            _references.PlayerModel.SetActive(false);

            InputReader inputReader = _references.InputReader; // TEMP

            inputReader.OnMoveEvent += OnMove;
            inputReader.OnScrollEvent += OnScroll;
            inputReader.OnInteractEvent += OnInteract;
            inputReader.OnDropItemEvent += OnDropItem;

            // LOCAL METHODS: -----------------------------

            void InitMovement()
            {
                Transform cameraTransform = _cameraController.transform;
                _movementBehaviour = new PhysicalMovementBehaviour3(playerEntity: this);
            }

            void BasicInit()
            {
                IPlayerInteractionObserver playerInteractionObserver = NetworkSpawner.Get().PlayerInteractionObserver;

                _cameraController.Init(playerEntity: this);

                _lookAtObject = _cameraController.CameraReferences.LookAtObject;

                _interactionChecker = new InteractionChecker(playerInteractionObserver, transform,
                    _cameraController.CameraReferences.MainCamera, _interactionMaxDistance,
                    interactionLM: _interactionLM, _interactionObstaclesLM);

                _interactionHandler = new InteractionHandler(_inventoryManager, playerInteractionObserver);

                InventoryHUD inventoryHUD = InventoryHUD.Get(); // TEMP
                if (inventoryHUD != null)
                    inventoryHUD.Init(playerEntity: this); // TEMP

                _inventory.OnSelectedSlotChangedEvent += OnOwnerSelectedSlotChanged;
            }
        }

        private void InitClient()
        {
            if (IsOwner)
                return;

            _currentSelectedSlotIndex.OnValueChanged += OnClientSelectedSlotChanged;

            Debug.Log($"Player #{OwnerClientId} setup Client.");
        }

        private void UpdateServer()
        {
            if (!IsOwner)
                return;

            if (Input.GetKeyDown(KeyCode.T))
                GameUtilities.SwapCursorLockState();

            _movementBehaviour.Tick();
            _interactionChecker.Check();

            /*Vector2 movementInput = _playerMovement.InputManager.GetMovementInput();
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
            _animator.SetFloat(AnimatorHashes.MoveSpeedBlend, blend);*/
        }

        private void UpdateClient()
        {
            if (IsOwner)
                return;

            _headLookObject.position = _lookAtPosition.Value;
        }

        private void FixedUpdateServer()
        {
            if (!IsOwner)
                return;

            _movementBehaviour.FixedTick();
        }

        private void LateUpdateServer()
        {
            if (!IsOwner)
                return;

            _lookAtPosition.Value = _lookAtObject.position;
        }

        private void Despawn()
        {
            DespawnServerAndClient();
            DespawnServer();
            DespawnClient();
        }

        private void DespawnServerAndClient()
        {
            _networkHorror.OnDisconnectEvent -= OnDisconnect;

            _rpcCaller.OnCreateItemPreviewEvent -= OnCreateItemPreview;
            _rpcCaller.OnDestroyItemPreviewEvent -= OnDestroyItemPreview;
            _rpcCaller.OnTeleportPlayerWithOffsetEvent -= OnTeleportPlayerWithOffset;
            _rpcCaller.OnTogglePlayerInsideMobileHQEvent -= OnTogglePlayerInsideMobileHQ;
        }

        private void DespawnServer()
        {
            if (!IsOwner)
                return;

            _inventoryManager.Dispose();
            _interactionHandler.Dispose();

            _inventory.OnSelectedSlotChangedEvent -= OnOwnerSelectedSlotChanged;

            InputReader inputReader = _references.InputReader; // TEMP

            inputReader.OnMoveEvent -= OnMove;
            inputReader.OnScrollEvent -= OnScroll;
            inputReader.OnInteractEvent -= OnInteract;
            inputReader.OnDropItemEvent -= OnDropItem;
        }

        private void DespawnClient()
        {
            if (IsOwner)
                return;

            _currentSelectedSlotIndex.OnValueChanged -= OnClientSelectedSlotChanged;
        }

        private void Interact() =>
            _interactionHandler.Interact();

        private void DropItem() =>
            _inventory.DropItem();

        private void SetMobileHQAsParent() =>
            _networkObject.TrySetParent(_mobileHeadquartersEntity.NetworkObject);

        private void RemoveParent() =>
            _networkObject.TryRemoveParent();

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

        private void OnTogglePlayerInsideMobileHQ(ulong clientID, bool isInside)
        {
            if (isInside == _isInsideMobileHQ)
                return;

            bool isMatches = clientID == OwnerClientId;

            if (!isMatches)
                return;

            _isInsideMobileHQ = isInside;
            _networkTransform.InLocalSpace = isInside;
        }
    }
}