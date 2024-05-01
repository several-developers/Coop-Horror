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
            ILevelObserver levelObserver)
        {
            RpcHandlerDecorator = rpcHandlerDecorator;
            GameManagerDecorator = gameManagerDecorator;
            QuestsManagerDecorator = questsManagerDecorator;
            _locationManagerDecorator = locationManagerDecorator;
            _levelObserver = levelObserver;
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
        public event Action OnOpenGameOverWarningMenuEvent = delegate { };
        public event Action OnCallDeliveryDroneEvent = delegate { };

        private ILocationManagerDecorator _locationManagerDecorator;
        private ILevelObserver _levelObserver;

        private MobileHeadquartersController _mobileHeadquartersController;
        private RigidbodyPathMovement _pathMovement;
        private bool _isInitialized;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _pathMovement = new RigidbodyPathMovement(mobileHeadquartersEntity: this, _mobileHeadquartersConfig);
            _mobileHeadquartersController = new MobileHeadquartersController(mobileHeadquartersEntity: this);
        }

        private void Update()
        {
            if (!_isInitialized)
                return;

            TickServerAndClient();
            TickServer();
            TickClient();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void InitServerAndClient()
        {
            _isInitialized = true;

            _mobileHeadquartersController.InitServerAndClient();
            ArrivedAtRoadLocation();

            _levelObserver.OnLocationLoadedEvent += OnLocationLoaded;
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

        public void EnableMainLever() =>
            _mobileHeadquartersController.EnableMainLever();

        public void ToggleMovement(bool canMove) =>
            _pathMovement.ToggleMovement(canMove);

        public void ArrivedAtRoadLocation()
        {
            RoadLocationManager roadLocationManager = RoadLocationManager.Get();
            CinemachinePath path = roadLocationManager.GetPath();

            ChangePath(path);
        }

        public void SendOpenQuestsSelectionMenu() =>
            OnOpenQuestsSelectionMenuEvent.Invoke();

        public void SendOpenLocationsSelectionMenu() =>
            OnOpenLocationsSelectionMenuEvent.Invoke();

        public void SendOpenGameOverWarningMenu() =>
            OnOpenGameOverWarningMenuEvent.Invoke();

        public void SendCallDeliveryDrone() =>
            OnCallDeliveryDroneEvent.Invoke();

        public Transform GetTransform() => transform;

        public NetworkObject GetNetworkObject() => NetworkObject;

        public Vector3 GetVelocity()
        {
            Vector3 direction = transform.forward;
            return direction;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ChangePath(CinemachinePath path) =>
            _pathMovement.ChangePath(path);

        private void ToggleDoorState(bool isOpen) =>
            _mobileHeadquartersController.ToggleDoorState(isOpen);

        private static bool IsCurrentPlayer(ulong senderClientID) =>
            NetworkHorror.ClientID == senderClientID;

        // RPC: -----------------------------------------------------------------------------------

#warning TEMP
        [ServerRpc(RequireOwnership = false)]
        public void StartLeavingLocationServerRpc()
        {
            CinemachinePath path = _locationManagerDecorator.GetExitPath();

            _pathMovement.ToggleArrived(isArrived: false);
            ChangePath(path);
            StartLeavingLocationClientRpc();
        }

        [ServerRpc]
        private void OpenDoorServerRpc() => OpenDoorClientRpc();

        [ServerRpc(RequireOwnership = false)]
        public void MainLeverAnimationServerRpc(ServerRpcParams serverRpcParams = default)
        {
            ulong senderClientId = serverRpcParams.Receive.SenderClientId;
            MainLeverAnimationClientRpc(senderClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void PlayQuestsButtonAnimationServerRpc(ServerRpcParams serverRpcParams = default)
        {
            ulong senderClientId = serverRpcParams.Receive.SenderClientId;
            PlayQuestsButtonAnimationClientRpc(senderClientId);
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void PlayLocationsButtonAnimationServerRpc(ServerRpcParams serverRpcParams = default)
        {
            ulong senderClientId = serverRpcParams.Receive.SenderClientId;
            PlayLocationsButtonAnimationClientRpc(senderClientId);
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void PlayDeliveryDroneButtonAnimationServerRpc(ServerRpcParams serverRpcParams = default)
        {
            ulong senderClientId = serverRpcParams.Receive.SenderClientId;
            PlayDeliveryDroneButtonAnimationClientRpc(senderClientId);
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void PlayCompleteQuestsButtonAnimationServerRpc(ServerRpcParams serverRpcParams = default)
        {
            ulong senderClientId = serverRpcParams.Receive.SenderClientId;
            PlayCompleteQuestsButtonAnimationClientRpc(senderClientId);
        }
        
        [ClientRpc]
        private void StartLeavingLocationClientRpc() => ToggleMovement(canMove: true);

        [ClientRpc]
        private void OpenDoorClientRpc() => ToggleDoorState(isOpen: true);

        [ClientRpc]
        private void MainLeverAnimationClientRpc(ulong senderClientID)
        {
            if (IsCurrentPlayer(senderClientID))
                return;

            MobileHQMainLever mainLever = _references.MainLever;
            mainLever.InteractWithoutEvents(isLeverPulled: true);
        }
        
        [ClientRpc]
        private void PlayQuestsButtonAnimationClientRpc(ulong senderClientID)
        {
            if (IsCurrentPlayer(senderClientID))
                return;

            SimpleButton openQuestsSelectionMenuButton = _references.OpenQuestsSelectionMenuButton;
            openQuestsSelectionMenuButton.PlayInteractAnimation();
        }
        
        [ClientRpc]
        private void PlayLocationsButtonAnimationClientRpc(ulong senderClientID)
        {
            if (IsCurrentPlayer(senderClientID))
                return;
            
            SimpleButton openLocationsSelectionMenuButton = _references.OpenLocationsSelectionMenuButton;
            openLocationsSelectionMenuButton.PlayInteractAnimation();
        }
        
        [ClientRpc]
        private void PlayDeliveryDroneButtonAnimationClientRpc(ulong senderClientID)
        {
            if (IsCurrentPlayer(senderClientID))
                return;
            
            SimpleButton callDeliveryDroneButton = _references.CallDeliveryDroneButton;
            callDeliveryDroneButton.PlayInteractAnimation();
        }
        
        [ClientRpc]
        private void PlayCompleteQuestsButtonAnimationClientRpc(ulong senderClientID)
        {
            if (IsCurrentPlayer(senderClientID))
                return;
            
            SimpleButton completeQuestsButton = _references.CompleteQuestsButton;
            completeQuestsButton.PlayInteractAnimation();
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

        private void OnDestinationReached()
        {
            switch (GameState)
            {
                case GameState.HeadingToTheLocation:
                    GameManagerDecorator.ChangeGameStateWhenAllPlayersReady(newState: GameState.ArrivedAtTheLocation,
                        previousState: GameState.HeadingToTheLocation);
                    break;

                case GameState.HeadingToTheRoad:
                    _levelObserver.LocationLeft();
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
    }
}