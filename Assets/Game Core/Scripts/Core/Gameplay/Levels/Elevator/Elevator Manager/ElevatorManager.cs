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

        private readonly NetworkVariable<ElevatorFloor> _currentFloor = new();
        private readonly NetworkVariable<ElevatorFloor> _targetFloor = new();
        private readonly NetworkVariable<bool> _isElevatorMoving = new();

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

            _currentFloor.OnValueChanged += OnCurrentFloorChanged;
        }

        private void InitServer()
        {
            if (!IsOwner)
                return;
        }

        private void InitClient()
        {
            if (IsOwner)
                return;
        }

        private void Despawn()
        {
            DespawnServerAndClient();
            DespawnServer();
            DespawnClient();
        }

        private void DespawnServerAndClient()
        {
            _currentFloor.OnValueChanged -= OnCurrentFloorChanged;
        }

        private void DespawnServer()
        {
            if (!IsOwner)
                return;

            _rpcCaller.OnStartElevatorEvent -= OnStartElevator;
        }

        private void DespawnClient()
        {
            if (IsOwner)
                return;
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

            StartElevatorOnServer(elevatorFloor);
        }

        private void StartElevatorOnServer(ElevatorFloor elevatorFloor)
        {
            if (!IsOwner)
                return;

            _targetFloor.Value = elevatorFloor;
            _isElevatorMoving.Value = true;

            _movementCO = StartCoroutine(routine: ElevatorMovementCO());
        }

        private void StopElevator()
        {
            if (_movementCO != null)
                StopCoroutine(_movementCO);

            _isElevatorMoving.Value = false;

            SendElevatorStoppedEvent();
            SendElevatorStoppedServerRpc();
        }

        private void SendElevatorStartedEvent(ElevatorFloor targetFloor)
        {
            bool isMovingUp = IsMovingUp(targetFloor);
            ElevatorStaticData data = new(_currentFloor.Value, targetFloor, isMovingUp);
            
            OnElevatorStartedEvent.Invoke(data);
        }

        private void SendElevatorStoppedEvent() =>
            OnElevatorStoppedEvent.Invoke(_currentFloor.Value);

        private IEnumerator ElevatorMovementCO()
        {
            ElevatorFloor targetFloor = _targetFloor.Value;
            
            SendElevatorStartedEvent(targetFloor);
            SendElevatorStartedServerRpc(targetFloor);
            
            float delay;
            bool isMovingUp = IsMovingUp(targetFloor);

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

        private bool IsMovingUp(ElevatorFloor targetFloor)
        {
            int currentFloorInt = (int)_currentFloor.Value;
            int targetFloorInt = (int)targetFloor;
            bool isMovingUp = targetFloorInt < currentFloorInt;
            return isMovingUp;
        }

        // RPC: -----------------------------------------------------------------------------------

        [ServerRpc(RequireOwnership = false)]
        private void SendElevatorStartedServerRpc(ElevatorFloor targetFloor) =>
            SendElevatorStartedClientRpc(targetFloor);

        [ServerRpc(RequireOwnership = false)]
        private void SendElevatorStoppedServerRpc() => SendElevatorStoppedClientRpc();

        [ClientRpc]
        private void SendElevatorStartedClientRpc(ElevatorFloor targetFloor)
        {
            if (IsOwner)
                return;

            SendElevatorStartedEvent(targetFloor);
        }

        [ClientRpc]
        private void SendElevatorStoppedClientRpc()
        {
            if (IsOwner)
                return;

            SendElevatorStoppedEvent();
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnStartElevator(ElevatorFloor elevatorFloor) => TryStartElevator(elevatorFloor);

        private void OnCurrentFloorChanged(ElevatorFloor previousValue, ElevatorFloor newValue)
        {
            bool isMovingUp = IsMovingUp(_targetFloor.Value);
            ElevatorStaticData data = new(currentFloor: newValue, _targetFloor.Value, isMovingUp);
            
            OnFloorChangedEvent.Invoke(data);
        }
    }
}