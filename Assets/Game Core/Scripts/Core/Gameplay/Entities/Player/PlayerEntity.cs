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
    public class PlayerEntity : NetcodeBehaviour, IPlayerEntity
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IItemsPreviewFactory itemsPreviewFactory,
            IMobileHeadquartersEntity mobileHeadquartersEntity, IPlayerInteractionObserver playerInteractionObserver,
            IRpcObserver rpcObserver, PlayerCamera playerCamera, IConfigsProvider configsProvider)
        {
            _itemsPreviewFactory = itemsPreviewFactory;
            _mobileHeadquartersEntity = mobileHeadquartersEntity;
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
        private readonly NetworkVariable<int> _currentSelectedSlotIndex = new(writePerm: OwnerPermission);

        private static PlayerEntity _localPlayer;

        private IItemsPreviewFactory _itemsPreviewFactory;
        private IMobileHeadquartersEntity _mobileHeadquartersEntity;
        private IPlayerInteractionObserver _playerInteractionObserver;
        private IRpcObserver _rpcObserver;

        private PlayerCamera _playerCamera;
        private PlayerInventoryManager _inventoryManager;
        private PlayerInventory _inventory;
        private InteractionChecker _interactionChecker;
        private InteractionHandler _interactionHandler;

        private Transform _cameraRightHandItemsHolder;
        private Transform _lookAtObject;

        private bool _isInsideMobileHQ;

        // PUBLIC METHODS: ------------------------------------------------------------------------

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

        public bool IsDead() => _isDead;

        public static PlayerEntity GetLocalPlayer() => _localPlayer;

        public static bool TryGetPlayer(ulong clientID, out PlayerEntity playerEntity) =>
            AllPlayers.TryGetValue(clientID, out playerEntity);

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void InitServerAndClientOnce()
        {
            CameraReferences cameraReferences = _playerCamera.CameraReferences;
            _lookAtObject = cameraReferences.LookAtObject;
            _cameraRightHandItemsHolder = cameraReferences.RightHandItemsHolder;
            
            _inventory = new PlayerInventory();

            _inventoryManager = new PlayerInventoryManager(playerEntity: this, _itemsPreviewFactory);
        }

        protected override void InitLocalPlayerOnce()
        {
            Other();
            DeactivatePlayerMesh();
            InitInteractionSystem();
            InitPlayerMovement();
            FixInvisiblePlayerBug(); // TEMP

            InputReader.OnScrollEvent += OnScroll;
            InputReader.OnInteractEvent += OnInteract;
            InputReader.OnDropItemEvent += OnDropItem;
            
            _rpcObserver.OnTogglePlayerInsideMobileHQEvent += OnTogglePlayerInsideMobileHQ;

            _inventory.OnSelectedSlotChangedEvent += OnLocalPlayerSelectedSlotChanged;
            
            // LOCAL METHODS: -----------------------------

            void Other()
            {
                _localPlayer = this;
            }
            
            void DeactivatePlayerMesh()
            {
                foreach (GameObject activeObject in _references.LocalPlayerActiveObjects)
                    activeObject.SetActive(true);
                
                foreach (GameObject inactiveObject in _references.LocalPlayerInactiveObjects)
                    inactiveObject.SetActive(false);

                foreach (SkinnedMeshRenderer meshRenderer in _references.HiddenMeshes)
                    meshRenderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            }

            void InitInteractionSystem()
            {
                _interactionChecker = new InteractionChecker(_playerInteractionObserver, transform,
                    _playerCamera.CameraReferences.MainCamera, _interactionMaxDistance,
                    interactionLM: _interactionLM, _interactionObstaclesLM);

                _interactionHandler =
                    new InteractionHandler(playerEntity: this, _inventoryManager, _playerInteractionObserver);
            }

            void InitPlayerMovement()
            {
                Character character = _references.Character;
                character.camera = _playerCamera.CameraReferences.MainCamera;

                PlayerMovementController playerMovementController = _references.PlayerMovementController;
                playerMovementController.Setup(playerEntity: this);

                MyAnimationController animationController = _references.AnimationController;
                animationController.Setup(character, _playerCamera);
            }
            
            void FixInvisiblePlayerBug()
            {
                Transform thisTransform = transform;
                Vector3 startPosition = thisTransform.position;
                startPosition.x += 0.05f;
                thisTransform.position = startPosition;
            }
        }

        protected override void InitServerAndClient() =>
            AllPlayers.TryAdd(OwnerClientId, this);

        protected override void InitClient() =>
            _currentSelectedSlotIndex.OnValueChanged += OnClientSelectedSlotChanged;

        protected override void TickServer() =>
            _references.HeadLookObject.position = _lookAtPosition.Value;

        protected override void TickClient() =>
            _references.HeadLookObject.position = _lookAtPosition.Value;

        protected override void TickLocalPlayer() =>
            _interactionChecker.Check();

        protected override void LateTickServer() =>
            _lookAtPosition.Value = _lookAtObject.position;

        protected override void DespawnLocalPlayerOnce()
        {
            _inventory.DropAllItems();
            _inventoryManager.Dispose();
            _interactionHandler.Dispose();
            
            InputReader.OnScrollEvent -= OnScroll;
            InputReader.OnInteractEvent -= OnInteract;
            InputReader.OnDropItemEvent -= OnDropItem;

            
            _rpcObserver.OnTogglePlayerInsideMobileHQEvent -= OnTogglePlayerInsideMobileHQ;
            
            _inventory.OnSelectedSlotChangedEvent -= OnLocalPlayerSelectedSlotChanged;
        }

        protected override void DespawnServerAndClient() =>
            AllPlayers.Remove(OwnerClientId);

        protected override void DespawnClient() =>
            _currentSelectedSlotIndex.OnValueChanged -= OnClientSelectedSlotChanged;

        // PRIVATE METHODS: -----------------------------------------------------------------------

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

        [ServerRpc(RequireOwnership = false)]
        public void CreateItemPreviewServerRpc(int slotIndex, int itemID, ServerRpcParams serverRpcParams = default)
        {
            ulong senderClientID = serverRpcParams.Receive.SenderClientId;
            CreateItemPreviewClientRpc(senderClientID, slotIndex, itemID);
        }

        [ServerRpc(RequireOwnership = false)]
        public void DestroyItemPreviewServerRpc(int slotIndex, ServerRpcParams serverRpcParams = default)
        {
            ulong senderClientID = serverRpcParams.Receive.SenderClientId;
            DestroyItemPreviewClientRpc(senderClientID, slotIndex);
        }

        [ClientRpc]
        private void CreateItemPreviewClientRpc(ulong senderClientID, int slotIndex, int itemID)
        {
            bool isMatches = senderClientID == NetworkHorror.ClientID;

            if (isMatches)
                return;

            _inventoryManager.CreateItemPreviewClientSide(slotIndex, itemID, isFirstPerson: false);
        }

        [ClientRpc]
        private void DestroyItemPreviewClientRpc(ulong senderClientID, int slotIndex)
        {
            bool isMatches = senderClientID == NetworkHorror.ClientID;

            if (isMatches)
                return;
            
            _inventoryManager.DestroyItemPreview(slotIndex);
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            OnPlayerSpawnedEvent.Invoke(this);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

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
    }
}