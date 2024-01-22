using Cinemachine;
using GameCore.Configs.Gameplay.MobileHeadquarters;
using GameCore.Enums;
using GameCore.Gameplay.Interactable.MobileHeadquarters;
using GameCore.Gameplay.Locations;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Other;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.Entities.MobileHeadquarters
{
    public class MobileHeadquartersEntity : NetworkBehaviour, IMobileHeadquartersEntity
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField, Required]
        private MobileHeadquartersConfigMeta _mobileHeadquartersConfig;

        [Title(Constants.References)]
        [SerializeField, Required]
        private Animator _animator;

        [SerializeField, Required]
        private AnimationObserver _animationObserver;

        [SerializeField, Required]
        private CinemachineDollyCart _dollyCart;

        [SerializeField, Required]
        private NetworkObject _networkObject;

        [SerializeField, Required]
        private ToggleMobileDoorLever _toggleMobileDoorLever;

        [SerializeField, Required]
        private LoadLocationLever _loadLocationLever;

        [SerializeField, Required]
        private LeaveLocationLever _leaveLocationLever;

        // FIELDS: --------------------------------------------------------------------------------

        private static MobileHeadquartersEntity _instance;

        private readonly NetworkVariable<float> _pathPosition = new();

        private RigidbodyPathMovement _pathMovement;
        private RpcCaller _rpcCaller;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _instance = this;
            
            _pathMovement = new RigidbodyPathMovement(mobileHeadquartersEntity: this, _animator, _dollyCart,
                _mobileHeadquartersConfig);

            _pathMovement.OnDestinationReachedEvent += OnDestinationReached;

            _animationObserver.OnDoorOpenedEvent += OnDoorOpened;
            _animationObserver.OnDoorClosedEvent += OnDoorClosed;

            _toggleMobileDoorLever.OnInteractEvent += OnInteractWithMobileDoorLever;
            _toggleMobileDoorLever.OnEnabledEvent += OnDoorLeverEnabled;
            _toggleMobileDoorLever.OnDisabledEvent += OnDoorLeverDisabled;

            _loadLocationLever.OnInteractEvent += OnInteractWithLoadLocationLever;
            _loadLocationLever.OnEnabledEvent += OnLoadLocation;

            _leaveLocationLever.OnInteractEvent += OnInteractWithLeaveLocationLever;
            _leaveLocationLever.OnEnabledEvent += OnLeaveLocation;
        }
        
        public override void OnNetworkSpawn()
        {
            Init();
            base.OnNetworkSpawn();
        }

        public override void OnNetworkDespawn()
        {
            Despawn();
            base.OnNetworkDespawn();
        }

        public override void OnDestroy()
        {
            _pathMovement.OnDestinationReachedEvent -= OnDestinationReached;

            _animationObserver.OnDoorOpenedEvent -= OnDoorOpened;
            _animationObserver.OnDoorClosedEvent -= OnDoorClosed;

            _toggleMobileDoorLever.OnInteractEvent -= OnInteractWithMobileDoorLever;
            _toggleMobileDoorLever.OnEnabledEvent -= OnDoorLeverEnabled;
            _toggleMobileDoorLever.OnDisabledEvent -= OnDoorLeverDisabled;

            _loadLocationLever.OnInteractEvent -= OnInteractWithLoadLocationLever;
            _loadLocationLever.OnEnabledEvent -= OnLoadLocation;

            _leaveLocationLever.OnInteractEvent -= OnInteractWithLeaveLocationLever;
            _leaveLocationLever.OnEnabledEvent -= OnLeaveLocation;

            base.OnDestroy();
        }

        private void FixedUpdate()
        {
            if (IsOwner)
                _pathMovement.Movement();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void ArriveAtRoadLocation()
        {
            RoadLocationManager roadLocationManager = RoadLocationManager.Get();
            CinemachinePath path = roadLocationManager.GetPath();
            
            ChangePath(path);
        }

        public void LeftLocation()
        {
            _loadLocationLever.InteractWithoutEvents(isEnabled: false);
            _loadLocationLever.ToggleInteract(canInteract: true);
        }

        public void SetPathPosition(float position)
        {
            if (!IsServer)
                return;

            _pathPosition.Value = position;
        }

        public Transform GetTransform() => transform;

        public NetworkObject GetNetworkObject() => _networkObject;

        public Vector3 GetVelocity()
        {
            Vector3 direction = transform.forward * _dollyCart.m_Speed;
            return direction;
        }

        public float GetPathPosition() =>
            _pathPosition.Value;

        public static MobileHeadquartersEntity Get() => _instance;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void Init()
        {
            InitClientAndServer();
            InitServer();
            InitClient();
        }

        private void InitClientAndServer()
        {
            _rpcCaller = RpcCaller.Get();

            _rpcCaller.OnRoadLocationLoadedEvent += OnRoadLocationLoaded;
            _rpcCaller.OnLocationLoadedEvent += OnLocationLoaded;
        }
        
        private void InitServer()
        {
            if (!IsServer)
                return;
        }

        private void InitClient()
        {
            if (IsServer)
                return;
            
            _pathPosition.OnValueChanged += OnPathPositionChanged;
        }

        private void Despawn()
        {
            DespawnServerAndClient();
            DespawnServer();
            DespawnClient();
        }

        private void DespawnServerAndClient()
        {
            _rpcCaller.OnRoadLocationLoadedEvent -= OnRoadLocationLoaded;
            _rpcCaller.OnLocationLoadedEvent -= OnLocationLoaded;
        }

        private void DespawnServer()
        {
            if (!IsServer)
                return;
        }

        private void DespawnClient()
        {
            if (IsServer)
                return;
            
            _pathPosition.OnValueChanged -= OnPathPositionChanged;
        }

        private void ChangePath(CinemachinePath path) =>
            _pathMovement.ChangePath(path);
        
        [Button]
        private void ToggleMovement(bool canMove) =>
            _pathMovement.ToggleMovement(canMove);

        private void ToggleDoorState(bool isOpen) =>
            _animator.SetBool(id: AnimatorHashes.IsOpen, isOpen);

        private void EnableDoorLever() =>
            _toggleMobileDoorLever.ToggleInteract(canInteract: true);

        // RPC: -----------------------------------------------------------------------------------
        
        [ServerRpc(RequireOwnership = false)]
        private void InteractWithMobileDoorLeverServerRpc() =>
            InteractWithMobileDoorLeverClientRpc();

        [ClientRpc]
        private void InteractWithMobileDoorLeverClientRpc() =>
            _toggleMobileDoorLever.InteractLogic();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnPathPositionChanged(float previousValue, float newValue) =>
            _pathMovement.SetPosition(newValue);

        [Button]
        private void OnDestinationReached()
        {
            _leaveLocationLever.InteractWithoutEvents(isEnabled: false);
            _leaveLocationLever.ToggleInteract(canInteract: true);
        }

        private void OnDoorOpened() => EnableDoorLever();

        private void OnDoorClosed() => EnableDoorLever();

        private void OnInteractWithMobileDoorLever() => InteractWithMobileDoorLeverServerRpc();

        private void OnDoorLeverEnabled() => ToggleDoorState(isOpen: true);

        private void OnDoorLeverDisabled() => ToggleDoorState(isOpen: false);

        private void OnInteractWithLoadLocationLever() =>
            _loadLocationLever.InteractLogic();

        private void OnLoadLocation() =>
            _rpcCaller.LoadLocation(SceneName.Desert);

        private void OnInteractWithLeaveLocationLever() =>
            _leaveLocationLever.InteractLogic();

        private void OnLeaveLocation()
        {
            ToggleMovement(canMove: false);
            _rpcCaller.LeaveLocation();
        }

        private void OnRoadLocationLoaded()
        {
            
        }
        
        private void OnLocationLoaded()
        {
            LocationManager locationManager = LocationManager.Get();
            CinemachinePath path = locationManager.GetPath();

            ChangePath(path);
            OnDestinationReached(); // TEMP
            ToggleMovement(canMove: true);
        }
    }
}