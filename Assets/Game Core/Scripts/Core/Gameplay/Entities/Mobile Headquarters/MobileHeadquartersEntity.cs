using System;
using Cinemachine;
using EFPController;
using GameCore.Configs.Gameplay.MobileHeadquarters;
using GameCore.Enums;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Interactable.MobileHeadquarters;
using GameCore.Gameplay.Locations;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Other;
using Sirenix.OdinInspector;
using Unity.Mathematics;
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
        private NetworkObject _networkObject;

        [SerializeField, Required]
        private Transform _playerPoint;

        [SerializeField, Required]
        private ToggleMobileDoorLever _toggleMobileDoorLever;

        [SerializeField, Required]
        private LoadLocationLever _loadLocationLever;

        [SerializeField, Required]
        private LeaveLocationLever _leaveLocationLever;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnTeleportedEvent; 

        private static MobileHeadquartersEntity _instance;

        private readonly NetworkVariable<float> _pathPosition = new();

        private RigidbodyPathMovement _pathMovement;
        private RpcCaller _rpcCaller;

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

        private void FixedUpdate() =>
            _pathMovement.Movement();

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void ArriveAtRoadLocation(bool sendTeleportEvent = true)
        {
            RoadLocationManager roadLocationManager = RoadLocationManager.Get();
            CinemachinePath path = roadLocationManager.GetPath();
            
            ChangePath(path, sendTeleportEvent);
            
            _loadLocationLever.InteractWithoutEvents(isLeverPulled: false);
            _loadLocationLever.ToggleInteract(canInteract: true);
        }

        public Transform GetTransform() => transform;

        public Transform GetPlayerPoint() => _playerPoint;

        public NetworkObject GetNetworkObject() => _networkObject;

        public Vector3 GetVelocity()
        {
            Vector3 direction = transform.forward;
            return direction;
        }
        
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
            _rpcCaller.OnLocationLeftEvent += OnLocationLeft;
        }
        
        private void InitServer()
        {
            if (!IsOwner)
                return;
        }

        private void InitClient()
        {
            if (IsOwner)
                return;
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
            _rpcCaller.OnLocationLeftEvent -= OnLocationLeft;
        }

        private void DespawnServer()
        {
            if (!IsOwner)
                return;
        }

        private void DespawnClient()
        {
            if (IsOwner)
                return;
        }

        private void ChangePath(CinemachinePath path, bool sendTeleportEvent = true)
        {
            PlayerEntity playerEntity = PlayerEntity.Instance;

            if (sendTeleportEvent)
            {
                _playerPoint.position = playerEntity.transform.position;
            }

            Quaternion startRotation = transform.rotation;
            
            _pathMovement.ChangePath(path);
            
            if (!sendTeleportEvent)
                return;
            
            Quaternion newRotation = transform.rotation;
            
            Transform playerTransform = playerEntity.transform;
            playerTransform.position = _playerPoint.position;
            
            var player = playerEntity.GetComponent<EFPController.Player>();
            SmoothLook smoothLook = player.SmoothLook;

            Vector3 difference = newRotation.eulerAngles - startRotation.eulerAngles;
            
            smoothLook.RotationX += difference.y;

            OnTeleportedEvent?.Invoke();
        }

        [Button]
        private void ToggleMovement(bool canMove)
        {
            _pathMovement.ToggleMovement(canMove);
            //_pathMovement.ToggleMoveAnimation(canMove);
        }

        private void ToggleDoorState(bool isOpen) =>
            _animator.SetBool(id: AnimatorHashes.IsOpen, isOpen);

        private void EnableDoorLever() =>
            _toggleMobileDoorLever.ToggleInteract(canInteract: true);

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
            _leaveLocationLever.InteractWithoutEvents(isLeverPulled: false);
            _leaveLocationLever.ToggleInteract(canInteract: true);
        }

        private void OnDoorOpened() => EnableDoorLever();

        private void OnDoorClosed() => EnableDoorLever();

        private void OnInteractWithMobileDoorLever() => InteractWithMobileDoorLeverServerRpc();

        private void OnDoorLeverEnabled() => ToggleDoorState(isOpen: true);

        private void OnDoorLeverDisabled() => ToggleDoorState(isOpen: false);

        private void OnInteractWithLoadLocationLever() => InteractWithLoadLocationLeverServerRpc();

        private void OnLoadLocation() =>
            _rpcCaller.LoadLocation(SceneName.Desert);

        private void OnInteractWithLeaveLocationLever() =>
            _leaveLocationLever.InteractLogic();

        private void OnLeaveLocation() =>
            _rpcCaller.LeaveLocation();

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

        private void OnLocationLeft()
        {
            ToggleMovement(canMove: false);
            ArriveAtRoadLocation();
        }
    }
}