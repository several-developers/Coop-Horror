using Cinemachine;
using Cysharp.Threading.Tasks;
using GameCore.Configs.Gameplay.MobileHeadquarters;
using GameCore.Gameplay.Interactable.MobileHeadquarters;
using GameCore.Gameplay.Levels.Locations;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Network.Utilities;
using GameCore.Observers.Gameplay.Rpc;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using Zenject;

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

        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IRpcHandlerDecorator rpcHandlerDecorator, IRpcObserver rpcObserver)
        {
            RpcHandlerDecorator = rpcHandlerDecorator;
            _rpcObserver = rpcObserver;
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField, Required]
        private MobileHeadquartersConfigMeta _mobileHeadquartersConfig;

        [Title(Constants.References)]
        [SerializeField]
        private MobileHeadquartersReferences _references;

        // PROPERTIES: ----------------------------------------------------------------------------

        public MobileHeadquartersReferences References => _references;
        public IRpcHandlerDecorator RpcHandlerDecorator { get; private set; }

        // FIELDS: --------------------------------------------------------------------------------

        private IRpcObserver _rpcObserver;
        
        private MobileHeadquartersController _mobileHeadquartersController;
        private RigidbodyPathMovement _pathMovement;
        private State _currentState = State.MovingOnRoad;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _pathMovement = new RigidbodyPathMovement(mobileHeadquartersEntity: this, _mobileHeadquartersConfig);
            _mobileHeadquartersController = new MobileHeadquartersController(mobileHeadquartersEntity: this);

            _mobileHeadquartersController.Init();

            _pathMovement.OnDestinationReachedEvent += OnDestinationReached;
        }

        private void Update()
        {
            TickServerAndClient();
            TickServer();
            TickClient();
        }

        public override void OnDestroy()
        {
            _mobileHeadquartersController.Dispose();

            _pathMovement.OnDestinationReachedEvent -= OnDestinationReached;

            base.OnDestroy();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void InitServerAndClient()
        {
            ArrivedAtRoadLocation();
            
            _rpcObserver.OnLocationLoadedEvent += OnLocationLoaded;
            _rpcObserver.OnLeavingLocationEvent += OnLeavingLocation;
            _rpcObserver.OnLocationLeftEvent += OnLocationLeft;
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

        public void TickServerAndClient()
        {
        }

        public void TickServer()
        {
            if (!IsOwner)
                return;

            _pathMovement.Movement();
        }

        public void TickClient()
        {
            if (IsOwner)
                return;
        }

        public void DespawnServerAndClient()
        {
            _rpcObserver.OnLocationLoadedEvent -= OnLocationLoaded;
            _rpcObserver.OnLeavingLocationEvent -= OnLeavingLocation;
            _rpcObserver.OnLocationLeftEvent -= OnLocationLeft;
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

        public NetworkObject GetNetworkObject() => NetworkObject;

        public Vector3 GetVelocity()
        {
            Vector3 direction = transform.forward;
            return direction;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ArrivedAtRoadLocation()
        {
            RoadLocationManager roadLocationManager = RoadLocationManager.Get();
            CinemachinePath path = roadLocationManager.GetPath();

            ChangePath(path);

            LoadLocationLever loadLocationLever = _references.LoadLocationLever;
            loadLocationLever.InteractWithoutEvents(isLeverPulled: false);
            loadLocationLever.ToggleInteract(canInteract: true);
        }

        private void ChangePath(CinemachinePath path)
        {
            _pathMovement.ChangePath(path);
        }

        private void ToggleMovement(bool canMove) =>
            _pathMovement.ToggleMovement(canMove);

        private void EnterState(State state) =>
            _currentState = state;

        // RPC: -----------------------------------------------------------------------------------

        [ServerRpc(RequireOwnership = false)]
        public void InteractWithMobileDoorLeverServerRpc() => InteractWithMobileDoorLeverClientRpc();

        [ServerRpc(RequireOwnership = false)]
        public void InteractWithLoadLocationLeverServerRpc() => InteractWithLoadLocationLeverClientRpc();

        [ClientRpc]
        private void InteractWithMobileDoorLeverClientRpc()
        {
            ToggleMobileDoorLever toggleMobileDoorLever = _references.ToggleMobileDoorLever;
            toggleMobileDoorLever.InteractLogic();
        }

        [ClientRpc]
        private void InteractWithLoadLocationLeverClientRpc()
        {
            LoadLocationLever loadLocationLever = _references.LoadLocationLever;
            loadLocationLever.InteractLogic();
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

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
        
        [Button]
        private void OnDestinationReached()
        {
            switch (_currentState)
            {
                case State.ArrivingAtLocation:
                    _mobileHeadquartersController.ToggleDoorState(isOpen: true);

                    LeaveLocationLever leaveLocationLever = _references.LeaveLocationLever;
                    leaveLocationLever.InteractWithoutEvents(isLeverPulled: false);
                    leaveLocationLever.ToggleInteract(canInteract: true);

                    EnterState(State.IdleOnLocation);
                    break;

                case State.LeavingLocation:
                    RpcHandlerDecorator.LeaveLocation();

                    EnterState(State.MovingOnRoad);
                    break;
            }
        }

        private async void OnLocationLoaded()
        {
#warning ЛОКАЦИЯ НЕ УСПЕВАЕТ ЗАГРУЗИТСЯ НА КЛИЕНТЕ
            bool isCanceled = await UniTask
                .DelayFrame(delayFrameCount: 2, cancellationToken: this.GetCancellationTokenOnDestroy())
                .SuppressCancellationThrow();

            if (isCanceled)
                return;

            LocationManager locationManager = LocationManager.Get();
            CinemachinePath path = locationManager.GetEnterPath();

            _pathMovement.ToggleArrived(isArrived: false);
            ChangePath(path);
            ToggleMovement(canMove: true);

            EnterState(State.ArrivingAtLocation);
        }

        private void OnLeavingLocation()
        {
            LocationManager locationManager = LocationManager.Get();
            CinemachinePath path = locationManager.GetExitPath();

            _pathMovement.ToggleArrived(isArrived: false);
            ChangePath(path);
            ToggleMovement(canMove: true);
            _mobileHeadquartersController.ToggleDoorState(isOpen: false);

            EnterState(State.LeavingLocation);
        }

        private void OnLocationLeft()
        {
            ToggleMovement(canMove: false);
            ArrivedAtRoadLocation();
        }
    }
}