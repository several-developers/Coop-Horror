using System;
using System.Collections.Generic;
using GameCore.Configs.Gameplay.Elevator;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Items;
using GameCore.Gameplay.Level;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Systems.SoundReproducer;
using GameCore.Gameplay.VisualManagement;
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
            ILevelProvider levelProvider,
            IVisualManager visualManager,
            IGameplayConfigsProvider gameplayConfigsProvider
        )
        {
            _levelProvider = levelProvider;
            _visualManager = visualManager;
            _elevatorConfig = gameplayConfigsProvider.GetConfig<ElevatorConfigMeta>();
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [BoxGroup(Constants.References, showLabel: false), SerializeField]
        private ElevatorReferences _references;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action<Floor> OnCurrentFloorChangedEvent = delegate { };
        public event Action<Floor> OnTargetFloorChangedEvent = delegate { };

        private readonly NetworkVariable<Floor> _currentElevatorFloor = new(writePerm: OwnerPermission);
        private readonly NetworkVariable<Floor> _targetElevatorFloor = new(writePerm: OwnerPermission);
        private readonly NetworkVariable<ElevatorState> _currentState = new(writePerm: OwnerPermission);

        private static ElevatorEntity _instance;

        private ILevelProvider _levelProvider;
        private IVisualManager _visualManager;
        private ElevatorConfigMeta _elevatorConfig;

        private ElevatorMovementSystem _movementSystem;
        private bool _isOpen;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _instance = this;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void StartElevator()
        {
            Floor targetFloor = GetTargetElevatorFloor();
            StartElevatorRpc(targetFloor);
        }

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

            _currentElevatorFloor.Value = Floor.Surface;
            _targetElevatorFloor.Value = Floor.Surface;
            _currentState.Value = ElevatorState.Idle;
        }

        public static ElevatorEntity Get() => _instance;

        public ElevatorConfigMeta GetElevatorConfig() => _elevatorConfig;

        public ElevatorReferences GetReferences() => _references;

        public Floor GetCurrentElevatorFloor() =>
            _currentElevatorFloor.Value;

        public Floor GetTargetElevatorFloor() =>
            _targetElevatorFloor.Value;

        public ElevatorState GetElevatorState() =>
            _currentState.Value;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void InitAll()
        {
            SoundReproducer = new ElevatorSoundReproducer(soundProducer: this, _elevatorConfig);

            _currentElevatorFloor.OnValueChanged += OnCurrentFloorChanged;
            _targetElevatorFloor.OnValueChanged += OnTargetFloorChanged;
        }

        protected override void InitServerOnly()
        {
            _movementSystem = new ElevatorMovementSystem(elevatorEntity: this, _levelProvider, _visualManager);

            _movementSystem.OnElevatorStoppedEvent += OnElevatorStopped;
            _movementSystem.OnElevatorFloorChangedEvent += OnElevatorFloorChanged;

            _references.AnimationObserver.OnDoorClosedEvent += OnDoorClosed;

            _references.ElevatorTrigger.OnTargetLeftTriggerEvent += OnTargetLeftWhileElevatorMoving;
        }

        protected override void DespawnAll()
        {
            _currentElevatorFloor.OnValueChanged -= OnCurrentFloorChanged;
            _targetElevatorFloor.OnValueChanged -= OnTargetFloorChanged;
        }

        protected override void DespawnServerOnly()
        {
            _movementSystem.OnElevatorStoppedEvent -= OnElevatorStopped;
            _movementSystem.OnElevatorFloorChangedEvent -= OnElevatorFloorChanged;

            _references.AnimationObserver.OnDoorClosedEvent -= OnDoorClosed;
            
            _references.ElevatorTrigger.OnTargetLeftTriggerEvent -= OnTargetLeftWhileElevatorMoving;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void HandleParentForEntities(bool removeParent)
        {
            ElevatorTrigger elevatorTrigger = _references.ElevatorTrigger;
            IEnumerable<IReParentable> insideTargetsList = elevatorTrigger.GetInsideTargetsList();

            foreach (IReParentable target in insideTargetsList)
            {
                if (target is ElevatorEntity)
                    continue;

                if (removeParent)
                    target.RemoveParent();
                else
                    target.SetParent(NetworkObject);
            }
        }

        private void HandleFloorChangeForEntities()
        {
            Floor currentFloor = GetCurrentElevatorFloor();
            
            EntityLocation entityLocation = currentFloor == Floor.Surface
                ? EntityLocation.Surface
                : EntityLocation.Dungeon;

            ElevatorTrigger elevatorTrigger = _references.ElevatorTrigger;
            IEnumerable<IReParentable> insideEntitiesList = elevatorTrigger.GetInsideTargetsList();

            foreach (IReParentable target in insideEntitiesList)
            {
                if (target is not Entity entity)
                    continue;
                
                entity.SetEntityLocation(entityLocation);
                entity.SetFloor(currentFloor);

                ChangeVisualPresetForPlayer(entity);
            }

            // LOCAL METHODS: -----------------------------

            void ChangeVisualPresetForPlayer(Entity entity)
            {
                if (entity is not PlayerEntity playerEntity)
                    return;

                ulong clientID = playerEntity.OwnerClientId;
                _visualManager.ChangePresetByFloor(currentFloor, clientID);
            }
        }

        private void PlayOpenAnimation() =>
            _references.NetworkAnimator.SetTrigger(hash: AnimatorHashes.Open);

        private void PlayCloseAnimation() =>
            _references.NetworkAnimator.SetTrigger(hash: AnimatorHashes.Close);

        // RPC: -----------------------------------------------------------------------------------
        
        [Rpc(target: SendTo.Owner)]
        private void StartElevatorRpc(Floor targetFloor)
        {
            ElevatorState elevatorState = GetElevatorState();
            bool isStateValid = elevatorState == ElevatorState.Idle;

            if (!isStateValid)
                return;

            _targetElevatorFloor.Value = targetFloor;
            Floor currentFloor = GetCurrentElevatorFloor();
            bool isFloorsMatches = targetFloor == currentFloor;

            if (isFloorsMatches)
                return;

            Close();
            _movementSystem.StartMovement();
            
            _currentState.Value = _movementSystem.GetElevatorState();
        }

        [Rpc(target: SendTo.Owner)]
        private void SetCurrentFloorRpc(Floor floor) =>
            _currentElevatorFloor.Value = floor;

        [Rpc(target: SendTo.Owner)]
        private void SetTargetFloorRpc(Floor floor) =>
            _targetElevatorFloor.Value = floor;

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnCurrentFloorChanged(Floor previousValue, Floor newValue) =>
            OnCurrentFloorChangedEvent.Invoke(newValue);

        private void OnTargetFloorChanged(Floor previousValue, Floor newValue) =>
            OnTargetFloorChangedEvent.Invoke(newValue);

        private void OnElevatorStopped()
        {
            _currentState.Value = ElevatorState.Idle;
            
            HandleParentForEntities(removeParent: true);
            Open();
        }

        private void OnElevatorFloorChanged() => HandleFloorChangeForEntities();

        private void OnDoorClosed() => HandleParentForEntities(removeParent: false);

        private void OnTargetLeftWhileElevatorMoving(IReParentable target)
        {
            ElevatorState currentState = GetElevatorState();
            bool isStateValid = currentState != ElevatorState.Idle;
            
            if (!isStateValid)
                return;
            
            target.RemoveParent();
        }

        // DEBUG BUTTONS: -------------------------------------------------------------------------

        [Title(Constants.DebugButtons)]
        [Button(buttonSize: 30), DisableInEditorMode]
        private void DebugOpen() => Open();

        [Button(buttonSize: 30), DisableInEditorMode]
        private void DebugClose() => Close();
    }
}