using System;
using System.Collections.Generic;
using ECM2;
using GameCore.Configs.Gameplay.Player;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.CamerasManagement;
using GameCore.Gameplay.Entities.Player.CameraManagement;
using GameCore.Gameplay.Entities.Player.Interaction;
using GameCore.Gameplay.Entities.Player.States;
using GameCore.Gameplay.Entities.Train;
using GameCore.Gameplay.Factories.ItemsPreview;
using GameCore.Gameplay.InputManagement;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Systems.Footsteps;
using GameCore.Gameplay.Systems.Health;
using GameCore.Gameplay.Systems.Inventory;
using GameCore.Gameplay.Systems.Noise;
using GameCore.Gameplay.Systems.Ragdoll;
using GameCore.Gameplay.Systems.SoundReproducer;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using GameCore.Infrastructure.Providers.Global;
using GameCore.Infrastructure.StateMachine;
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
    [GenerateSerializationForType(typeof(SFXType))]
    public class PlayerEntity : SoundProducerEntity<PlayerEntity.SFXType>, ITeleportableEntity, IDamageable
    {
        public enum SFXType
        {
            //_ = 0,
            Footsteps = 1,
            Jump = 2,
            Land = 3,
            ItemPickup = 4,
            ItemDrop = 5,
            ItemSwitch = 6
        }

        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(
            IItemsPreviewFactory itemsPreviewFactory,
            IPlayerInteractionObserver playerInteractionObserver,
            ICamerasManager camerasManager,
            ITrainEntity trainEntity,
            IGameplayConfigsProvider gameplayConfigsProvider,
            IConfigsProvider configsProvider
        )
        {
            _itemsPreviewFactory = itemsPreviewFactory;
            _playerInteractionObserver = playerInteractionObserver;
            _camerasManager = camerasManager;
            _trainEntity = trainEntity;

            _playerConfig = gameplayConfigsProvider.GetConfig<PlayerConfigMeta>();
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
        public float TimeSinceAfk { get; private set; }
        public bool IsInsideTrain { get; private set; } = true;

        // FIELDS: --------------------------------------------------------------------------------

        public static event Action<PlayerEntity> OnPlayerSpawnedEvent = delegate { };
        public static event Action<PlayerEntity> OnPlayerDespawnedEvent = delegate { };

        public event Action<EntityLocation> OnPlayerLocationChangedEvent = delegate { };
        public event Action<float> OnSanityChangedEvent = delegate { };
        public event Action OnLeftMobileHQSeat = delegate { };
        public event Action OnDeathEvent = delegate { };
        public event Action OnRevivedEvent = delegate { };
        public event Action<bool> OnParentChangedEvent = delegate { };

        public event Func<bool> IsCrouching = () => false;
        public event Func<bool> IsSprinting = () => false;

        private const float MinVelocityForNonAfk = 0.25f;

        private static readonly Dictionary<ulong, PlayerEntity> AllPlayers = new();

        private readonly NetworkVariable<EntityLocation> _entityLocation = new(writePerm: Constants.OwnerPermission);
        private readonly NetworkVariable<Floor> _currentFloor = new(writePerm: Constants.OwnerPermission);
        private readonly NetworkVariable<float> _sanity = new(writePerm: Constants.OwnerPermission);
        private readonly NetworkVariable<int> _currentSelectedSlotIndex = new(writePerm: Constants.OwnerPermission);
        private readonly NetworkVariable<bool> _isDead = new(writePerm: Constants.OwnerPermission);

        private static PlayerEntity _localPlayer;

        private IItemsPreviewFactory _itemsPreviewFactory;
        private IPlayerInteractionObserver _playerInteractionObserver;
        private ICamerasManager _camerasManager;
        private ITrainEntity _trainEntity;

        private PlayerConfigMeta _playerConfig;
        private StateMachineBase _playerStateMachine;
        private PlayerInventoryManager _inventoryManager;
        private PlayerInventory _inventory;
        private InteractionChecker _interactionChecker;
        private InteractionHandler _interactionHandler;
        private HealthSystem _healthSystem;

        private Vector3 _lastFramePosition;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void TakeDamage(float damage, IEntity source = null) =>
            _healthSystem.TakeDamage(damage);

        public void Teleport(Vector3 position, Quaternion rotation, bool resetVelocity = false)
        {
            if (resetVelocity)
                _references.Rigidbody.velocity = Vector3.zero;

            _references.NetworkTransform.InLocalSpace = false;
            _references.NetworkTransform.Teleport(position, rotation, transform.localScale);
        }

        [Button(ButtonStyle.FoldoutButton)]
        public void TeleportToTrainSeat(int seatIndex)
        {
            if (IsOwner)
                TeleportToTrainSeatLocal(seatIndex);
            else
                TeleportToTrainSeatServerRPC(seatIndex);
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

            bool isItemDropped = _inventory.DropItem(destroy);

            if (!isItemDropped)
                return;

            PlaySound(SFXType.ItemDrop, onlyLocal: true);
        }

        public void Kill(PlayerDeathReason deathReason)
        {
            if (IsOwner)
                KillClientRpc(deathReason);
            else
                KillServerRpc(deathReason);
        }

        public void ToggleDead(bool isDead)
        {
            if (_isDead.Value == isDead)
                return;

            _isDead.Value = isDead;
        }

        public void ToggleInsideTrainState(bool isInsideTrain) =>
            IsInsideTrain = isInsideTrain;

        public void SendLeftMobileHQSeat() =>
            OnLeftMobileHQSeat.Invoke();

        public void EnterAliveState() => ChangeState<AliveState>();

        public void EnterReviveState() => ChangeState<ReviveState>();

        public void EnterSittingState() => ChangeState<SittingState>();

        public static IReadOnlyDictionary<ulong, PlayerEntity> GetAllPlayers() => AllPlayers;

        public static PlayerEntity GetLocalPlayer() => _localPlayer;

        public static int GetPlayersAmount() =>
            AllPlayers.Count;

        public static int GetAlivePlayersAmount()
        {
            int alivePlayersAmount = 0;

            foreach (PlayerEntity playerEntity in AllPlayers.Values)
            {
                bool isDead = playerEntity.IsDead();

                if (isDead)
                    continue;

                alivePlayersAmount += 1;
            }

            return alivePlayersAmount;
        }

        public PlayerInventory GetInventory() => _inventory;

        public float GetSanity() =>
            _sanity.Value;

        public static bool TryGetPlayer(ulong clientID, out PlayerEntity playerEntity) =>
            AllPlayers.TryGetValue(clientID, out playerEntity);

        [Button]
        public void SetTrainAsParent()
        {
            if (IsServerOnly)
                SetTrainAsParentLocal();
            else
                SetTrainAsParentServerRPC();
        }

        [Button]
        public void RemoveParent()
        {
            if (IsServerOnly)
                RemoveParentLocal();
            else
                RemoveParentServerRPC();
        }

        public bool IsDead() =>
            _isDead.Value;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void InitAll()
        {
            AllPlayers.TryAdd(OwnerClientId, this);

            SoundReproducer = new PlayerSoundReproducer(soundProducer: this, _playerConfig);
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
            EnterAliveState();

            InputReader.OnScrollEvent += OnScrollInventory;
            InputReader.OnInteractEvent += OnInteract;
            InputReader.OnDropItemEvent += OnDropItem;

            _entityLocation.OnValueChanged += OnPlayerLocationChanged;

            _inventory.OnItemEquippedEvent += OnItemEquipped;
            _inventory.OnSelectedSlotChangedEvent += OnOwnerSelectedSlotChanged;

            _healthSystem.OnHealthChangedEvent += OnHealthChanged;

            // LOCAL METHODS: -----------------------------

            void Other()
            {
                _localPlayer = this;
            }

            void InitSystems()
            {
                float health = _playerConfig.Health;
                _healthSystem = _references.HealthSystem;
                _healthSystem.Setup(health);

                PlayerFootstepsSystem footstepsSystem = _references.FootstepsSystem;
                footstepsSystem.Setup(playerEntity: this);
                footstepsSystem.ToggleActiveState(isActive: true);
                footstepsSystem.OnFootstepPerformedEvent += OnFootstepPerformed;

                _playerStateMachine = new StateMachineBase();

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

                character.Jumped += OnJumped;
                character.Landed += OnLanded;
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
                SittingState sittingState = new(playerEntity: this, _camerasManager);

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

        protected override void TickAll() => CheckForAfk();

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
            _localPlayer = null;
            
            _interactionHandler.Dispose();

            InputReader.OnScrollEvent -= OnScrollInventory;
            InputReader.OnInteractEvent -= OnInteract;
            InputReader.OnDropItemEvent -= OnDropItem;

            _entityLocation.OnValueChanged -= OnPlayerLocationChanged;

            _inventory.OnItemEquippedEvent -= OnItemEquipped;
            _inventory.OnSelectedSlotChangedEvent -= OnOwnerSelectedSlotChanged;

            _healthSystem.OnHealthChangedEvent -= OnHealthChanged;
        }

        protected override void DespawnNotOwner() =>
            _currentSelectedSlotIndex.OnValueChanged -= OnNotOwnerSelectedSlotChanged;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void TeleportToTrainSeatLocal(int seatIndex) =>
            _trainEntity.TeleportLocalPlayerToTrainSeat(seatIndex);

        private void SetTrainAsParentLocal()
        {
            Transform newParent = _trainEntity.GetTransform();
            Transform parent = transform.parent;
            bool alreadyParented = parent != null && parent == newParent;

            if (alreadyParented)
                return;

            NetworkObject.TrySetParent(newParent);
        }

        private void RemoveParentLocal() =>
            NetworkObject.TryRemoveParent();

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

        private void CheckForAfk()
        {
            Vector3 currentPosition = transform.position;
            float distanceFromLastFrame = Vector3.Distance(a: currentPosition, b: _lastFramePosition);

            _lastFramePosition = currentPosition;
            
            //return;
            float moveSpeed = _references.Character.GetSpeed();
            bool isMoving = moveSpeed > MinVelocityForNonAfk;

            if (isMoving)
                TimeSinceAfk = 0f;
            else
                TimeSinceAfk += Time.deltaTime;
        }

        private void MakeFootstepsNoise()
        {
            float noiseRange = 17f;
            float noiseLoudness = 0.4f;

            if (IsSprinting.Invoke())
            {
                noiseRange = 22f;
                noiseLoudness = 0.6f;
            }

            if (IsCrouching.Invoke())
            {
                noiseRange = 5f;
                noiseLoudness = 0.05f;
            }

            NoiseManager.MakeNoise(transform.position, noiseRange, noiseLoudness);
        }

#warning REPLACE

        private void MakeLandingNoise(float velocity)
        {
            const float quiteLandingVelocity = 6;
            const float loudLandingVelocity = 12;

            float noiseRange;
            float noiseLoudness;

            // Loud
            if (velocity > loudLandingVelocity)
            {
                noiseRange = 25f;
                noiseLoudness = 0.6f;
            }
            // Quite
            else if (velocity < quiteLandingVelocity)
            {
                noiseRange = 14f;
                noiseLoudness = 0.4f;
            }
            // Middle
            else
            {
                noiseRange = 19f;
                noiseLoudness = 0.5f;
            }

            NoiseManager.MakeNoise(transform.position, noiseRange, noiseLoudness);
        }

        private void EnterDeathState() => ChangeState<DeathState>();

        private void ChangeState<TState>() where TState : IState =>
            _playerStateMachine.ChangeState<TState>();

        // RPC: -----------------------------------------------------------------------------------

        [ServerRpc(RequireOwnership = false)]
        private void KillServerRpc(PlayerDeathReason deathReason) => KillClientRpc(deathReason);

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
            SetPlayerLocationClientRpc(entityLocation);

        [ServerRpc(RequireOwnership = false)]
        private void SetFloorServerRpc(Floor floor) =>
            _currentFloor.Value = floor;

        [ServerRpc(RequireOwnership = false)]
        private void TeleportToTrainSeatServerRPC(int seatIndex, ServerRpcParams serverRpcParams = default)
        {
            ulong senderClientID = serverRpcParams.Receive.SenderClientId;
            TeleportToTrainSeatClientRPC(senderClientID, seatIndex);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetTrainAsParentServerRPC() => SetTrainAsParentLocal();

        [ServerRpc(RequireOwnership = false)]
        private void RemoveParentServerRPC() => RemoveParentLocal();
        
        [ClientRpc]
        private void KillClientRpc(PlayerDeathReason deathReason) => EnterDeathState();

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

        [ClientRpc]
        private void SetPlayerLocationClientRpc(EntityLocation entityLocation)
        {
            if (!IsOwner)
                return;

            _entityLocation.Value = entityLocation;
        }

        [ClientRpc]
        private void TeleportToTrainSeatClientRPC(ulong senderClientID, int seatIndex)
        {
            bool isMatches = senderClientID == NetworkHorror.ClientID;

            if (isMatches)
                return;
            
            TeleportToTrainSeatLocal(seatIndex);
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

            OnParentChangedEvent.Invoke(hasParent);
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

        private void OnItemEquipped(EquippedItemStaticData data) => PlaySound(SFXType.ItemPickup);

        private void OnOwnerSelectedSlotChanged(ChangedSlotStaticData data)
        {
            int slotIndex = data.SlotIndex;

            if (IsOwner)
                _currentSelectedSlotIndex.Value = slotIndex;
            else
                ChangeSelectedSlotServerRpc(slotIndex);

            PlaySound(SFXType.ItemSwitch, onlyLocal: true);
        }

        private void OnNotOwnerSelectedSlotChanged(int previousValue, int newValue)
        {
            _inventory.SetSelectedSlotIndex(newValue);
            _inventoryManager.ToggleItemsState();
        }

        private void OnPlayerLocationChanged(EntityLocation previousValue, EntityLocation newValue) =>
            OnPlayerLocationChangedEvent.Invoke(newValue);

        private void OnSanityChanged(float previousValue, float newValue) =>
            OnSanityChangedEvent.Invoke(newValue);

        private void OnHealthChanged(HealthData healthData) => CheckDeadStatus(healthData);

        private void OnDead(bool previousValue, bool newValue)
        {
            if (newValue)
                OnDeathEvent.Invoke();
            else
                OnRevivedEvent.Invoke();
        }

        private void OnFootstepPerformed(string colliderTag)
        {
            MakeFootstepsNoise();

            if (!IsCrouching.Invoke())
                PlaySound(SFXType.Footsteps);
        }

        private void OnJumped() => PlaySound(SFXType.Jump);

        private void OnLanded(Vector3 landingVelocity)
        {
            const float minVelocity = 4.0f;
            float velocity = landingVelocity.magnitude;
            bool ignore = velocity <= minVelocity;

            if (ignore)
                return;

            PlaySound(SFXType.Land);
            MakeLandingNoise(velocity);
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