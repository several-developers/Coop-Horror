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
    public interface IElevatorsManager
    {
        event Action<ElevatorStaticData> OnElevatorStartedEvent;
        event Action<ElevatorStaticData> OnFloorChangedEvent;
        event Action<Floor> OnElevatorStoppedEvent;
        Floor GetCurrentFloor();
        bool IsElevatorMoving();
    }
    
    public class ElevatorsManager : NetworkBehaviour, IElevatorsManager
    {
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<ElevatorStaticData> OnElevatorStartedEvent = delegate { };
        public event Action<ElevatorStaticData> OnFloorChangedEvent = delegate { };
        public event Action<Floor> OnElevatorStoppedEvent = delegate { };

        private readonly NetworkVariable<Floor> _currentFloor = new();
        private readonly NetworkVariable<Floor> _targetFloor = new();
        private readonly NetworkVariable<bool> _isElevatorMoving = new();

        private static ElevatorsManager _instance;

        private ElevatorConfigMeta _elevatorConfig;
        private RpcCaller _rpcCaller;
        private Coroutine _movementCO;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _instance = this;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Init();
        }

        public override void OnNetworkDespawn()
        {
            Despawn();
            base.OnNetworkDespawn();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public Floor GetCurrentFloor() =>
            _currentFloor.Value;

        public bool IsElevatorMoving() =>
            _isElevatorMoving.Value;

        public static ElevatorsManager Get() => _instance;

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

        private void TryStartElevator(Floor floor)
        {
            if (_isElevatorMoving.Value)
                return;

            if (floor == _currentFloor.Value)
                return;

            StartElevatorOnServer(floor);
        }

        private void StartElevatorOnServer(Floor floor)
        {
            if (!IsOwner)
                return;

            _targetFloor.Value = floor;
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

        private void SendElevatorStartedEvent(Floor targetFloor)
        {
            bool isMovingUp = IsMovingUp(targetFloor);
            ElevatorStaticData data = new(_currentFloor.Value, targetFloor, isMovingUp);
            
            OnElevatorStartedEvent.Invoke(data);
        }

        private void SendElevatorStoppedEvent() =>
            OnElevatorStoppedEvent.Invoke(_currentFloor.Value);

        private IEnumerator ElevatorMovementCO()
        {
            Floor targetFloor = _targetFloor.Value;
            
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

                var newFloor = (Floor)newFloorInt;
                _currentFloor.Value = newFloor;
            }

            delay = _elevatorConfig.ReactivationDelay;

            yield return new WaitForSeconds(delay);

            StopElevator();
        }

        private bool IsMovingUp(Floor targetFloor)
        {
            int currentFloorInt = (int)_currentFloor.Value;
            int targetFloorInt = (int)targetFloor;
            bool isMovingUp = targetFloorInt < currentFloorInt;
            return isMovingUp;
        }

        // RPC: -----------------------------------------------------------------------------------

        [ServerRpc(RequireOwnership = false)]
        private void SendElevatorStartedServerRpc(Floor targetFloor) =>
            SendElevatorStartedClientRpc(targetFloor);

        [ServerRpc(RequireOwnership = false)]
        private void SendElevatorStoppedServerRpc() => SendElevatorStoppedClientRpc();

        [ClientRpc]
        private void SendElevatorStartedClientRpc(Floor targetFloor)
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

        private void OnStartElevator(Floor floor) => TryStartElevator(floor);

        private void OnCurrentFloorChanged(Floor previousValue, Floor newValue)
        {
            bool isMovingUp = IsMovingUp(_targetFloor.Value);
            ElevatorStaticData data = new(currentFloor: newValue, _targetFloor.Value, isMovingUp);
            
            OnFloorChangedEvent.Invoke(data);
        }
    }
}