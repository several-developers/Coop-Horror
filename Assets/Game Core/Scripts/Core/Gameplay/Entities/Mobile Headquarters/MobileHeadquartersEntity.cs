using Cinemachine;
using GameCore.Configs.Gameplay.MobileHeadquarters;
using GameCore.Enums.Global;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Interactable.MobileHeadquarters;
using GameCore.Gameplay.Levels.Locations;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Network.Utilities;
using GameCore.Gameplay.Other;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.Entities.MobileHeadquarters
{
    public class MobileHeadquartersEntity : NetworkBehaviour, IMobileHeadquartersEntity, INetcodeBehaviour
    {
        private enum State
        {
            _ = 0,

            MovingOnRoad = 1,
            IdleOnRoad = 2,
            IdleOnLocation = 3,
            ArrivingAtLocation = 4,
            LeavingLocation = 5,
        }

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
        private ToggleMobileDoorLever _toggleMobileDoorLever;

        [SerializeField, Required]
        private LoadLocationLever _loadLocationLever;

        [SerializeField, Required]
        private LeaveLocationLever _leaveLocationLever;

        // PROPERTIES: ----------------------------------------------------------------------------

        public RpcCaller RpcCaller => _rpcCaller;

        // FIELDS: --------------------------------------------------------------------------------

        private static MobileHeadquartersEntity _instance;

        private RigidbodyPathMovement _pathMovement;
        private RpcCaller _rpcCaller;
        private State _currentState = State.MovingOnRoad;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _instance = this;

            _pathMovement = new RigidbodyPathMovement(mobileHeadquartersEntity: this, _animator,
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
            base.OnNetworkSpawn();
            InitServerAndClient();
            InitServer();
            InitClient();
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            DespawnServerAndClient();
            DespawnServer();
            DespawnClient();
        }

        private void Update()
        {
            UpdateServerAndClient();
            UpdateServer();
            UpdateClient();
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

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void InitServerAndClient()
        {
            ArrivedAtRoadLocation();
            
            _rpcCaller = RpcCaller.Get();

            _rpcCaller.OnLocationLoadedEvent += OnLocationLoaded;
            _rpcCaller.OnLeavingLocationEvent += OnLeavingLocation;
            _rpcCaller.OnLocationLeftEvent += OnLocationLeft;
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
        }
        
        public void UpdateServerAndClient()
        {
        }

        public void UpdateServer()
        {
            if (!IsOwner)
                return;
            
            _pathMovement.Movement();
        }

        public void UpdateClient()
        {
            if (IsOwner)
                return;
        }
        
        public void DespawnServerAndClient()
        {
            _rpcCaller.OnLocationLoadedEvent -= OnLocationLoaded;
            _rpcCaller.OnLeavingLocationEvent -= OnLeavingLocation;
            _rpcCaller.OnLocationLeftEvent -= OnLocationLeft;
        }

        public void DespawnServer()
        {
            if (!IsOwner)
                return;
        }

        public void DespawnClient()
        {
            if (IsOwner)
                return;
        }

        public Transform GetTransform() => transform;
        
        public Vector3 GetVelocity()
        {
            Vector3 direction = transform.forward;
            return direction;
        }

        public static MobileHeadquartersEntity Get() => _instance;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ArrivedAtRoadLocation()
        {
            RoadLocationManager roadLocationManager = RoadLocationManager.Get();
            CinemachinePath path = roadLocationManager.GetPath();

            ChangePath(path);

            _loadLocationLever.InteractWithoutEvents(isLeverPulled: false);
            _loadLocationLever.ToggleInteract(canInteract: true);
        }
        
        private void ChangePath(CinemachinePath path)
        {
            _pathMovement.ChangePath(path);
        }

        private void ToggleMovement(bool canMove) =>
            _pathMovement.ToggleMovement(canMove);

        private void ToggleDoorState(bool isOpen) =>
            _animator.SetBool(id: AnimatorHashes.IsOpen, isOpen);

        private void EnableDoorLever() =>
            _toggleMobileDoorLever.ToggleInteract(canInteract: true);

        private void EnterState(State state) =>
            _currentState = state;

        // RPC: -----------------------------------------------------------------------------------

        [ServerRpc(RequireOwnership = false)]
        private void InteractWithMobileDoorLeverServerRpc() => InteractWithMobileDoorLeverClientRpc();

        [ServerRpc(RequireOwnership = false)]
        private void InteractWithLoadLocationLeverServerRpc() => InteractWithLoadLocationLeverClientRpc();

        [ClientRpc]
        private void InteractWithMobileDoorLeverClientRpc() =>
            _toggleMobileDoorLever.InteractLogic();

        [ClientRpc]
        private void InteractWithLoadLocationLeverClientRpc() =>
            _loadLocationLever.InteractLogic();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        [Button]
        private void OnDestinationReached()
        {
            switch (_currentState)
            {
                case State.ArrivingAtLocation:
                    ToggleDoorState(isOpen: true);

                    _leaveLocationLever.InteractWithoutEvents(isLeverPulled: false);
                    _leaveLocationLever.ToggleInteract(canInteract: true);

                    EnterState(State.IdleOnLocation);
                    break;

                case State.LeavingLocation:
                    _rpcCaller.LeaveLocation();

                    EnterState(State.MovingOnRoad);
                    break;
            }
        }

        private void OnDoorOpened() => EnableDoorLever();

        private void OnDoorClosed() => EnableDoorLever();

        private void OnInteractWithMobileDoorLever() => InteractWithMobileDoorLeverServerRpc();

        private void OnDoorLeverEnabled() => ToggleDoorState(isOpen: true);

        private void OnDoorLeverDisabled() => ToggleDoorState(isOpen: false);

        private void OnInteractWithLoadLocationLever() => InteractWithLoadLocationLeverServerRpc();

        private void OnLoadLocation() =>
            _rpcCaller.LoadLocation(SceneName.Desert);

        private void OnLocationLoaded()
        {
            LocationManager locationManager = LocationManager.Get();
            CinemachinePath path = locationManager.GetEnterPath();

            _pathMovement.ToggleArrived(isArrived: false);
            ChangePath(path);
            ToggleMovement(canMove: true);

            EnterState(State.ArrivingAtLocation);
        }

        private void OnInteractWithLeaveLocationLever() =>
            _leaveLocationLever.InteractLogic();

        private void OnLeaveLocation() =>
            _rpcCaller.StartLeavingLocation();

        private void OnLeavingLocation()
        {
            LocationManager locationManager = LocationManager.Get();
            CinemachinePath path = locationManager.GetExitPath();

            _pathMovement.ToggleArrived(isArrived: false);
            ChangePath(path);
            ToggleMovement(canMove: true);
            ToggleDoorState(isOpen: false);

            EnterState(State.LeavingLocation);
        }

        private void OnLocationLeft()
        {
            ToggleMovement(canMove: false);
            ArrivedAtRoadLocation();
        }
    }
}