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
using GameCore.Gameplay.Systems.Footsteps;
using GameCore.Gameplay.Systems.Health;
using GameCore.Gameplay.Systems.InputManagement;
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

        [BoxGroup(Constants.References, showLabel: false), SerializeField]
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
        public event Action<PlayerEntity> OnRevivedEvent = delegate { };
        public event Action<bool> OnParentChangedEvent = delegate { };

        public event Func<bool> IsCrouching = () => false;
        public event Func<bool> IsSprinting = () => false;

        private const float MinVelocityForNonAfk = 0.015f;

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

        public void TakeDamage(float damage) => TakeDamageRpc(damage);

        public void Teleport(Vector3 position, Quaternion rotation, bool resetVelocity = false)
        {
            if (resetVelocity)
                _references.Rigidbody.velocity = Vector3.zero;

            _references.NetworkTransform.InLocalSpace = false;
            _references.NetworkTransform.Teleport(position, rotation, transform.localScale);
        }

        public void TeleportToTrainSeat(int seatIndex) => TeleportToTrainSeatRpc(seatIndex);

        public void SetEntityLocation(EntityLocation entityLocation) => SetEntityLocationRpc(entityLocation);

        public void SetFloor(Floor floor)
        {
            if (IsOwner)
                _currentFloor.Value = floor;
            else
                SetFloorRpc(floor);
        }

        public void DropItem(bool destroy = false)
        {
            if (IsDead())
                return;

            bool isItemDropped = _inventory.DropItem(destroy);

            if (!isItemDropped)
                return;

            PlaySound(SFXType.ItemDrop, onlyLocal: true).Forget();
        }

        public void Kill(PlayerDeathReason deathReason) => KillRpc(deathReason);

        public void Revive() => ReviveRpc();

        public void ToggleDead(bool isDead)
        {
            if (_isDead.Value == isDead)
                return;

            _isDead.Value = isDead;
        }

        public void ToggleInsideTrainState(bool isInsideTrain) =>
            IsInsideTrain = isInsideTrain;

        [Button]
        public void SetTrainAsParent() => SetTrainAsParentRpc();

        [Button]
        public void RemoveParent() => RemoveParentRpc();

        public void SendLeftMobileHQSeat() =>
            OnLeftMobileHQSeat.Invoke();

        public void EnterAliveState() => ChangeState<AliveState>();
        public void EnterSittingState() => ChangeState<SittingState>();

        public static IReadOnlyDictionary<ulong, PlayerEntity> GetAllPlayers() => AllPlayers;

        public static PlayerEntity GetLocalPlayer() => _localPlayer;

        public PlayerConfigMeta GetConfig() => _playerConfig;

        public PlayerReferences GetReferences() => _references;

        public PlayerInventory GetInventory() => _inventory;

        public float GetSanity() =>
            _sanity.Value;

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

        public static bool TryGetPlayer(ulong clientID, out PlayerEntity playerEntity) =>
            AllPlayers.TryGetValue(clientID, out playerEntity);

        public bool IsDead() =>
            _isDead.Value;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void InitAll()
        {
            AllPlayers.TryAdd(OwnerClientId, this);

            InitSystems();

            _sanity.OnValueChanged += OnSanityChanged;
            _isDead.OnValueChanged += OnDead;

            // LOCAL METHODS: -----------------------------

            void InitSystems()
            {
                SoundReproducer = new PlayerSoundReproducer(soundProducer: this, _playerConfig);
                _inventory = new PlayerInventory();
                _inventoryManager = new PlayerInventoryManager(playerEntity: this, _itemsPreviewFactory);
            }
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
            
            _references.RigController.SetAnimator(_references.Animator);

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
            bool isMoving = distanceFromLastFrame > MinVelocityForNonAfk;

            if (isMoving)
                TimeSinceAfk = 0f;
            else
                TimeSinceAfk += Time.deltaTime;
                
            _lastFramePosition = currentPosition;
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

        private void EnterReviveState() => ChangeState<ReviveState>();
        private void EnterDeathState() => ChangeState<DeathState>();

        private void ChangeState<TState>() where TState : IState =>
            _playerStateMachine.ChangeState<TState>();

        // RPC: -----------------------------------------------------------------------------------

        [Rpc(target: SendTo.Everyone)]
        public void ToggleRagdollRpc(bool enable)
        {
            CapsuleCollider capsuleCollider = _references.Collider;
            capsuleCollider.isTrigger = enable;

            RagdollController ragdollController = _references.RagdollController;
            ragdollController.ToggleRagdoll(enable);
        }

        [Rpc(target: SendTo.Server)]
        private void SetTrainAsParentRpc()
        {
            Transform newParent = _trainEntity.GetTransform();
            Transform parent = transform.parent;
            bool alreadyParented = parent != null && parent == newParent;

            if (alreadyParented)
                return;

            NetworkObject.TrySetParent(newParent);
        }

        [Rpc(target: SendTo.Server)]
        private void RemoveParentRpc() =>
            NetworkObject.TryRemoveParent();

        [Rpc(target: SendTo.Owner)]
        private void TakeDamageRpc(float damage) =>
            _healthSystem.TakeDamage(damage);

        [Rpc(target: SendTo.Owner)]
        private void KillRpc(PlayerDeathReason deathReason) => EnterDeathState();

        [Rpc(target: SendTo.Owner)]
        private void ReviveRpc() => EnterReviveState();
        
        [Rpc(target: SendTo.Owner)]
        private void ChangeSelectedSlotRpc(int slotIndex) =>
            _currentSelectedSlotIndex.Value = slotIndex;

        [Rpc(target: SendTo.Owner)]
        private void SetEntityLocationRpc(EntityLocation entityLocation) =>
            _entityLocation.Value = entityLocation;

        [Rpc(target: SendTo.Owner)]
        private void TeleportToTrainSeatRpc(int seatIndex) =>
            _trainEntity.TeleportLocalPlayerToTrainSeat(seatIndex);

        [Rpc(target: SendTo.Owner)]
        private void SetFloorRpc(Floor floor) =>
            _currentFloor.Value = floor;

        [Rpc(target: SendTo.NotOwner)]
        public void CreateItemPreviewRpc(int slotIndex, int itemID) =>
            _inventoryManager.CreateItemPreviewClientSide(slotIndex, itemID, isFirstPerson: false);

        [Rpc(target: SendTo.NotOwner)]
        public void DestroyItemPreviewRpc(int slotIndex) =>
            _inventoryManager.DestroyItemPreview(slotIndex);

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

        private void OnItemEquipped(EquippedItemStaticData data) => PlaySound(SFXType.ItemPickup).Forget();

        private void OnOwnerSelectedSlotChanged(ChangedSlotStaticData data)
        {
            int slotIndex = data.SlotIndex;

            if (IsOwner)
                _currentSelectedSlotIndex.Value = slotIndex;
            else
                ChangeSelectedSlotRpc(slotIndex);

            PlaySound(SFXType.ItemSwitch, onlyLocal: true).Forget();
        }

        private void OnNotOwnerSelectedSlotChanged(int previousValue, int newValue)
        {
            _inventory.SetSelectedSlotIndex(newValue);
            _inventoryManager.ToggleItemsState();
            
            _references.RigController.TryUpdateRig(OwnerClientId);
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
                OnRevivedEvent.Invoke(this);
        }

        private void OnFootstepPerformed(string colliderTag)
        {
            MakeFootstepsNoise();

            if (!IsCrouching.Invoke())
                PlaySound(SFXType.Footsteps).Forget();
        }

        private void OnJumped() => PlaySound(SFXType.Jump).Forget();

        private void OnLanded(Vector3 landingVelocity)
        {
            const float minVelocity = 4.0f;
            float velocity = landingVelocity.magnitude;
            bool ignore = velocity <= minVelocity;

            if (ignore)
                return;

            PlaySound(SFXType.Land).Forget();
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