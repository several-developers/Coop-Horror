using System;
using Cinemachine;
using GameCore.Configs.Gameplay.MobileHeadquarters;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.Interactable;
using GameCore.Gameplay.Interactable.MobileHeadquarters;
using GameCore.Gameplay.Levels.Locations;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Network.Utilities;
using GameCore.Gameplay.Quests;
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
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IRpcHandlerDecorator rpcHandlerDecorator, IGameManagerDecorator gameManagerDecorator,
            ILocationManagerDecorator locationManagerDecorator, IQuestsManagerDecorator questsManagerDecorator,
            ILevelObserver levelObserver, IRpcObserver rpcObserver)
        {
            RpcHandlerDecorator = rpcHandlerDecorator;
            GameManagerDecorator = gameManagerDecorator;
            QuestsManagerDecorator = questsManagerDecorator;
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
        public IQuestsManagerDecorator QuestsManagerDecorator { get; private set; }
        public GameState GameState => GameManagerDecorator.GetGameState();

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnOpenQuestsSelectionMenuEvent = delegate { };
        public event Action OnOpenLocationsSelectionMenuEvent = delegate { };
        public event Action OnCallDeliveryDroneEvent = delegate { };

        private ILocationManagerDecorator _locationManagerDecorator;
        private ILevelObserver _levelObserver;
        private IRpcObserver _rpcObserver;
        
        private MobileHeadquartersController _mobileHeadquartersController;
        private RigidbodyPathMovement _pathMovement;

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
            
            SimpleButton completeQuestsButton = _references.CompleteQuestsButton;
            completeQuestsButton.OnTriggerEvent += OnCompleteQuests;
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
            
            SimpleButton completeQuestsButton = _references.CompleteQuestsButton;
            completeQuestsButton.OnTriggerEvent -= OnCompleteQuests;

            base.OnDestroy();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void InitServerAndClient()
        {
            _mobileHeadquartersController.InitServerAndClient();

            ArrivedAtRoadLocation();

            _levelObserver.OnLocationLoadedEvent += OnLocationLoaded;

            _rpcObserver.OnLocationLeftEvent += OnLocationLeft;
        }

        public void InitServer()
        {
            if (!IsOwner)
                return;

            _mobileHeadquartersController.InitServer();
            
            _pathMovement.OnDestinationReachedEvent += OnDestinationReached;
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

            _rpcObserver.OnLocationLeftEvent -= OnLocationLeft;
        }

        public void DespawnServer()
        {
            if (!IsOwner)
                return;

            _mobileHeadquartersController.DespawnServer();
            
            _pathMovement.OnDestinationReachedEvent -= OnDestinationReached;
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

        private void ToggleDoorState(bool isOpen) =>
            _mobileHeadquartersController.ToggleDoorState(isOpen);

        // RPC: -----------------------------------------------------------------------------------

        [ServerRpc(RequireOwnership = false)]
        public void LoadLocationServerRpc() => LoadLocationClientRpc();

        [ServerRpc(RequireOwnership = false)]
        public void StartLeavingLocationServerRpc() => StartLeavingLocationClientRpc();

        [ClientRpc]
        private void LoadLocationClientRpc()
        {
            MobileHQMainLever loadLocationLever = _references.MainLever;
            loadLocationLever.InteractLogic();

            ToggleDoorState(isOpen: false);
        }

        [ClientRpc]
        private void StartLeavingLocationClientRpc()
        {
            CinemachinePath path = _locationManagerDecorator.GetExitPath();

            _pathMovement.ToggleArrived(isArrived: false);
            ChangePath(path);
            ToggleMovement(canMove: true);
            ToggleDoorState(isOpen: false);
        }
        
        [ServerRpc]
        private void OpenDoorServerRpc() => OpenDoorClientRpc();

        [ClientRpc]
        private void OpenDoorClientRpc() => ToggleDoorState(isOpen: true);

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

        private void OnDestinationReached()
        {
            switch (GameState)
            {
                case GameState.HeadingToTheLocation:
                    GameManagerDecorator.ChangeGameState(GameState.ArrivedAtTheLocation);
                    break;
                
                case GameState.HeadingToTheRoad:
                    _levelObserver.LocationLeft();
                    RpcHandlerDecorator.LocationLeft();
                    break;
            }
        }

        private void OnLocationLoaded()
        {
            CinemachinePath path = _locationManagerDecorator.GetEnterPath();

            _pathMovement.ToggleArrived(isArrived: false);
            ChangePath(path);
            ToggleMovement(canMove: true);
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

        private void OnCompleteQuests()
        {
            QuestsManagerDecorator.CompleteQuests();
            GameManagerDecorator.ChangeGameState(GameState.QuestsRewarding);
        }
    }
}