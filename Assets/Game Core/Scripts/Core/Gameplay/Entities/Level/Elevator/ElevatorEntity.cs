using System;
using GameCore.Configs.Gameplay.Elevator;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Level.Elevator;
using GameCore.Gameplay.Systems.SoundReproducer;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Entities.Level.Elevator
{
    [GenerateSerializationForType(typeof(SFXType))]
    public class ElevatorEntity : SoundProducerEntity<ElevatorEntity.SFXType>
    {
        public enum SFXType
        {
            // _ = 0,
            DoorOpening = 1,
            DoorClosing = 2,
            FloorChange = 3,
            ButtonPush = 4
        }
        
        public enum ElevatorState
        {
            Idle = 0,
            MovingUp = 1,
            MovingDown = 2
        }

        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(
            IElevatorsManagerDecorator elevatorsManagerDecorator,
            IGameplayConfigsProvider gameplayConfigsProvider
        )
        {
            _elevatorsManagerDecorator = elevatorsManagerDecorator;
            _elevatorConfig = gameplayConfigsProvider.GetConfig<ElevatorConfigMeta>();
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [BoxGroup(Constants.References, showLabel: false), SerializeField]
        private ElevatorReferences _references;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action<Floor> OnCurrentFloorChangedEvent = delegate { };
        public event Action<Floor> OnTargetFloorChangedEvent = delegate { };

        private readonly NetworkVariable<Floor> _currentFloor = new(writePerm: OwnerPermission);
        private readonly NetworkVariable<Floor> _targetFloor = new(writePerm: OwnerPermission);
        private readonly NetworkVariable<ElevatorState> _currentState = new(writePerm: OwnerPermission);

        private static ElevatorEntity _instance;

        private IElevatorsManagerDecorator _elevatorsManagerDecorator;
        private ElevatorConfigMeta _elevatorConfig;

        private ElevatorMovementSystem _movementSystem;
        private bool _isOpen;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _instance = this;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void StartElevator() => StartElevatorRpc();

        public void ChangeTargetFloor(Floor floor)
        {
        }

        public static ElevatorEntity Get() => _instance;

        public ElevatorConfigMeta GetElevatorConfig() => _elevatorConfig;

        public ElevatorReferences GetReferences() => _references;

        public Floor GetCurrentFloor() =>
            _currentFloor.Value;

        public Floor GetTargetFloor() =>
            _targetFloor.Value;

        public ElevatorState GetElevatorState() =>
            _currentState.Value;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void InitAll()
        {
            SoundReproducer = new ElevatorSoundReproducer(soundProducer: this, _elevatorConfig);

            _currentFloor.OnValueChanged += OnCurrentFloorChanged;
            _targetFloor.OnValueChanged += OnTargetFloorChanged;
        }

        protected override void InitServer()
        {
            _movementSystem = new ElevatorMovementSystem(elevatorEntity: this);
        }

        protected override void TickServer() =>
            _movementSystem.Tick();

        protected override void DespawnAll()
        {
            _currentFloor.OnValueChanged -= OnCurrentFloorChanged;
            _targetFloor.OnValueChanged -= OnTargetFloorChanged;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void TrySpawnNetworkObject()
        {
            if (IsSpawned)
                return;

            NetworkObject.Spawn();
        }

        // RPC: -----------------------------------------------------------------------------------

        [Rpc(target: SendTo.Owner)]
        private void StartElevatorRpc()
        {
            ElevatorState elevatorState = GetElevatorState();
            bool isStateValid = elevatorState == ElevatorState.Idle;

            if (!isStateValid)
                return;

            Floor targetFloor = GetTargetFloor();
            Floor currentFloor = GetCurrentFloor();
            bool isFloorValid = targetFloor != currentFloor;

            if (!isFloorValid)
                return;
            
            _references.NetworkAnimator.SetTrigger(hash: AnimatorHashes.Close);
        }
        
        [Rpc(target: SendTo.Owner)]
        private void ChangeTargetFloorRpc(Floor floor) =>
            _targetFloor.Value = floor;

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnCurrentFloorChanged(Floor previousValue, Floor newValue) =>
            OnCurrentFloorChangedEvent.Invoke(newValue);
        
        private void OnTargetFloorChanged(Floor previousValue, Floor newValue) =>
            OnTargetFloorChangedEvent.Invoke(newValue);
    }
}