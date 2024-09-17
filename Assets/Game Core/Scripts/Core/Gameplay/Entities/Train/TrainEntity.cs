﻿using System;
using System.Collections.Generic;
using Cinemachine;
using Cysharp.Threading.Tasks;
using GameCore.Configs.Gameplay.Train;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.Interactable.Train;
using GameCore.Gameplay.Level.Locations;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Systems.Quests;
using GameCore.Gameplay.Systems.SoundReproducer;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Entities.Train
{
    [GenerateSerializationForType(typeof(SFXType))]
    public class TrainEntity : SoundProducerEntity<TrainEntity.SFXType>, ITrainEntity
    {
        public enum SFXType
        {
            //_ = 0,
            DoorOpen = 1,
            DoorClose = 2,
            Departure = 3,
            Arrival = 4,
            MovementLoop = 5
        }

        public enum MovementBehaviour
        {
            InfiniteMovement = 0,
            StopAtPathEnd = 1,
            LeaveAtPathEnd = 2
        }

        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(
            IGameManagerDecorator gameManagerDecorator,
            IQuestsManagerDecorator questsManagerDecorator,
            IGameplayConfigsProvider gameplayConfigsProvider
        )
        {
            GameManagerDecorator = gameManagerDecorator;
            QuestsManagerDecorator = questsManagerDecorator;
            _trainConfig = gameplayConfigsProvider.GetConfig<TrainConfigMeta>();
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField]
        private TrainReferences _references;

        // PROPERTIES: ----------------------------------------------------------------------------

        public TrainReferences References => _references;
        public IGameManagerDecorator GameManagerDecorator { get; private set; }
        public IQuestsManagerDecorator QuestsManagerDecorator { get; private set; }

        // FIELDS: --------------------------------------------------------------------------------

        public static int LastPathID = -1;

        public event Action OnMovementStoppedEvent = delegate { };
        public event Action OnMovementStartedEvent = delegate { };
        public event Action OnLeaveLocationEvent = delegate { };
        public event Action OnOpenQuestsSelectionMenuEvent = delegate { };
        public event Action OnOpenGameOverWarningMenuEvent = delegate { };
        public event Action OnOpenGameMapEvent = delegate { };

        private const NetworkVariableWritePermission OwnerPermission = Constants.OwnerPermission;
        
        private readonly NetworkVariable<SeatsRuntimeDataContainer> _seatsData = new(writePerm: OwnerPermission);
        private readonly NetworkVariable<bool> _isMainLeverEnabled = new(value: true, writePerm: OwnerPermission);
        private readonly NetworkVariable<bool> _isDoorOpened = new(writePerm: OwnerPermission);
        private readonly NetworkVariable<bool> _isStoppedAtSector = new(writePerm: OwnerPermission);

        private TrainConfigMeta _trainConfig;
        private TrainController _trainController;
        private MoveSpeedController _moveSpeedController;
        private PathMovement _pathMovement;

        private MovementBehaviour _movementBehaviour;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _pathMovement = new PathMovement(trainEntity: this, _trainConfig);
            _trainController = new TrainController(trainEntity: this);
            _moveSpeedController = _references.MoveSpeedController;
            LastPathID = -1;

            _moveSpeedController.Init(_trainConfig);
            InitMobileHQSeats();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void ChangePath(CinemachinePath path, float startDistancePercent = 0, bool stayAtSamePosition = false) =>
            _pathMovement.ChangePath(path, startDistancePercent, stayAtSamePosition);

        public void SetMovementBehaviour(MovementBehaviour movementBehaviour) =>
            _movementBehaviour = movementBehaviour;

        public void TeleportToTheRoad()
        {
            LastPathID = -1;
            _pathMovement.ResetDistance();
            _pathMovement.ToggleArrived(isArrived: false);
            ChangeToTheMetroPath();
            _pathMovement.SetMovementType(PathMovement.MovementType.Cycle);
        }

        public void TeleportToTheMetroPlatform()
        {
            LocationManager locationManager = LocationManager.Get();
            CinemachinePath path = locationManager.GetEnterPath();

            _pathMovement.ToggleArrived(isArrived: false);
            ChangePath(path);
            ToggleMovement(canMove: true);
            _pathMovement.SetMovementType(PathMovement.MovementType.SlowingDown);
            _pathMovement.SlowDownTrain();
        }

        public void TeleportAllPlayersToRandomSeats(bool ignoreChecks = false)
        {
            IReadOnlyDictionary<ulong, PlayerEntity> allPlayers = PlayerEntity.GetAllPlayers();

            foreach (PlayerEntity playerEntity in allPlayers.Values)
                TeleportPlayerToRandomSeat(playerEntity, ignoreChecks);
        }

        public void TeleportLocalPlayerToRandomSeat(bool ignoreChecks = false) =>
            TeleportLocalPlayerToRandomSeatRpc(ignoreChecks);

        public void TeleportLocalPlayerToTrainSeat(int seatIndex)
        {
            IReadOnlyList<TrainSeat> allMobileHQSeats = _references.GetAllMobileHQSeats();

            foreach (TrainSeat mobileHQSeat in allMobileHQSeats)
            {
                bool isMatches = mobileHQSeat.SeatIndex == seatIndex;

                if (!isMatches)
                    continue;

                PlayerEntity localPlayer = PlayerEntity.GetLocalPlayer();
                localPlayer.References.Rigidbody.velocity = Vector3.zero;
                
                mobileHQSeat.Interact(localPlayer);
                break;
            }
        }

        [Button(ButtonStyle.FoldoutButton)]
        public void ToggleMainLeverState(bool isEnabled)
        {
            if (IsServerOnly)
                _isMainLeverEnabled.Value = isEnabled;
            else
                ToggleMainLeverStateServerRpc(isEnabled);
        }

        public void ToggleDoorState(bool isOpened)
        {
            if (IsServerOnly)
                _isDoorOpened.Value = isOpened;
            else
                ToggleDoorStateServerRpc(isOpened);
            
            SFXType sfxType = isOpened ? SFXType.DoorOpen : SFXType.DoorClose;
            PlaySound(sfxType).Forget();
        }

        public void ToggleStoppedAtSectorState(bool isStoppedAtSector)
        {
            if (IsOwner)
                _isStoppedAtSector.Value = isStoppedAtSector;
            else
                ToggleStoppedAtSectorStateServerRPC(isStoppedAtSector);
        }

        public void SendOpenQuestsSelectionMenu() =>
            OnOpenQuestsSelectionMenuEvent.Invoke();

        public void SendOpenGameOverWarningMenu() =>
            OnOpenGameOverWarningMenuEvent.Invoke();

        public void SendOpenGameMapEvent() =>
            OnOpenGameMapEvent.Invoke();
        
        public void SendLeaveLocation() =>
            OnLeaveLocationEvent.Invoke();

        public Camera GetOutsideCamera() => _references.OutsideCamera;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void InitAll()
        {
            _trainController.InitAll();

            SoundReproducer = new TrainSoundReproducer(this, _trainConfig);
            PlaySound(SFXType.MovementLoop, onlyLocal: true).Forget();

            IReadOnlyList<TrainSeat> allMobileHQSeats = _references.GetAllMobileHQSeats();

            foreach (TrainSeat trainSeat in allMobileHQSeats)
                trainSeat.ShouldRemovePlayerParentEvent += ShouldRemovePlayerParent;

            _isMainLeverEnabled.OnValueChanged += OnMainLeverStateChanged;

            _isDoorOpened.OnValueChanged += OnDoorStateChanged;

            PlayerEntity.OnPlayerSpawnedEvent += OnPlayerSpawned;
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
            
            _isMainLeverEnabled.OnValueChanged -= OnMainLeverStateChanged;

            _isDoorOpened.OnValueChanged -= OnDoorStateChanged;
            
            PlayerEntity.OnPlayerSpawnedEvent -= OnPlayerSpawned;
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
            }
        }

        private void ChangeToTheMetroPath()
        {
            MetroLocationManager metroLocationManager = MetroLocationManager.Get();
            CinemachinePath path;
            float startPositionAtRoadLocation = 0f;

            if (LastPathID == -1)
            {
                path = metroLocationManager.GetMainPath();
                startPositionAtRoadLocation = _trainConfig.StartPositionAtRoadLocation;
            }
            else
            {
                bool isPathFound = metroLocationManager.TryGetEnterPathByID(LastPathID, out path);

                if (!isPathFound)
                {
                    path = metroLocationManager.GetMainPath();
                    Log.PrintError(log: $"Path with ID <gb>({LastPathID})</gb> <rb>not found</rb>!");
                }
            }

            ChangePath(path, startPositionAtRoadLocation);
        }
        
        private void ToggleMovement(bool canMove) =>
            _pathMovement.ToggleMovement(canMove);

        private void TeleportPlayerToRandomSeat(PlayerEntity playerEntity, bool ignoreChecks = false)
        {
            bool isDead = playerEntity.IsDead();

            if (isDead)
                return;

            bool isInsideTrain = playerEntity.IsInsideTrain;

            if (!isInsideTrain && !ignoreChecks)
                return;

            SeatsRuntimeDataContainer seatsRuntimeDataContainer = _seatsData.Value;
            int seatIndex = seatsRuntimeDataContainer.GetRandomFreeSeatIndex();
            playerEntity.TeleportToTrainSeat(seatIndex);
        }

        private void ToggleSeatsColliders(bool isEnabled)
        {
            IReadOnlyList<TrainSeat> allMobileHQSeats = _references.GetAllMobileHQSeats();

            foreach (TrainSeat mobileHQSeat in allMobileHQSeats)
                mobileHQSeat.ToggleCollider(isEnabled);
        }

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

        private bool ShouldRemovePlayerParent() =>
            _isStoppedAtSector.Value;

        // RPC: -----------------------------------------------------------------------------------

        [Rpc(target: SendTo.Owner)]
        private void TeleportLocalPlayerToRandomSeatRpc(bool ignoreChecks)
        {
            PlayerEntity localPlayer = PlayerEntity.GetLocalPlayer();
            TeleportPlayerToRandomSeat(localPlayer, ignoreChecks);
        }

        [Rpc(target: SendTo.Server)]
        public void StartTrainRpc()
        {
            OnMovementStartedEvent.Invoke();
            
            _pathMovement.ToggleArrived(isArrived: false);
            
            LocationManager locationManager = LocationManager.Get();
            CinemachinePath exitPath = locationManager.GetExitPath();
            ChangePath(exitPath);
            
            _pathMovement.SetMovementType(PathMovement.MovementType.SpeedingUp);
            _pathMovement.SpeedUpTrain();

            IReadOnlyDictionary<ulong, PlayerEntity> allPlayers = PlayerEntity.GetAllPlayers();

            foreach (PlayerEntity playerEntity in allPlayers.Values)
            {
                bool isDead = playerEntity.IsDead();
                bool isInsideTrain = playerEntity.IsInsideTrain;
                bool isPlayerValid = !isDead && isInsideTrain;

                if (!isPlayerValid)
                    continue;
                
                playerEntity.SetTrainAsParent();
            }
        }

        [Rpc(target: SendTo.NotOwner)]
        public void PlayMainLeverPullAnimationRpc()
        {
            TrainMainLever mainLever = _references.MainLever;
            mainLever.InteractWithoutEvents(isLeverPulled: true);
        }

        [ServerRpc(RequireOwnership = false)]
        private void TakeSeatServerRpc(int seatIndex) =>
            _seatsData.Value.TakeSeat(seatIndex);

        [ServerRpc(RequireOwnership = false)]
        private void LeftSeatServerRpc(int seatIndex) =>
            _seatsData.Value.LeftSeat(seatIndex);

        [ServerRpc(RequireOwnership = false)]
        private void ToggleMainLeverStateServerRpc(bool isEnabled) =>
            _isMainLeverEnabled.Value = isEnabled;

        [ServerRpc(RequireOwnership = false)]
        private void ToggleDoorStateServerRpc(bool isOpened) =>
            _isDoorOpened.Value = isOpened;

        [ServerRpc(RequireOwnership = false)]
        private void ToggleStoppedAtSectorStateServerRPC(bool isStoppedAtSector) =>
            _isStoppedAtSector.Value = isStoppedAtSector;

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnDestinationReached()
        {
            switch (_movementBehaviour)
            {
                case MovementBehaviour.InfiniteMovement:
                    _pathMovement.ResetDistance();
                    _pathMovement.ToggleArrived(isArrived: false);
                    break;
                
                case MovementBehaviour.StopAtPathEnd:
                    OnMovementStoppedEvent.Invoke();
                    break;
                
                case MovementBehaviour.LeaveAtPathEnd:
                    SendLeaveLocation();
                    break;
            }
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

        private void OnMainLeverStateChanged(bool previousValue, bool newValue) =>
            _trainController.ToggleMainLeverState(newValue);

        private void OnDoorStateChanged(bool previousValue, bool newValue) =>
            _references.Doors.SetActive(!newValue);

        private async void OnPlayerSpawned(PlayerEntity playerEntity)
        {
            // Чтобы успел поменяться родитель у игрока.
            bool isCanceled = await UniTask
                .DelayFrame(delayFrameCount: 1, cancellationToken: this.GetCancellationTokenOnDestroy())
                .SuppressCancellationThrow();

            if (isCanceled)
                return;

            bool isLocalPlayer = playerEntity.IsLocalPlayer();

            if (!isLocalPlayer)
                return;

            TeleportPlayerToRandomSeat(playerEntity);
        }
    }
}