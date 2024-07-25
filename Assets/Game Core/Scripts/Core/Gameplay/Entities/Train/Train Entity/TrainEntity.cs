using System;
using System.Collections.Generic;
using Cinemachine;
using GameCore.Configs.Gameplay.Train;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.Interactable;
using GameCore.Gameplay.Interactable.Train;
using GameCore.Gameplay.Level.Locations;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Quests;
using GameCore.Observers.Gameplay.Level;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Entities.Train
{
    public class TrainEntity : NetcodeBehaviour, ITrainEntity
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(
            IGameManagerDecorator gameManagerDecorator,
            ILocationManagerDecorator locationManagerDecorator,
            IQuestsManagerDecorator questsManagerDecorator,
            ILevelObserver levelObserver
        )
        {
            GameManagerDecorator = gameManagerDecorator;
            QuestsManagerDecorator = questsManagerDecorator;
            _locationManagerDecorator = locationManagerDecorator;
            _levelObserver = levelObserver;
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField, Required]
        private TrainConfigMeta _trainConfig;

        [Title(Constants.References)]
        [SerializeField]
        private TrainReferences _references;

        // PROPERTIES: ----------------------------------------------------------------------------

        public TrainReferences References => _references;
        public IGameManagerDecorator GameManagerDecorator { get; private set; }
        public IQuestsManagerDecorator QuestsManagerDecorator { get; private set; }
        public PathMovement PathMovement => _pathMovement;
        public GameState GameState => GameManagerDecorator.GetGameState();

        // FIELDS: --------------------------------------------------------------------------------

        public static int LastPathID = -1;

        public event Action OnOpenQuestsSelectionMenuEvent = delegate { };
        public event Action OnOpenGameOverWarningMenuEvent = delegate { };
        public event Action OnOpenGameMapEvent = delegate { };

        private readonly NetworkVariable<SeatsRuntimeDataContainer> _seatsData =
            new(writePerm: Constants.OwnerPermission);

        private readonly NetworkVariable<bool> _doorState = new(writePerm: Constants.OwnerPermission);

        private ILocationManagerDecorator _locationManagerDecorator;
        private ILevelObserver _levelObserver;

        private TrainController _trainController;
        private MoveSpeedController _moveSpeedController;
        private PathMovement _pathMovement;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _pathMovement = new PathMovement(trainEntity: this);
            _trainController = new TrainController(trainEntity: this);
            _moveSpeedController = _references.MoveSpeedController;
            LastPathID = -1;

            _moveSpeedController.Init(_trainConfig);
            InitMobileHQSeats();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void OpenDoor() => OpenDoorServerRpc();

        public void EnableMainLever() =>
            _trainController.EnableMainLever();

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
                startPositionAtRoadLocation = _trainConfig.StartPositionAtRoadLocation;
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
            _trainController.EnableMainLever();
            ChangeToTheRoadPath();
        }

        public void SendOpenQuestsSelectionMenu() =>
            OnOpenQuestsSelectionMenuEvent.Invoke();

        public void SendOpenGameOverWarningMenu() =>
            OnOpenGameOverWarningMenuEvent.Invoke();

        public void SendOpenGameMapEvent() =>
            OnOpenGameMapEvent.Invoke();

        public MonoBehaviour GetMonoBehaviour() => this;

        public Transform GetTransform() => transform;

        public NetworkObject GetNetworkObject() => NetworkObject;

        public Camera GetOutsideCamera() => _references.OutsideCamera;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void InitAll()
        {
            _trainController.InitAll();
            ChangeToTheRoadPath();

            _levelObserver.OnLocationLoadedEvent += OnLocationLoaded;

            _doorState.OnValueChanged += OnDoorStateChanged;
        }

        protected override void InitServerOnly()
        {
            IReadOnlyList<TrainSeat> allMobileHQSeats = _references.GetAllMobileHQSeats();
            int mobileHQSeatsAmount = allMobileHQSeats.Count;
            _seatsData.Value = new SeatsRuntimeDataContainer(mobileHQSeatsAmount);
            
            _pathMovement.OnDestinationReachedEvent += OnDestinationReached;
        }

        protected override void TickServerOnly()
        {
            _moveSpeedController.Tick();
            _pathMovement.Movement();
        }

        protected override void DespawnAll()
        {
            _trainController.DespawnAll();

            _levelObserver.OnLocationLoadedEvent -= OnLocationLoaded;
            
            _doorState.OnValueChanged -= OnDoorStateChanged;
        }

        protected override void DespawnServerOnly() =>
            _pathMovement.OnDestinationReachedEvent -= OnDestinationReached;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void InitMobileHQSeats()
        {
            IReadOnlyList<TrainSeat> allMobileHQSeats = _references.GetAllMobileHQSeats();
            int seatsAmount = allMobileHQSeats.Count;

            for (int i = 0; i < seatsAmount; i++)
            {
                TrainSeat trainSeat = allMobileHQSeats[i];
                trainSeat.SetSeatIndex(i);
                
                trainSeat.OnTakeSeatEvent += OnTakeSeat;
                trainSeat.OnLeftSeatEvent += OnLeftSeat;
                trainSeat.IsSeatBusyEvent += IsSeatBusy;
                trainSeat.ShouldRemovePlayerParentEvent += ShouldRemovePlayerParent;
            }
        }

        private void ToggleMovement(bool canMove) =>
            _pathMovement.ToggleMovement(canMove);

        private void ToggleDoorState(bool isOpen) =>
            _trainController.ToggleDoorState(isOpen);
        
        private void TeleportLocalPlayerToRandomSeat()
        {
            PlayerEntity playerEntity = PlayerEntity.GetLocalPlayer();
            bool hasParent = playerEntity.transform.parent != null;

            if (hasParent)
                return;

            SeatsRuntimeDataContainer seatsRuntimeDataContainer = _seatsData.Value;
            int seatIndex = seatsRuntimeDataContainer.GetRandomFreeSeatIndex();

            IReadOnlyList<TrainSeat> allMobileHQSeats = _references.GetAllMobileHQSeats();

            foreach (TrainSeat mobileHQSeat in allMobileHQSeats)
            {
                bool isMatches = mobileHQSeat.SeatIndex == seatIndex;

                if (!isMatches)
                    continue;

                mobileHQSeat.Interact(playerEntity);
                break;
            }
        }

        private void ToggleSeatsColliders(bool isEnabled)
        {
            IReadOnlyList<TrainSeat> allMobileHQSeats = _references.GetAllMobileHQSeats();

            foreach (TrainSeat mobileHQSeat in allMobileHQSeats)
                mobileHQSeat.ToggleCollider(isEnabled);
        }

        private void HandlePathChange()
        {
            switch (GameState)
            {
                case GameState.EnteringMainRoad:
                    GameManagerDecorator.ChangeGameStateWhenAllPlayersReady(newState: GameState.CycleMovement,
                        previousState: GameState.EnteringMainRoad);
                    break;
            }
        }

        private static bool IsCurrentPlayer(ulong senderClientID) =>
            NetworkHorror.ClientID == senderClientID;
        
        private bool IsSeatBusy(int seatIndex)
        {
            SeatsRuntimeDataContainer seatsRuntimeDataContainer = _seatsData.Value;
            int freeSeatsAmount = seatsRuntimeDataContainer.GetFreeSeatsAmount();

            if (freeSeatsAmount == 0)
                return true;

            IEnumerable<SeatRuntimeData> allSeatsData = seatsRuntimeDataContainer.GetAllSeatsData();
            bool isSeatBusy = true;

            foreach (SeatRuntimeData seatRuntimeData in allSeatsData)
            {
                bool isMatches = seatRuntimeData.SeatIndex == seatIndex;
                
                if (!isMatches)
                    continue;
                
                isSeatBusy = seatRuntimeData.IsBusy;
                break;
            }

            return isSeatBusy;
        }

        // TEMP
        private bool ShouldRemovePlayerParent()
        {
            GameState gameState = GameManagerDecorator.GetGameState();
            bool isGameStateValid = gameState == GameState.ReadyToLeaveTheLocation;
            return isGameStateValid;
        }

        // RPC: -----------------------------------------------------------------------------------

#warning TEMP
        [ServerRpc(RequireOwnership = false)]
        public void StartLeavingLocationServerRpc()
        {
            CinemachinePath path = _locationManagerDecorator.GetExitPath();

            TeleportLocalPlayerToRandomSeat();
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
        public void PlayCompleteQuestsButtonAnimationServerRpc(ServerRpcParams serverRpcParams = default)
        {
            ulong senderClientId = serverRpcParams.Receive.SenderClientId;
            PlayCompleteQuestsButtonAnimationClientRpc(senderClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        private void TakeSeatServerRpc(int seatIndex) =>
            _seatsData.Value.TakeSeat(seatIndex);
        
        [ServerRpc(RequireOwnership = false)]
        private void LeftSeatServerRpc(int seatIndex) =>
            _seatsData.Value.LeftSeat(seatIndex);

        [ServerRpc(RequireOwnership = false)]
        public void ToggleDoorServerRpc(bool isOpen) =>
            _doorState.Value = isOpen;

        [ClientRpc]
        private void StartLeavingLocationClientRpc() => ToggleMovement(canMove: true);

        [ClientRpc]
        private void OpenDoorClientRpc() => ToggleDoorState(isOpen: true);

        [ClientRpc]
        private void MainLeverAnimationClientRpc(ulong senderClientID)
        {
            if (IsCurrentPlayer(senderClientID))
                return;

            TrainMainLever mainLever = _references.MainLever;
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

        private void OnTakeSeat(int seatIndex)
        {
            ToggleSeatsColliders(isEnabled: false);
            TakeSeatServerRpc(seatIndex);
        }
        
        private void OnLeftSeat(int seatIndex)
        {
            ToggleSeatsColliders(isEnabled: true);
            LeftSeatServerRpc(seatIndex);
        }

        private void OnDoorStateChanged(bool previousValue, bool newValue) =>
            _references.Doors.SetActive(!newValue);
    }
}