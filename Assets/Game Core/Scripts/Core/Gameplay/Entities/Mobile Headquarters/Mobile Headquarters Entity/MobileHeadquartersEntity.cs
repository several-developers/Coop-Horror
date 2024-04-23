using System;
using Cinemachine;
using GameCore.Configs.Gameplay.MobileHeadquarters;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.Interactable;
using GameCore.Gameplay.Interactable.MobileHeadquarters;
using GameCore.Gameplay.Levels.Locations;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Network.Utilities;
using GameCore.Observers.Gameplay.Level;
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
        private void Construct(IRpcHandlerDecorator rpcHandlerDecorator, IGameManagerDecorator gameManagerDecorator,
            ILocationManagerDecorator locationManagerDecorator, ILevelObserver levelObserver, IRpcObserver rpcObserver)
        {
            RpcHandlerDecorator = rpcHandlerDecorator;
            GameManagerDecorator = gameManagerDecorator;
            _locationManagerDecorator = locationManagerDecorator;
            _levelObserver = levelObserver;
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
        public IGameManagerDecorator GameManagerDecorator { get; private set; }

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnOpenQuestsSelectionMenuEvent = delegate { }; // Server only.
        public event Action OnOpenLocationsSelectionMenuEvent = delegate { };
        public event Action OnCallDeliveryDroneEvent = delegate { }; // Server only.

        private ILocationManagerDecorator _locationManagerDecorator;
        private ILevelObserver _levelObserver;
        private IRpcObserver _rpcObserver;

        private MobileHeadquartersController _mobileHeadquartersController;
        private RigidbodyPathMovement _pathMovement;
        private State _currentState = State.MovingOnRoad;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _pathMovement = new RigidbodyPathMovement(mobileHeadquartersEntity: this, _mobileHeadquartersConfig);
            _mobileHeadquartersController = new MobileHeadquartersController(mobileHeadquartersEntity: this);

            SimpleButton openQuestsSelectionMenuButton = _references.OpenQuestsSelectionMenuButton;
            openQuestsSelectionMenuButton.OnTriggerEvent += OnOpenQuestsSelectionMenu;

            SimpleButton openLocationsSelectionMenuButton = _references.OpenLocationsSelectionMenuButton;
            openLocationsSelectionMenuButton.OnTriggerEvent += OnOpenLocationsSelectionMenu;

            SimpleButton callDeliveryDroneButton = _references.CallDeliveryDroneButton;
            callDeliveryDroneButton.OnTriggerEvent += OnCallDeliveryDrone;
        }

        private void Update()
        {
            TickServerAndClient();
            TickServer();
            TickClient();
        }

        public override void OnDestroy()
        {
            SimpleButton openQuestsSelectionMenuButton = _references.OpenQuestsSelectionMenuButton;
            openQuestsSelectionMenuButton.OnTriggerEvent -= OnOpenQuestsSelectionMenu;

            SimpleButton openLocationsSelectionMenuButton = _references.OpenLocationsSelectionMenuButton;
            openLocationsSelectionMenuButton.OnTriggerEvent -= OnOpenLocationsSelectionMenu;

            SimpleButton callDeliveryDroneButton = _references.CallDeliveryDroneButton;
            callDeliveryDroneButton.OnTriggerEvent -= OnCallDeliveryDrone;

            base.OnDestroy();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void InitServerAndClient()
        {
            _mobileHeadquartersController.InitServerAndClient();

            ArrivedAtRoadLocation();

            _levelObserver.OnLocationLoadedEvent += OnLocationLoaded;

            _rpcObserver.OnStartLeavingLocationEvent += StartLeavingLocation;
            _rpcObserver.OnLocationLeftEvent += OnLocationLeft;

            _pathMovement.OnDestinationReachedEvent += OnDestinationReached;
        }

        public void InitServer()
        {
            if (!IsOwner)
                return;

            _mobileHeadquartersController.InitServer();
        }

        public void InitClient()
        {
            if (IsOwner)
                return;

            _mobileHeadquartersController.InitClient();
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
            _mobileHeadquartersController.DespawnServerAndClient();

            _levelObserver.OnLocationLoadedEvent -= OnLocationLoaded;

            _rpcObserver.OnStartLeavingLocationEvent -= StartLeavingLocation;
            _rpcObserver.OnLocationLeftEvent -= OnLocationLeft;

            _pathMovement.OnDestinationReachedEvent -= OnDestinationReached;
        }

        public void DespawnServer()
        {
            if (!IsOwner)
                return;

            _mobileHeadquartersController.DespawnServer();
        }

        public void DespawnClient()
        {
            if (IsOwner)
                return;

            _mobileHeadquartersController.DespawnClient();
        }

        public void OpenDoor() => OpenDoorServerRpc();

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

            MobileHQMainLever mainLever = _references.MainLever;
            mainLever.InteractWithoutEvents(isLeverPulled: false);
            mainLever.ToggleInteract(canInteract: true);
        }

        private void ChangePath(CinemachinePath path) =>
            _pathMovement.ChangePath(path);

        private void ToggleMovement(bool canMove) =>
            _pathMovement.ToggleMovement(canMove);

        private void EnterState(State state)
        {
            _currentState = state;

            switch (state)
            {
                case State.IdleOnLocation:
                    _mobileHeadquartersController.ToggleDoorState(isOpen: true);

                    MobileHQMainLever mainLever = _references.MainLever;
                    mainLever.InteractWithoutEvents(isLeverPulled: false);
                    mainLever.ToggleInteract(canInteract: true);
                    break;
            }
        }

        // RPC: -----------------------------------------------------------------------------------

        [ServerRpc(RequireOwnership = false)]
        public void LoadLocationServerRpc() => LoadLocationClientRpc();

        [ServerRpc]
        private void DestinationReachedServerRpc() => DestinationReachedClientRpc();

        [ServerRpc]
        private void OpenDoorServerRpc() => OpenDoorClientRpc();

        [ClientRpc]
        private void LoadLocationClientRpc()
        {
            MobileHQMainLever loadLocationLever = _references.MainLever;
            loadLocationLever.InteractLogic();

            _mobileHeadquartersController.ToggleDoorState(isOpen: false);
        }

        [ClientRpc]
        private void DestinationReachedClientRpc()
        {
            if (IsOwner)
                return;

            OnDestinationReached();
        }

        [ClientRpc]
        private void OpenDoorClientRpc() =>
            _mobileHeadquartersController.ToggleDoorState(isOpen: true);

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
                    if (IsOwner)
                        DestinationReachedServerRpc();

                    EnterState(State.IdleOnLocation);
                    break;

                case State.LeavingLocation:
                    _levelObserver.LocationLeft();
                    RpcHandlerDecorator.LocationLeft();

                    EnterState(State.MovingOnRoad);
                    break;
            }
        }

        private void OnLocationLoaded()
        {
            CinemachinePath path = _locationManagerDecorator.GetEnterPath();

            _pathMovement.ToggleArrived(isArrived: false);
            ChangePath(path);
            ToggleMovement(canMove: true);

#warning Синхронизировать через Network Variable?
            EnterState(State.ArrivingAtLocation);
        }

        private void StartLeavingLocation()
        {
            CinemachinePath path = _locationManagerDecorator.GetExitPath();

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

        private void OnOpenQuestsSelectionMenu() =>
            OnOpenQuestsSelectionMenuEvent.Invoke();

        private void OnOpenLocationsSelectionMenu() =>
            OnOpenLocationsSelectionMenuEvent.Invoke();

        private void OnCallDeliveryDrone() =>
            OnCallDeliveryDroneEvent.Invoke();
    }
}