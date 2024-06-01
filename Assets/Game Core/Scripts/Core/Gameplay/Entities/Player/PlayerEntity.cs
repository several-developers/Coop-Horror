using System;
using System.Collections.Generic;
using ECM2;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Inventory;
using GameCore.Gameplay.Entities.Player.CameraManagement;
using GameCore.Gameplay.Entities.Player.Interaction;
using GameCore.Gameplay.Factories.ItemsPreview;
using GameCore.Gameplay.InputManagement;
using GameCore.Gameplay.Network;
using GameCore.Infrastructure.Providers.Global;
using GameCore.Observers.Gameplay.PlayerInteraction;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using Zenject;

namespace GameCore.Gameplay.Entities.Player
{
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerEntity : NetcodeBehaviour, IEntity, IDamageable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IItemsPreviewFactory itemsPreviewFactory,
            IPlayerInteractionObserver playerInteractionObserver, PlayerCamera playerCamera,
            IConfigsProvider configsProvider)
        {
            _itemsPreviewFactory = itemsPreviewFactory;
            _playerInteractionObserver = playerInteractionObserver;
            _playerCamera = playerCamera;
            InputReader = configsProvider.GetInputReader();
            
            Debug.LogWarning("CONSTRUCTOR " + (playerCamera == null));
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

        // PROPERTIES: ----------------------------------------------------------------------------

        public PlayerReferences References => _references;
        public InputReader InputReader { get; private set; }

        // FIELDS: --------------------------------------------------------------------------------

        public static event Action<PlayerEntity> OnPlayerSpawnedEvent = delegate { };
        public static event Action<PlayerEntity> OnPlayerDespawnedEvent = delegate { };

        public event Action<PlayerLocation> OnPlayerLocationChangedEvent = delegate { };

        private const NetworkVariableWritePermission OwnerPermission = NetworkVariableWritePermission.Owner;

        private static readonly Dictionary<ulong, PlayerEntity> AllPlayers = new();

        private readonly NetworkVariable<PlayerLocation> _playerLocation = new(writePerm: OwnerPermission);
        private readonly NetworkVariable<Vector3> _lookAtPosition = new(writePerm: OwnerPermission);
        private readonly NetworkVariable<int> _currentSelectedSlotIndex = new(writePerm: OwnerPermission);

        private static PlayerEntity _localPlayer;

        private IItemsPreviewFactory _itemsPreviewFactory;
        private IPlayerInteractionObserver _playerInteractionObserver;

        private PlayerCamera _playerCamera;
        private PlayerInventoryManager _inventoryManager;
        private PlayerInventory _inventory;
        private InteractionChecker _interactionChecker;
        private InteractionHandler _interactionHandler;

        private Transform _cameraLookAtObject;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void TakeDamage(IEntity source, float damage)
        {
            // TO DO
        }

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            _references.NetworkTransform.InLocalSpace = false;
            _references.NetworkTransform.Teleport(position, rotation, transform.localScale);
        }

        public void SetParent(NetworkObject parentNetworkObject)
        {
            if (IsServer)
                TrySetParent(parentNetworkObject);
            else
                TrySetParentServerRpc(parentNetworkObject);
        }

        public void RemoveParent()
        {
            if (IsServer)
                TryRemoveParent();
            else
                TryRemoveParentServerRpc();
        }

#warning ПРОВЕРИТЬ ИЛИ КОРРЕКТНО РАБОТАЕТ, ДОБАВИТЬ СЕРВЕР РПС
        public void ChangePlayerLocation(PlayerLocation playerLocation)
        {
            if (!IsOwner)
                return;

            _playerLocation.Value = playerLocation;
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

        public PlayerInventory GetInventory() => _inventory;

        public PlayerLocation GetPlayerLocation() =>
            _playerLocation.Value;

        public bool IsDead() => _isDead;

        public static IReadOnlyDictionary<ulong, PlayerEntity> GetAllPlayers() => AllPlayers;

        public static PlayerEntity GetLocalPlayer() => _localPlayer;

        public static bool TryGetPlayer(ulong clientID, out PlayerEntity playerEntity) =>
            AllPlayers.TryGetValue(clientID, out playerEntity);

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void InitAll()
        {
            AllPlayers.TryAdd(OwnerClientId, this);

            _inventory = new PlayerInventory();

            _inventoryManager = new PlayerInventoryManager(playerEntity: this, _itemsPreviewFactory);
        }

        protected override void InitOwner()
        {
            Other();
            DeactivatePlayerMesh();
            InitInteractionSystem();
            InitPlayerMovement();

            InputReader.OnScrollEvent += OnScrollInventory;
            InputReader.OnInteractEvent += OnInteract;
            InputReader.OnDropItemEvent += OnDropItem;

            _playerLocation.OnValueChanged += OnOwnerPlayerLocationChanged;
            _inventory.OnSelectedSlotChangedEvent += OnOwnerSelectedSlotChanged;

            // LOCAL METHODS: -----------------------------

            void Other()
            {
                _localPlayer = this;

                CameraReferences cameraReferences = _playerCamera.CameraReferences;
                _cameraLookAtObject = cameraReferences.LookAtObject;
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
        }

        protected override void InitNotOwner()
        {
            FixInvisiblePlayerBug(); // TEMP

            _currentSelectedSlotIndex.OnValueChanged += OnNotOwnerSelectedSlotChanged;

            // LOCAL METHODS: -----------------------------

            void FixInvisiblePlayerBug()
            {
                Transform thisTransform = transform;
                Vector3 startPosition = thisTransform.position;
                startPosition.x += 0.05f;
                thisTransform.position = startPosition;
            }
        }

        protected override void TickOwner()
        {
            _lookAtPosition.Value = _cameraLookAtObject.position;
            _interactionChecker.Check();
        }

        protected override void TickNotOwner() =>
            _references.HeadLookAtObject.position = _lookAtPosition.Value;

        protected override void DespawnAll()
        {
            _inventory.DropAllItems();
            _inventoryManager.Dispose();

            AllPlayers.Remove(OwnerClientId);
        }

        protected override void DespawnOwner()
        {
            _interactionHandler.Dispose();

            InputReader.OnScrollEvent -= OnScrollInventory;
            InputReader.OnInteractEvent -= OnInteract;
            InputReader.OnDropItemEvent -= OnDropItem;

            _playerLocation.OnValueChanged -= OnOwnerPlayerLocationChanged;
            _inventory.OnSelectedSlotChangedEvent -= OnOwnerSelectedSlotChanged;
        }

        protected override void DespawnNotOwner() =>
            _currentSelectedSlotIndex.OnValueChanged -= OnNotOwnerSelectedSlotChanged;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void Interact() =>
            _interactionHandler.Interact();

        private void TrySetParent(NetworkObject networkObject) =>
            NetworkObject.TrySetParent(networkObject);

        private void TryRemoveParent() =>
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

        [ServerRpc(RequireOwnership = false)]
        private void TrySetParentServerRpc(NetworkObjectReference networkObjectReference)
        {
            bool isNetworkObjectFound = networkObjectReference.TryGet(out NetworkObject networkObject);

            if (!isNetworkObjectFound)
                return;

            TrySetParent(networkObject);
        }

        [ServerRpc(RequireOwnership = false)]
        private void TryRemoveParentServerRpc() => TryRemoveParent();

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

        private void OnTransformParentChanged()
        {
            bool hasParent = transform.parent != null;
            _references.NetworkTransform.InLocalSpace = hasParent;
            
            _references.Rigidbody.interpolation = hasParent 
                ? RigidbodyInterpolation.None
                : RigidbodyInterpolation.Interpolate;
        }

        private void OnScrollInventory(float scrollValue)
        {
            bool switchToNextSlot = scrollValue <= 0;

            if (switchToNextSlot)
                _inventory.SwitchToNextSlot();
            else
                _inventory.SwitchToPreviousSlot();
        }

        private void OnInteract() => Interact();

        private void OnDropItem() => DropItem();

        private void OnOwnerSelectedSlotChanged(ChangedSlotStaticData data)
        {
            int slotIndex = data.SlotIndex;

            if (IsOwner)
                _currentSelectedSlotIndex.Value = slotIndex;
            else
                ChangeSelectedSlotServerRpc(slotIndex);
        }

        private void OnNotOwnerSelectedSlotChanged(int previousValue, int newValue)
        {
            _inventory.SetSelectedSlotIndex(newValue);
            _inventoryManager.ToggleItemsState();
        }

        private void OnOwnerPlayerLocationChanged(PlayerLocation previousValue, PlayerLocation newValue) =>
            OnPlayerLocationChangedEvent.Invoke(newValue);
    }
}