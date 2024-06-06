using System;
using Cinemachine;
using GameCore.Configs.Gameplay.MobileHeadquarters;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.Interactable;
using GameCore.Gameplay.Interactable.MobileHeadquarters;
using GameCore.Gameplay.Level.Locations;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Quests;
using GameCore.Observers.Gameplay.Level;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Entities.MobileHeadquarters
{
    public class MobileHeadquartersEntity : NetcodeBehaviour, IMobileHeadquartersEntity
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IGameManagerDecorator gameManagerDecorator,
            ILocationManagerDecorator locationManagerDecorator, IQuestsManagerDecorator questsManagerDecorator,
            ILevelObserver levelObserver)
        {
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
        public IGameManagerDecorator GameManagerDecorator { get; private set; }
        public IQuestsManagerDecorator QuestsManagerDecorator { get; private set; }
        public PathMovement PathMovement => _pathMovement;
        public GameState GameState => GameManagerDecorator.GetGameState();

        // FIELDS: --------------------------------------------------------------------------------
        
        public static int LastPathID = -1;

        public event Action OnOpenQuestsSelectionMenuEvent = delegate { };
        public event Action OnOpenLocationsSelectionMenuEvent = delegate { };
        public event Action OnOpenGameOverWarningMenuEvent = delegate { };
        public event Action OnCallDeliveryDroneEvent = delegate { };

        private ILocationManagerDecorator _locationManagerDecorator;
        private ILevelObserver _levelObserver;

        private MobileHeadquartersController _mobileHeadquartersController;
        private MoveSpeedController _moveSpeedController;
        private PathMovement _pathMovement;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _pathMovement = new PathMovement(mobileHeadquartersEntity: this);
            _mobileHeadquartersController = new MobileHeadquartersController(mobileHeadquartersEntity: this);
            _moveSpeedController = _references.MoveSpeedController;
            
            _moveSpeedController.Init(_mobileHeadquartersConfig, GameManagerDecorator);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _moveSpeedController.Dispose();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void OpenDoor() => OpenDoorServerRpc();

        public void EnableMainLever() =>
            _mobileHeadquartersController.EnableMainLever();

        public void ChangePath(CinemachinePath path, float startDistancePercent = 0, bool stayAtSamePosition = false)
        {
            _pathMovement.ChangePath(path, startDistancePercent, stayAtSamePosition);
            HandlePathChange();
        }
        
        public void ChangeToTheRoadPath()
        {
            RoadLocationManager roadLocationManager = RoadLocationManager.Get();
            CinemachinePath path;
            float startPositionAtRoadLocation = 0f;

            if (LastPathID == -1)
            {
                path = roadLocationManager.GetMainPath();
                startPositionAtRoadLocation = _mobileHeadquartersConfig.StartPositionAtRoadLocation;
            }
            else
            {
                bool isPathFound = roadLocationManager.TryGetEnterPathByID(LastPathID, out path);

                if (!isPathFound)
                {
                    path = roadLocationManager.GetMainPath();
                    Log.PrintError(log: $"Path with ID <gb>({LastPathID})</gb> <rb>not found</rb>!");
                }
            }

            ChangePath(path, startPositionAtRoadLocation);
        }

        public void TeleportToTheRoad()
        {
            LastPathID = -1;
            _pathMovement.ResetDistance();
            _pathMovement.ToggleArrived(isArrived: false);
            _mobileHeadquartersController.EnableMainLever();
            ChangeToTheRoadPath();
        }

        public void Teleport(Vector3 position, Quaternion rotation)
        {
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
        
        public Camera GetOutsideCamera() => _references.OutsideCamera;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void InitAll()
        {
            _mobileHeadquartersController.InitAll();
            ChangeToTheRoadPath();

            _levelObserver.OnLocationLoadedEvent += OnLocationLoaded;
        }

        protected override void InitServerOnly() =>
            _pathMovement.OnDestinationReachedEvent += OnDestinationReached;

        protected override void TickServerOnly()
        {
            _moveSpeedController.Tick();
            _pathMovement.Movement();
        }

        protected override void DespawnAll()
        {
            _mobileHeadquartersController.DespawnAll();

            _levelObserver.OnLocationLoadedEvent -= OnLocationLoaded;
        }

        protected override void DespawnServerOnly() =>
            _pathMovement.OnDestinationReachedEvent -= OnDestinationReached;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ToggleMovement(bool canMove) =>
            _pathMovement.ToggleMovement(canMove);

        private void ToggleDoorState(bool isOpen) =>
            _mobileHeadquartersController.ToggleDoorState(isOpen);

        private void HandlePathChange()
        {
            switch (GameState)
            {
                case GameState.EnteringMainRoad:
                    GameManagerDecorator.ChangeGameStateWhenAllPlayersReady(newState: GameState.ReadyToLeaveTheRoad,
                        previousState: GameState.EnteringMainRoad);
                    break;
            }
        }

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

                case GameState.LeavingMainRoad:
                    GameManagerDecorator.LoadSelectedLocation();
                    break;

                default:
                    _pathMovement.ResetDistance();
                    _pathMovement.ToggleArrived(isArrived: false);
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