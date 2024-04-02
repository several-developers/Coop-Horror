using System.Collections;
using Cysharp.Threading.Tasks;
using GameCore.Configs.Gameplay.Elevator;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Network;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Levels.Elevator
{
    public class ElevatorsManager : NetworkBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IElevatorsManagerDecorator elevatorsManagerDecorator,
            IGameplayConfigsProvider gameplayConfigsProvider)
        {
            _elevatorsManagerDecorator = elevatorsManagerDecorator;
            _elevatorConfig = gameplayConfigsProvider.GetElevatorConfig();
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly NetworkVariable<Floor> _currentFloor = new();
        private readonly NetworkVariable<Floor> _targetFloor = new();
        private readonly NetworkVariable<bool> _isElevatorMoving = new();

        private IElevatorsManagerDecorator _elevatorsManagerDecorator;
        private ElevatorConfigMeta _elevatorConfig;
        private RpcCaller _rpcCaller;
        private Coroutine _movementCO;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _elevatorsManagerDecorator.OnGetCurrentFloorEvent += GetCurrentFloor;
            _elevatorsManagerDecorator.OnIsElevatorMovingEvent += IsElevatorMoving;
        }

        public override void OnDestroy()
        {
            _elevatorsManagerDecorator.OnGetCurrentFloorEvent -= GetCurrentFloor;
            _elevatorsManagerDecorator.OnIsElevatorMovingEvent -= IsElevatorMoving;
            
            base.OnDestroy();
        }

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

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void Init()
        {
            InitServerAndClient();
            InitServer();
            InitClient();
        }

        private async void InitServerAndClient()
        {
            _currentFloor.OnValueChanged += OnCurrentFloorChanged;

            await UniTask.DelayFrame(1);
            
            _rpcCaller = RpcCaller.Get();

            _rpcCaller.OnStartElevatorEvent += OnStartElevator;

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
            
            _elevatorsManagerDecorator.ElevatorStarted(data);
        }

        private void SendElevatorStoppedEvent() =>
            _elevatorsManagerDecorator.ElevatorStopped(_currentFloor.Value);

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
            
            _elevatorsManagerDecorator.FloorChanged(data);
        }
    }
}