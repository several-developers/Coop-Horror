using System;
using System.Collections.Generic;
using ECM2;
using GameCore.Configs.Gameplay.Player;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.CamerasManagement;
using GameCore.Gameplay.Entities.MobileHeadquarters;
using GameCore.Gameplay.Entities.Player.CameraManagement;
using GameCore.Gameplay.Entities.Player.Interaction;
using GameCore.Gameplay.Entities.Player.States;
using GameCore.Gameplay.EntitiesSystems.Health;
using GameCore.Gameplay.EntitiesSystems.Inventory;
using GameCore.Gameplay.EntitiesSystems.Ragdoll;
using GameCore.Gameplay.Factories.ItemsPreview;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.InputManagement;
using GameCore.Gameplay.Network;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
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
    public class PlayerEntity : NetcodeBehaviour, ITeleportableEntity, IDamageable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(
            IItemsPreviewFactory itemsPreviewFactory,
            IPlayerInteractionObserver playerInteractionObserver,
            IGameManagerDecorator gameManagerDecorator,
            ICamerasManager camerasManager,
            IMobileHeadquartersEntity mobileHeadquartersEntity,
            IGameplayConfigsProvider gameplayConfigsProvider,
            IConfigsProvider configsProvider
        )
        {
            _itemsPreviewFactory = itemsPreviewFactory;
            _playerInteractionObserver = playerInteractionObserver;
            _gameManagerDecorator = gameManagerDecorator;
            _camerasManager = camerasManager;
            _mobileHeadquartersEntity = mobileHeadquartersEntity;

            _playerConfig = gameplayConfigsProvider.GetPlayerConfig();
            InputReader = configsProvider.GetInputReader();
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
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
        public EntityLocation EntityLocation => _entityLocation.Value;
        public Floor CurrentFloor => _currentFloor.Value;

        // FIELDS: --------------------------------------------------------------------------------

        public static event Action<PlayerEntity> OnPlayerSpawnedEvent = delegate { };
        public static event Action<PlayerEntity> OnPlayerDespawnedEvent = delegate { };

        public event Action<EntityLocation> OnPlayerLocationChangedEvent = delegate { };
        public event Action<float> OnSanityChangedEvent = delegate { };
        public event Action OnLeftMobileHQSeat = delegate { };
        public event Action OnDiedEvent = delegate { };
        public event Action OnRevivedEvent = delegate { };

        private static readonly Dictionary<ulong, PlayerEntity> AllPlayers = new();

        private readonly NetworkVariable<EntityLocation> _entityLocation = new(writePerm: Constants.OwnerPermission);
        private readonly NetworkVariable<Floor> _currentFloor = new(writePerm: Constants.OwnerPermission);
        private readonly NetworkVariable<float> _sanity = new(writePerm: Constants.OwnerPermission);
        private readonly NetworkVariable<int> _currentSelectedSlotIndex = new(writePerm: Constants.OwnerPermission);
        private readonly NetworkVariable<bool> _isDead = new(writePerm: Constants.OwnerPermission);

        private static PlayerEntity _localPlayer;

        private IItemsPreviewFactory _itemsPreviewFactory;
        private IPlayerInteractionObserver _playerInteractionObserver;
        private IGameManagerDecorator _gameManagerDecorator;
        private ICamerasManager _camerasManager;
        private IMobileHeadquartersEntity _mobileHeadquartersEntity;

        private PlayerConfigMeta _playerConfig;
        private StateMachine _playerStateMachine;
        private PlayerInventoryManager _inventoryManager;
        private PlayerInventory _inventory;
        private InteractionChecker _interactionChecker;
        private InteractionHandler _interactionHandler;
        private HealthSystem _healthSystem;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void TakeDamage(float damage, IEntity source = null) =>
            _healthSystem.TakeDamage(damage);

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            _references.NetworkTransform.InLocalSpace = false;
            _references.NetworkTransform.Teleport(position, rotation, transform.localScale);
        }

        public void SetEntityLocation(EntityLocation entityLocation)
        {
            if (IsOwner)
                _entityLocation.Value = entityLocation;
            else
                SetPlayerLocationServerRpc(entityLocation);
        }

        public void SetFloor(Floor floor)
        {
            if (IsOwner)
                _currentFloor.Value = floor;
            else
                SetFloorServerRpc(floor);
        }

        public void DropItem(bool destroy = false)
        {
            if (IsDead())
                return;

            _inventory.DropItem(destroy);
        }

        public void Kill(PlayerDeathReason deathReason)
        {
            if (!IsOwner)
                return;

            EnterDeathState();
        }

        public void ToggleDead(bool isDead)
        {
            if (_isDead.Value == isDead)
                return;

            _isDead.Value = isDead;
        }

        public void SendLeftMobileHQSeat() =>
            OnLeftMobileHQSeat.Invoke();

        public void EnterAliveState() => ChangeState<AliveState>();

        public void EnterReviveState() => ChangeState<ReviveState>();
        
        public void EnterSittingState() => ChangeState<SittingState>();

        public static IReadOnlyDictionary<ulong, PlayerEntity> GetAllPlayers() => AllPlayers;

        public static PlayerEntity GetLocalPlayer() => _localPlayer;

        public MonoBehaviour GetMonoBehaviour() => this;
        
        public Transform GetTransform() => transform;

        public PlayerInventory GetInventory() => _inventory;

        public float GetSanity() =>
            _sanity.Value;

        public static bool TryGetPlayer(ulong clientID, out PlayerEntity playerEntity) =>
            AllPlayers.TryGetValue(clientID, out playerEntity);

        public bool TrySetMobileHQAsParent()
        {
            Transform newParent = _mobileHeadquartersEntity.GetTransform();
            Transform parent = transform.parent;
            bool alreadyParented = parent != null && parent == newParent;

            if (alreadyParented)
                return true;
            
            return NetworkObject.TrySetParent(newParent);
        }

        public bool TryRemoveParent() =>
            NetworkObject.TryRemoveParent();

        public bool IsDead() =>
            _isDead.Value;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void InitAll()
        {
            AllPlayers.TryAdd(OwnerClientId, this);

            _inventory = new PlayerInventory();

            _inventoryManager = new PlayerInventoryManager(playerEntity: this, _itemsPreviewFactory);

            _sanity.OnValueChanged += OnSanityChanged;
            _isDead.OnValueChanged += OnDead;
        }

        protected override void InitOwner()
        {
            PlayerCamera playerCamera = _camerasManager.GetPlayerCamera();
            CameraReferences cameraReferences = playerCamera.CameraReferences;
            Camera mainCamera = cameraReferences.MainCamera;

            Other();
            InitSystems();
            InitPlayerMovement();
            DeactivatePlayerMesh();
            SetupStates();
            EnterSittingState();

            InputReader.OnScrollEvent += OnScrollInventory;
            InputReader.OnInteractEvent += OnInteract;
            InputReader.OnDropItemEvent += OnDropItem;

            _entityLocation.OnValueChanged += OnOwnerPlayerLocationChanged;

            _inventory.OnSelectedSlotChangedEvent += OnOwnerSelectedSlotChanged;

            _healthSystem.OnHealthChangedEvent += OnHealthChanged;

            // LOCAL METHODS: -----------------------------

            void Other()
            {
                _localPlayer = this;
            }

            void InitSystems()
            {
                _healthSystem = _references.HealthSystem;
                float health = _playerConfig.Health;
                _healthSystem.Setup(health);

                _playerStateMachine = new StateMachine();

                _interactionChecker = new InteractionChecker(_playerInteractionObserver, transform,
                    mainCamera, _interactionMaxDistance, interactionLM: _interactionLM, _interactionObstaclesLM);

                _interactionHandler = new InteractionHandler(playerEntity: this, _inventoryManager,
                    _playerInteractionObserver);
            }

            void InitPlayerMovement()
            {
                Character character = _references.Character;
                character.camera = mainCamera;

                PlayerMovementController playerMovementController = _references.PlayerMovementController;
                playerMovementController.Setup(playerEntity: this);

                MyAnimationController animationController = _references.AnimationController;
                animationController.Setup(character, InputReader, cameraReferences);

                SittingCameraController sittingCameraController = _references.SittingCameraController;
                sittingCameraController.ToggleActiveState(isEnabled: false);
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

            void SetupStates()
            {
                AliveState aliveState = new(_camerasManager);
                DeathState deathState = new(playerEntity: this, _camerasManager);
                ReviveState reviveState = new(playerEntity: this);
                SittingState sittingState = new(playerEntity: this, _gameManagerDecorator, _camerasManager);

                _playerStateMachine.AddState(aliveState);
                _playerStateMachine.AddState(deathState);
                _playerStateMachine.AddState(reviveState);
                _playerStateMachine.AddState(sittingState);
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

        protected override void TickOwner() =>
            _interactionChecker.Check();

        protected override void DespawnAll()
        {
            if (!IsServerOnly)
            {
                _inventory.DropAllItems();
                _inventoryManager.Dispose();
            }

            AllPlayers.Remove(OwnerClientId);

            _sanity.OnValueChanged -= OnSanityChanged;
            _isDead.OnValueChanged -= OnDead;
        }

        protected override void DespawnOwner()
        {
            _interactionHandler.Dispose();

            InputReader.OnScrollEvent -= OnScrollInventory;
            InputReader.OnInteractEvent -= OnInteract;
            InputReader.OnDropItemEvent -= OnDropItem;

            _entityLocation.OnValueChanged -= OnOwnerPlayerLocationChanged;

            _inventory.OnSelectedSlotChangedEvent -= OnOwnerSelectedSlotChanged;

            _healthSystem.OnHealthChangedEvent -= OnHealthChanged;
        }

        protected override void DespawnNotOwner() =>
            _currentSelectedSlotIndex.OnValueChanged -= OnNotOwnerSelectedSlotChanged;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CheckDeadStatus(HealthData healthData)
        {
            if (IsDead())
                return;

            float currentHealth = healthData.CurrentHealth;
            bool isDead = Mathf.Approximately(a: currentHealth, b: 0f);

            if (!isDead)
                return;

            bool isStateFound = _playerStateMachine.TryGetCurrentState(out IState state);
            bool isStateValid = isStateFound && state.GetType() != typeof(DeathState);

            if (!isStateValid)
                return;

            EnterDeathState();
        }

        private void EnterDeathState() => ChangeState<DeathState>();

        private void ChangeState<TState>() where TState : IState =>
            _playerStateMachine.ChangeState<TState>();

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
        public void ToggleRagdollServerRpc(bool enable) => ToggleRagdollClientRpc(enable);

        [ServerRpc(RequireOwnership = false)]
        private void SetPlayerLocationServerRpc(EntityLocation entityLocation) =>
            _entityLocation.Value = entityLocation;

        [ServerRpc(RequireOwnership = false)]
        private void SetFloorServerRpc(Floor floor) =>
            _currentFloor.Value = floor;

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

        [ClientRpc]
        private void ToggleRagdollClientRpc(bool enable)
        {
            CapsuleCollider capsuleCollider = _references.Collider;
            capsuleCollider.isTrigger = enable;

            RagdollController ragdollController = _references.RagdollController;
            ragdollController.ToggleRagdoll(enable);
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
            if (IsDead())
                return;

            bool switchToNextSlot = scrollValue <= 0;

            if (switchToNextSlot)
                _inventory.SwitchToNextSlot();
            else
                _inventory.SwitchToPreviousSlot();
        }

        private void OnInteract()
        {
            if (IsDead())
                return;

            _interactionHandler.Interact();
        }

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

        private void OnOwnerPlayerLocationChanged(EntityLocation previousValue, EntityLocation newValue)
        {
            string log = Log.HandleLog($"New Location: <gb>{newValue}</gb>");
            Debug.Log(log);
            OnPlayerLocationChangedEvent.Invoke(newValue);
        }

        private void OnSanityChanged(float previousValue, float newValue) =>
            OnSanityChangedEvent.Invoke(newValue);

        private void OnHealthChanged(HealthData healthData) => CheckDeadStatus(healthData);

        private void OnDead(bool previousValue, bool newValue)
        {
            if (newValue)
                OnDiedEvent.Invoke();
            else
                OnRevivedEvent.Invoke();
        }

        // DEBUG BUTTONS: -------------------------------------------------------------------------

        [Title(Constants.DebugButtons)]
        [Button(buttonSize: 30), DisableInEditorMode]
        private void DebugKill() => Kill(PlayerDeathReason._);

        [Button(buttonSize: 30), DisableInEditorMode]
        private void DebugEnterAliveState() => EnterAliveState();

        [Button(buttonSize: 30), DisableInEditorMode]
        private void DebugEnterDeathState() => EnterDeathState();

        [Button(buttonSize: 30), DisableInEditorMode]
        private void DebugEnterReviveState() => EnterReviveState();

        [Button(buttonSize: 30), DisableInEditorMode]
        private void DebugEnterSittingState() => EnterSittingState();

        [Button(buttonSize: 30), DisableInEditorMode]
        private void DebugSetCameraStatus(CameraStatus cameraStatus) =>
            _camerasManager.SetCameraStatus(cameraStatus);
    }
}