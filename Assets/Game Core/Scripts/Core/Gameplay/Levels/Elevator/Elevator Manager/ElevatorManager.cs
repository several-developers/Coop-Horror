using System;
using System.Collections;
using GameCore.Configs.Gameplay.Elevator;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Network.Other;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.Levels.Elevator
{
    public class ElevatorManager : NetworkBehaviour
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action<ElevatorStaticData> OnElevatorStartedEvent = delegate { };
        public event Action<ElevatorStaticData> OnFloorChangedEvent = delegate { };
        public event Action<ElevatorFloor> OnElevatorStoppedEvent = delegate { };

        private const NetworkVariableWritePermission Owner = NetworkVariableWritePermission.Owner;

        private readonly NetworkVariable<ElevatorFloor> _currentFloor = new(writePerm: Owner);
        private readonly NetworkVariable<ElevatorFloor> _targetFloor = new(writePerm: Owner);
        private readonly NetworkVariable<bool> _isElevatorMoving = new(writePerm: Owner);

        private static ElevatorManager _instance;

        private ElevatorConfigMeta _elevatorConfig;
        private RpcCaller _rpcCaller;
        private Coroutine _movementCO;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _instance = this;

        public override void OnNetworkSpawn()
        {
            Init();
            base.OnNetworkSpawn();
        }

        public override void OnNetworkDespawn()
        {
            Despawn();
            base.OnNetworkDespawn();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public ElevatorFloor GetCurrentFloor() =>
            _currentFloor.Value;

        public bool IsElevatorMoving() =>
            _isElevatorMoving.Value;

        public static ElevatorManager Get() => _instance;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void Init()
        {
            InitServerAndClient();
            InitServer();
            InitClient();
        }

        private void InitServerAndClient()
        {
            GetElevatorConfig();
            
            _rpcCaller = RpcCaller.Get();
            _rpcCaller.OnStartElevatorEvent += OnStartElevator;
        }

        private void InitServer()
        {
            if (!IsOwner)
                return;

            _currentFloor.OnValueChanged += OnCurrentFloorChanged;
        }

        private void InitClient()
        {
            if (IsOwner)
                return;
        }

        private void Despawn()
        {
            DespawnServer();

            _rpcCaller.OnStartElevatorEvent -= OnStartElevator;
        }

        private void DespawnServer()
        {
            if (!IsOwner)
                return;

            _currentFloor.OnValueChanged -= OnCurrentFloorChanged;
        }

        private void GetElevatorConfig()
        {
            NetworkServiceLocator networkServiceLocator = NetworkServiceLocator.Get();
            IGameplayConfigsProvider gameplayConfigsProvider = networkServiceLocator.GetGameplayConfigsProvider();
            _elevatorConfig = gameplayConfigsProvider.GetElevatorConfig();
        }

        private void TryStartElevator(ElevatorFloor elevatorFloor)
        {
            if (_isElevatorMoving.Value)
                return;

            if (elevatorFloor == _currentFloor.Value)
                return;

            StartElevator(elevatorFloor);
        }

        private void StartElevator(ElevatorFloor elevatorFloor)
        {
            _targetFloor.Value = elevatorFloor;
            _isElevatorMoving.Value = true;

            _movementCO = StartCoroutine(routine: ElevatorMovementCO());
        }
        
        private void StopElevator()
        {
            if (_movementCO != null)
                StopCoroutine(_movementCO);

            _isElevatorMoving.Value = false;
            OnElevatorStoppedEvent.Invoke(_currentFloor.Value);
        }

        private IEnumerator ElevatorMovementCO()
        {
            float delay;
            bool isMovingUp = IsMovingUp();

            ElevatorStaticData data = new(_currentFloor.Value, _targetFloor.Value, isMovingUp);
            OnElevatorStartedEvent.Invoke(data);

            while (_currentFloor.Value != _targetFloor.Value)
            {
                delay = _elevatorConfig.FloorMovementDuration;

                yield return new WaitForSeconds(delay);

                int newFloorInt = (int)_currentFloor.Value;
                newFloorInt += isMovingUp ? -1 : 1;

                var newFloor = (ElevatorFloor)newFloorInt;
                _currentFloor.Value = newFloor;
            }

            delay = _elevatorConfig.ReactivationDelay;

            yield return new WaitForSeconds(delay);
            
            StopElevator();
        }

        private bool IsMovingUp()
        {
            int currentFloor = (int)_currentFloor.Value;
            int targetFloor = (int)_targetFloor.Value;
            bool isMovingUp = targetFloor < currentFloor;
            return isMovingUp;
        }
        
        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnStartElevator(ElevatorFloor elevatorFloor) => TryStartElevator(elevatorFloor);

        private void OnCurrentFloorChanged(ElevatorFloor previousValue, ElevatorFloor newValue)
        {
            bool isMovingUp = IsMovingUp();
            ElevatorStaticData data = new(currentFloor: newValue, _targetFloor.Value, isMovingUp);

            OnFloorChangedEvent.Invoke(data);
        }
    }
}