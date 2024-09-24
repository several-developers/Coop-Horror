using System;
using System.Collections.Generic;
using GameCore.Configs.Gameplay.Elevator;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Level;
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
        private void Construct(ILevelProvider levelProvider, IGameplayConfigsProvider gameplayConfigsProvider)
        {
            _levelProvider = levelProvider;
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

        private ILevelProvider _levelProvider;
        private ElevatorConfigMeta _elevatorConfig;
        private ElevatorMovementSystem _movementSystem;
        private bool _isOpen;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _instance = this;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void StartElevator() => StartElevatorRpc();

        public void StartElevator(Floor targetFloor) => StartElevatorRpc(targetFloor);

        public void SetCurrentFloor(Floor floor) => SetCurrentFloorRpc(floor);
        
        public void SetTargetFloor(Floor floor) => SetTargetFloorRpc(floor);

        public void Open()
        {
            PlayOpenAnimation();
        }

        public void Close()
        {
            PlayCloseAnimation();
        }

        public void ResetElevator()
        {
            if (!IsOwner)
                return;

            _currentFloor.Value = Floor.Surface;
            _targetFloor.Value = Floor.Surface;
            _currentState.Value = ElevatorState.Idle;
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

        protected override void InitServerOnly()
        {
            _movementSystem = new ElevatorMovementSystem(elevatorEntity: this, _levelProvider);

            _movementSystem.OnElevatorStoppedEvent += OnElevatorStopped;
            
            _references.AnimationObserver.OnDoorClosedEvent += OnDoorClosed;
        }

        protected override void DespawnAll()
        {
            _currentFloor.OnValueChanged -= OnCurrentFloorChanged;
            _targetFloor.OnValueChanged -= OnTargetFloorChanged;
        }

        protected override void DespawnServerOnly()
        {
            _movementSystem.OnElevatorStoppedEvent -= OnElevatorStopped;
            
            _references.AnimationObserver.OnDoorClosedEvent -= OnDoorClosed;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SetElevatorAsParentForAllEntitiesInside()
        {
            ElevatorTrigger elevatorTrigger = _references.ElevatorTrigger;
            IEnumerable<Entity> insideEntitiesList = elevatorTrigger.GetInsideEntitiesList();

            foreach (Entity entity in insideEntitiesList)
            {
                if (entity == this)
                    continue;
                
                entity.SetParent(NetworkObject);
            }
        }

        private void RemoveParentForAllEntitiesInside()
        {
            ElevatorTrigger elevatorTrigger = _references.ElevatorTrigger;
            IEnumerable<Entity> insideEntitiesList = elevatorTrigger.GetInsideEntitiesList();

            foreach (Entity entity in insideEntitiesList)
            {
                if (entity == this)
                    continue;
                
                entity.RemoveParent();
            }
        }
        
        private void PlayOpenAnimation() =>
            _references.NetworkAnimator.SetTrigger(hash: AnimatorHashes.Open);

        private void PlayCloseAnimation() =>
            _references.NetworkAnimator.SetTrigger(hash: AnimatorHashes.Close);
        
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
            bool isFloorsMatches = targetFloor == currentFloor;

            if (isFloorsMatches)
                return;
            
            Close();
            _movementSystem.StartMovement();
        }
        
        [Rpc(target: SendTo.Owner)]
        private void StartElevatorRpc(Floor targetFloor)
        {
            ElevatorState elevatorState = GetElevatorState();
            bool isStateValid = elevatorState == ElevatorState.Idle;

            if (!isStateValid)
                return;

            _targetFloor.Value = targetFloor;
            Floor currentFloor = GetCurrentFloor();
            bool isFloorsMatches = targetFloor == currentFloor;

            if (isFloorsMatches)
                return;
                
            Close();
            _movementSystem.StartMovement();
        }
        
        [Rpc(target: SendTo.Owner)]
        private void SetCurrentFloorRpc(Floor floor) =>
            _currentFloor.Value = floor;
        
        [Rpc(target: SendTo.Owner)]
        private void SetTargetFloorRpc(Floor floor) =>
            _targetFloor.Value = floor;

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnCurrentFloorChanged(Floor previousValue, Floor newValue) =>
            OnCurrentFloorChangedEvent.Invoke(newValue);
        
        private void OnTargetFloorChanged(Floor previousValue, Floor newValue) =>
            OnTargetFloorChangedEvent.Invoke(newValue);

        private void OnElevatorStopped()
        {
            RemoveParentForAllEntitiesInside();
            Open();
        }
        
        private void OnDoorClosed() => SetElevatorAsParentForAllEntitiesInside();
    }
}