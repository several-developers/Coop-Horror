using System;
using System.Collections.Generic;
using ECM2;
using GameCore.Gameplay.Entities.Inventory;
using GameCore.Gameplay.Entities.MobileHeadquarters;
using GameCore.Gameplay.Entities.Player.CameraManagement;
using GameCore.Gameplay.Entities.Player.Interaction;
using GameCore.Gameplay.Factories.ItemsPreview;
using GameCore.Gameplay.InputManagement;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Network.Utilities;
using GameCore.Infrastructure.Providers.Global;
using GameCore.Observers.Gameplay.PlayerInteraction;
using GameCore.Observers.Gameplay.Rpc;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
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
            IPlayerInteractionObserver playerInteractionObserver, IRpcObserver rpcObserver, PlayerCamera playerCamera,
            IConfigsProvider configsProvider)
        {
            _itemsPreviewFactory = itemsPreviewFactory;
            _mobileHeadquartersEntity = mobileHeadquartersEntity;
            _rpcHandlerDecorator = rpcHandlerDecorator;
            _playerInteractionObserver = playerInteractionObserver;
            _rpcObserver = rpcObserver;
            _playerCamera = playerCamera;
            InputReader = configsProvider.GetInputReader();
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

        // PROPERTIES: ----------------------------------------------------------------------------

        public PlayerReferences References => _references;
        public InputReader InputReader { get; private set; }

        // FIELDS: --------------------------------------------------------------------------------

        public static event Action<PlayerEntity> OnPlayerSpawnedEvent = delegate { };
        public static event Action<PlayerEntity> OnPlayerDespawnedEvent = delegate { };
        
        private const NetworkVariableWritePermission OwnerPermission = NetworkVariableWritePermission.Owner;

        private static readonly Dictionary<ulong, PlayerEntity> AllPlayers = new();

        private readonly NetworkVariable<Vector3> _lookAtPosition = new(writePerm: OwnerPermission);
        private readonly NetworkVariable<int> _currentSelectedSlotIndex = new();

        private static PlayerEntity _localPlayer;

        private IItemsPreviewFactory _itemsPreviewFactory;
        private IMobileHeadquartersEntity _mobileHeadquartersEntity;
        private IRpcHandlerDecorator _rpcHandlerDecorator;
        private IPlayerInteractionObserver _playerInteractionObserver;
        private IRpcObserver _rpcObserver;

        private PlayerCamera _playerCamera;
        private PlayerInventoryManager _inventoryManager;
        private PlayerInventory _inventory;
        private InteractionChecker _interactionChecker;
        private InteractionHandler _interactionHandler;

        private Transform _cameraRightHandItemsHolder;
        private Transform _lookAtObject;

        private bool _isInitialized;
        private bool _isInsideMobileHQ;
        private bool _isLocalPlayer;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Update()
        {
            if (!_isInitialized)
                return;

            TickServerAndClient();
            TickServer();
            TickClient();
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
            InitInventory();
            FixInvisiblePlayerBug();
            
            _rpcObserver.OnCreateItemPreviewEvent += OnCreateItemPreview;
            _rpcObserver.OnDestroyItemPreviewEvent += OnDestroyItemPreview;
            _rpcObserver.OnTogglePlayerInsideMobileHQEvent += OnTogglePlayerInsideMobileHQ;

            // LOCAL METHODS: -----------------------------

            void BasicInit()
            {
                AllPlayers.TryAdd(OwnerClientId, this);
                
                _cameraRightHandItemsHolder = _playerCamera.CameraReferences.RightHandItemsHolder;
            }
            
            void InitInventory()
            {
                _inventory = new PlayerInventory();

                _inventoryManager = new PlayerInventoryManager(playerEntity: this, _rpcHandlerDecorator,
                    _itemsPreviewFactory);
            }

            void FixInvisiblePlayerBug()
            {
                Transform thisTransform = transform;
                Vector3 startPosition = thisTransform.position;
                startPosition.x += 0.05f;
                thisTransform.position = startPosition;
            }
        }

        public void InitServer()
        {
            if (!IsOwner)
                return;
        }

        public void InitClient()
        {
            if (IsOwner)
                return;
            
            _currentSelectedSlotIndex.OnValueChanged += OnClientSelectedSlotChanged;
        }

        public void TickServerAndClient()
        {
        }

        public void TickServer()
        {
            if (!IsOwner)
                return;

            _references.HeadLookObject.position = _lookAtPosition.Value;
            
            if (_isLocalPlayer)
                _interactionChecker.Check();
        }

        public void TickClient()
        {
            if (IsOwner)
                return;

            _references.HeadLookObject.position = _lookAtPosition.Value;
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
            
            _currentSelectedSlotIndex.OnValueChanged -= OnClientSelectedSlotChanged;
        }

        public void DespawnClient()
        {
            if (IsOwner)
                return;
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
        
        public void KillSelf()
        {
            _inventory.DropAllItems();
            _playerCamera.gameObject.SetActive(false);
        }

        public void Revive() =>
            _playerCamera.gameObject.SetActive(true);

        public Transform GetTransform() => transform;

        public Transform GetCameraItemPivot() => _cameraRightHandItemsHolder;

        public Transform GetRightHandItemsHolder() =>
            _references.RightHandItemsHolder;

        public PlayerInventory GetInventory() => _inventory;

        public new bool IsLocalPlayer() => _isLocalPlayer;
        
        public bool IsDead() => _isDead;

        public static PlayerEntity GetLocalPlayer() => _localPlayer;

        public static bool TryGetPlayer(ulong clientID, out PlayerEntity playerEntity) =>
            AllPlayers.TryGetValue(clientID, out playerEntity);

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void LateUpdateServer()
        {
            if (!IsOwner)
                return;

            _lookAtPosition.Value = _lookAtObject.position;
        }

        private void InitLocalPlayer()
        {
            _isLocalPlayer = NetworkHorror.ClientID == OwnerClientId;
            
            Debug.LogWarning($"Is Local: {_isLocalPlayer}, " +
                             $"Client ID: {NetworkHorror.ClientID}, " +
                             $"Owner Client ID: {OwnerClientId}");

            if (_isLocalPlayer)
            {
                BasicInit();
                InitInput();
                InitMovement();
                InitInteractionCheckerAndHandler();
                
                _inventory.OnSelectedSlotChangedEvent += OnLocalPlayerSelectedSlotChanged;
            }

            // LOCAL METHODS: -----------------------------

            void BasicInit()
            {
                _localPlayer = this;
                _lookAtObject = _playerCamera.CameraReferences.LookAtObject;
                _playerCamera.Init(playerEntity: this);

                foreach (GameObject activeObject in _references.LocalPlayerActiveObjects)
                    activeObject.SetActive(true);
                
                foreach (GameObject inactiveObject in _references.LocalPlayerInactiveObjects)
                    inactiveObject.SetActive(false);

                foreach (SkinnedMeshRenderer meshRenderer in _references.HiddenMeshes)
                    meshRenderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            }

            void InitInput()
            {
                InputReader.OnScrollEvent += OnScroll;
                InputReader.OnInteractEvent += OnInteract;
                InputReader.OnDropItemEvent += OnDropItem;
            }

            void InitMovement()
            {
                Character character = _references.Character;
                character.camera = _playerCamera.CameraReferences.MainCamera;

                PlayerMovementController playerMovementController = _references.PlayerMovementController;
                playerMovementController.Setup(playerEntity: this);

                MyAnimationController animationController = _references.AnimationController;
                animationController.Setup(character, _playerCamera);
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

        private void DespawnLocalPlayer()
        {
            if (!_isLocalPlayer)
                return;
            
            _inventory.DropAllItems();
            _inventoryManager.Dispose();
            _interactionHandler.Dispose();
            _playerCamera.Disable();

            _inventory.OnSelectedSlotChangedEvent -= OnLocalPlayerSelectedSlotChanged;
            
            InputReader.OnScrollEvent -= OnScroll;
            InputReader.OnInteractEvent -= OnInteract;
            InputReader.OnDropItemEvent -= OnDropItem;
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

        // RPC: -----------------------------------------------------------------------------------

        [ServerRpc(RequireOwnership = false)]
        private void ChangeSelectedSlotServerRpc(int slotIndex) =>
            _currentSelectedSlotIndex.Value = slotIndex;

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            InitServerAndClient();
            InitServer();
            InitClient();
            InitLocalPlayer();

            _isInitialized = true;
            
            OnPlayerSpawnedEvent.Invoke(this);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            DespawnServerAndClient();
            DespawnServer();
            DespawnClient();
            DespawnLocalPlayer();

            _isInitialized = false;
            
            OnPlayerDespawnedEvent.Invoke(this);
        }
        
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
            if (!_isLocalPlayer)
                return;

            _inventoryManager.CreateItemPreviewClientSide(data.SlotIndex, data.ItemID, cameraPivot: false);
        }

        private void OnDestroyItemPreview(int slotIndex) =>
            _inventoryManager.DestroyItemPreview(slotIndex);

        private void OnLocalPlayerSelectedSlotChanged(ChangedSlotStaticData data)
        {
            int slotIndex = data.SlotIndex;
            
            if (IsOwner)
                _currentSelectedSlotIndex.Value = slotIndex;
            else
                ChangeSelectedSlotServerRpc(slotIndex);
        }

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