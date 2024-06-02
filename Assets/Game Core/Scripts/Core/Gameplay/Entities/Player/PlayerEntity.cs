using System;
using System.Collections.Generic;
using ECM2;
using GameCore.Configs.Gameplay.Player;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.CamerasManagement;
using GameCore.Gameplay.Entities.Player.CameraManagement;
using GameCore.Gameplay.Entities.Player.Interaction;
using GameCore.Gameplay.Entities.Player.States;
using GameCore.Gameplay.EntitiesSystems.Health;
using GameCore.Gameplay.EntitiesSystems.Inventory;
using GameCore.Gameplay.Factories.ItemsPreview;
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
    public class PlayerEntity : NetcodeBehaviour, IEntity, IDamageable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IItemsPreviewFactory itemsPreviewFactory, ICamerasManager camerasManager,
            IPlayerInteractionObserver playerInteractionObserver, IGameplayConfigsProvider gameplayConfigsProvider,
            IConfigsProvider configsProvider)
        {
            _itemsPreviewFactory = itemsPreviewFactory;
            _camerasManager = camerasManager;
            _playerInteractionObserver = playerInteractionObserver;

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

        // FIELDS: --------------------------------------------------------------------------------

        public static event Action<PlayerEntity> OnPlayerSpawnedEvent = delegate { };
        public static event Action<PlayerEntity> OnPlayerDespawnedEvent = delegate { };

        public event Action<EntityLocation> OnPlayerLocationChangedEvent = delegate { };
        public event Action OnDiedEvent = delegate { };
        public event Action OnRevivedEvent = delegate { };

        private const NetworkVariableWritePermission OwnerPermission = NetworkVariableWritePermission.Owner;

        private static readonly Dictionary<ulong, PlayerEntity> AllPlayers = new();

        private readonly NetworkVariable<EntityLocation> _entityLocation = new(writePerm: OwnerPermission);
        private readonly NetworkVariable<Vector3> _lookAtPosition = new(writePerm: OwnerPermission);
        private readonly NetworkVariable<int> _currentSelectedSlotIndex = new(writePerm: OwnerPermission);
        private readonly NetworkVariable<bool> _isDead = new();

        private static PlayerEntity _localPlayer;

        private IItemsPreviewFactory _itemsPreviewFactory;
        private ICamerasManager _camerasManager;
        private IPlayerInteractionObserver _playerInteractionObserver;

        private PlayerConfigMeta _playerConfig;
        private StateMachine _playerStateMachine;
        private PlayerInventoryManager _inventoryManager;
        private PlayerInventory _inventory;
        private InteractionChecker _interactionChecker;
        private InteractionHandler _interactionHandler;
        private HealthSystem _healthSystem;

        private Transform _cameraLookAtObject;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void TakeDamage(float damage, IEntity source = null) =>
            _healthSystem.TakeDamage(damage);

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            _references.NetworkTransform.InLocalSpace = false;
            _references.NetworkTransform.Teleport(position, rotation, transform.localScale);
        }

#warning ПРОВЕРИТЬ ИЛИ КОРРЕКТНО РАБОТАЕТ, ДОБАВИТЬ СЕРВЕР РПС
        public void ChangePlayerLocation(EntityLocation entityLocation)
        {
            if (!IsOwner)
                return;

            _entityLocation.Value = entityLocation;
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

        public void SendDiedEvent() =>
            OnDiedEvent.Invoke();

        public void SendRevivedEvent() =>
            OnRevivedEvent.Invoke();
        
        public void EnterAliveState() => ChangeState<AliveState>();

        public void EnterReviveState()
        {
            _isDead.Value = false;

            ChangeState<ReviveState>();
        }

        public static IReadOnlyDictionary<ulong, PlayerEntity> GetAllPlayers() => AllPlayers;

        public static PlayerEntity GetLocalPlayer() => _localPlayer;
        public Transform GetTransform() => transform;

        public PlayerInventory GetInventory() => _inventory;
        
        public EntityLocation GeEntityLocation() =>
            _entityLocation.Value;

        public static bool TryGetPlayer(ulong clientID, out PlayerEntity playerEntity) =>
            AllPlayers.TryGetValue(clientID, out playerEntity);

        public bool IsDead() =>
            _isDead.Value;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void InitAll()
        {
            AllPlayers.TryAdd(OwnerClientId, this);

            _inventory = new PlayerInventory();

            _inventoryManager = new PlayerInventoryManager(playerEntity: this, _itemsPreviewFactory);
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
            EnterAliveState();

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
                _cameraLookAtObject = cameraReferences.LookAtObject;
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
                animationController.Setup(character, cameraReferences);
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
                AliveState aliveState = new();
                DeathState deathState = new(playerEntity: this, _camerasManager);
                ReviveState reviveState = new(playerEntity: this, _camerasManager);

                _playerStateMachine.AddState(aliveState);
                _playerStateMachine.AddState(deathState);
                _playerStateMachine.AddState(reviveState);
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

            _isDead.Value = true;

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

        private void OnOwnerPlayerLocationChanged(EntityLocation previousValue, EntityLocation newValue) =>
            OnPlayerLocationChangedEvent.Invoke(newValue);

        private void OnHealthChanged(HealthData healthData) => CheckDeadStatus(healthData);

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
    }
}