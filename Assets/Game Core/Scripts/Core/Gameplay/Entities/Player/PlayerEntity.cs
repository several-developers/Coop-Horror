using System;
using System.Collections.Generic;
using GameCore.Gameplay.Entities.Inventory;
using GameCore.Gameplay.Entities.MobileHeadquarters;
using GameCore.Gameplay.Entities.Player.CameraManagement;
using GameCore.Gameplay.Entities.Player.Interaction;
using GameCore.Gameplay.Entities.Player.Movement;
using GameCore.Gameplay.Factories.ItemsPreview;
using GameCore.Gameplay.InputHandlerTEMP;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Network.Utilities;
using GameCore.Observers.Gameplay.PlayerInteraction;
using GameCore.Observers.Gameplay.Rpc;
using GameCore.Observers.Gameplay.UI;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace GameCore.Gameplay.Entities.Player
{
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerEntity : NetworkBehaviour, IPlayerEntity, INetcodeBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IItemsPreviewFactory itemsPreviewFactory,
            IMobileHeadquartersEntity mobileHeadquartersEntity, IRpcHandlerDecorator rpcHandlerDecorator,
            IPlayerInteractionObserver playerInteractionObserver, IRpcObserver rpcObserver, IUIObserver uiObserver)
        {
            _itemsPreviewFactory = itemsPreviewFactory;
            _mobileHeadquartersEntity = mobileHeadquartersEntity;
            _rpcHandlerDecorator = rpcHandlerDecorator;
            _playerInteractionObserver = playerInteractionObserver;
            _rpcObserver = rpcObserver;
            _uiObserver = uiObserver;
        }

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
        private ClientNetworkTransform _networkTransform;

        [SerializeField, Required]
        private Transform _playerItemPivot;

        [SerializeField, Required]
        private Transform _headLookObject;

        // PROPERTIES: ----------------------------------------------------------------------------

        public PlayerReferences References => _references;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action<Vector2> OnMovementVectorChangedEvent;

        private const NetworkVariableWritePermission OwnerPermission = NetworkVariableWritePermission.Owner;

        private static readonly Dictionary<ulong, PlayerEntity> AllPlayers = new();

        private readonly NetworkVariable<Vector3> _lookAtPosition = new(writePerm: OwnerPermission);
        private readonly NetworkVariable<int> _currentSelectedSlotIndex = new(writePerm: OwnerPermission);

        private static PlayerEntity _localPlayer;

        private IItemsPreviewFactory _itemsPreviewFactory;
        private IMobileHeadquartersEntity _mobileHeadquartersEntity;
        private IRpcHandlerDecorator _rpcHandlerDecorator;
        private IPlayerInteractionObserver _playerInteractionObserver;
        private IRpcObserver _rpcObserver;
        private IUIObserver _uiObserver;

        private PlayerCamera _playerCamera;
        private PlayerInventoryManager _inventoryManager;
        private PlayerInventory _inventory;
        private IMovementBehaviour _movementBehaviour;
        private InteractionChecker _interactionChecker;
        private InteractionHandler _interactionHandler;

        private Transform _cameraItemPivot;
        private Transform _lookAtObject;

        private bool _isInitialized;
        private bool _isInsideMobileHQ;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Update()
        {
            if (!_isInitialized)
                return;

            TickServerAndClient();
            TickServer();
            TickClient();
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

        public void InitServerAndClient()
        {
            BasicInit();
            GetPlayerCamera();
            GetRPCCaller();
            InitInventory();

            // LOCAL METHODS: -----------------------------

            void BasicInit()
            {
                AllPlayers.TryAdd(OwnerClientId, this);
            }

            void GetPlayerCamera()
            {
                _playerCamera = PlayerCamera.Get();

                if (_playerCamera != null)
                {
                    _cameraItemPivot = _playerCamera.CameraReferences.ItemPivot;
                    return;
                }

                string className = nameof(PlayerCamera).GetNiceName();
                string errorLog = Log.HandleLog($"<gb>{className}</gb> <rb>not found</rb>!");
                Debug.LogError(errorLog);
            }

            void GetRPCCaller()
            {
                _rpcObserver.OnCreateItemPreviewEvent += OnCreateItemPreview;
                _rpcObserver.OnDestroyItemPreviewEvent += OnDestroyItemPreview;
                _rpcObserver.OnTogglePlayerInsideMobileHQEvent += OnTogglePlayerInsideMobileHQ;
            }

            void InitInventory()
            {
                _inventory = new PlayerInventory();

                _inventoryManager = new PlayerInventoryManager(playerEntity: this, _rpcHandlerDecorator,
                    _itemsPreviewFactory);
            }
        }

        public void InitServer()
        {
            if (!IsOwner)
                return;

            BasicInit();
            InitInput();
            InitMovement();
            InitInteractionCheckerAndHandler();

            // LOCAL METHODS: -----------------------------

            void BasicInit()
            {
                _playerCamera.Init(playerEntity: this);
                _uiObserver.InitPlayer(playerEntity: this);
                _references.PlayerModel.SetActive(false);

                _localPlayer = this;
                _lookAtObject = _playerCamera.CameraReferences.LookAtObject;

                _inventory.OnSelectedSlotChangedEvent += OnOwnerSelectedSlotChanged;
            }

            void InitInput()
            {
                InputReader inputReader = _references.InputReader; // TEMP

                inputReader.OnMoveEvent += OnMove;
                inputReader.OnScrollEvent += OnScroll;
                inputReader.OnInteractEvent += OnInteract;
                inputReader.OnDropItemEvent += OnDropItem;
            }

            void InitMovement()
            {
                _movementBehaviour = new PhysicalMovementBehaviour3(playerEntity: this);
            }

            void InitInteractionCheckerAndHandler()
            {
                _interactionChecker = new InteractionChecker(_playerInteractionObserver, transform,
                    _playerCamera.CameraReferences.MainCamera, _interactionMaxDistance,
                    interactionLM: _interactionLM, _interactionObstaclesLM);

                _interactionHandler =
                    new InteractionHandler(playerEntity: this, _inventoryManager, _playerInteractionObserver);
            }
        }

        public void InitClient()
        {
            if (IsOwner)
                return;

            BasicInit();
            FixInvisiblePlayerBug();

            // LOCAL METHODS: -----------------------------

            void BasicInit()
            {
                _currentSelectedSlotIndex.OnValueChanged += OnClientSelectedSlotChanged;
            }

            void FixInvisiblePlayerBug()
            {
                Transform thisTransform = transform;
                Vector3 startPosition = thisTransform.position;
                startPosition.x += 0.05f;
                thisTransform.position = startPosition;
            }
        }

        public void TickServerAndClient()
        {
            // TO DO
        }

        public void TickServer()
        {
            if (!IsOwner)
                return;

            if (Input.GetKeyDown(KeyCode.T))
                GameUtilities.SwapCursorLockState();

            _movementBehaviour.Tick();
            _interactionChecker.Check();
        }

        public void TickClient()
        {
            if (IsOwner)
                return;

            _headLookObject.position = _lookAtPosition.Value;
        }

        public void DespawnServerAndClient()
        {
            _rpcObserver.OnCreateItemPreviewEvent -= OnCreateItemPreview;
            _rpcObserver.OnDestroyItemPreviewEvent -= OnDestroyItemPreview;
            _rpcObserver.OnTogglePlayerInsideMobileHQEvent -= OnTogglePlayerInsideMobileHQ;
        }

        public void DespawnServer()
        {
            if (!IsOwner)
                return;

            _playerCamera.Disable();
            _inventoryManager.Dispose();
            _interactionHandler.Dispose();

            _inventory.OnSelectedSlotChangedEvent -= OnOwnerSelectedSlotChanged;

            InputReader inputReader = _references.InputReader; // TEMP

            inputReader.OnMoveEvent -= OnMove;
            inputReader.OnScrollEvent -= OnScroll;
            inputReader.OnInteractEvent -= OnInteract;
            inputReader.OnDropItemEvent -= OnDropItem;
        }

        public void DespawnClient()
        {
            if (IsOwner)
                return;

            _inventory.DropAllItems();

            _currentSelectedSlotIndex.OnValueChanged -= OnClientSelectedSlotChanged;
        }

        public void TakeDamage(IEntity source, float damage)
        {
            // TO DO
        }

        public void TeleportPlayer(Vector3 position, Quaternion rotation)
        {
            _playerCamera.EnableSnap();
            _networkTransform.Teleport(position, rotation, transform.localScale);
        }

        public void ToggleInsideMobileHQ(bool isInside)
        {
            if (isInside)
                SetMobileHQAsParent();
            else
                RemoveParent();
        }

        public void DropItem(bool destroy = false) =>
            _inventory.DropItem(destroy);

        public Transform GetTransform() => transform;

        public Transform GetCameraItemPivot() => _cameraItemPivot;

        public Transform GetPlayerItemPivot() => _playerItemPivot;

        public PlayerInventory GetInventory() => _inventory;
        
        public bool IsDead() => _isDead;

        public static PlayerEntity GetLocalPlayer() => _localPlayer;

        public static bool TryGetPlayer(ulong clientID, out PlayerEntity playerEntity) =>
            AllPlayers.TryGetValue(clientID, out playerEntity);

        // PRIVATE METHODS: -----------------------------------------------------------------------

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

        private void Interact() =>
            _interactionHandler.Interact();

        private void SetMobileHQAsParent()
        {
            NetworkObject networkObject = _mobileHeadquartersEntity.GetNetworkObject();
            NetworkObject.TrySetParent(networkObject);
        }

        private void RemoveParent() =>
            NetworkObject.TryRemoveParent();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            InitServerAndClient();
            InitServer();
            InitClient();

            _isInitialized = true;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            DespawnServerAndClient();
            DespawnServer();
            DespawnClient();
        }

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

            _inventoryManager.CreateItemPreviewClientSide(data.SlotIndex, data.ItemID, cameraPivot: false);
        }

        private void OnDestroyItemPreview(int slotIndex) =>
            _inventoryManager.DestroyItemPreview(slotIndex);

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