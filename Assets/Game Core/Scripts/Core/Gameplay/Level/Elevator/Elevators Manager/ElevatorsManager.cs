using System.Collections;
using GameCore.Configs.Gameplay.Elevator;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Level.Elevator;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.VisualManagement;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using GameCore.Utilities;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Level.Elevator
{
    public class ElevatorsManager : NetcodeBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(
            IVisualManager visualManager,
            ILevelProvider levelProvider,
            IGameplayConfigsProvider gameplayConfigsProvider
        )
        {
            _visualManager = visualManager;
            _levelProvider = levelProvider;
            _elevatorConfig = gameplayConfigsProvider.GetConfig<ElevatorConfigMeta>();
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly NetworkVariable<Floor> _currentFloor = new();
        private readonly NetworkVariable<Floor> _targetFloor = new();
        private readonly NetworkVariable<bool> _isElevatorMoving = new();

        private IVisualManager _visualManager;
        private ILevelProvider _levelProvider;
        private ElevatorConfigMeta _elevatorConfig;
        private Coroutine _movementCO;
        
        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void InitAll() =>
            _currentFloor.OnValueChanged += OnCurrentFloorChanged;

        protected override void DespawnAll() =>
            _currentFloor.OnValueChanged -= OnCurrentFloorChanged;

        // PRIVATE METHODS: -----------------------------------------------------------------------

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
           
        }

        private void SendElevatorStoppedEvent()
        {
        }

        private IEnumerator ElevatorMovementCO()
        {
            Floor targetFloor = _targetFloor.Value;

            SendElevatorStartedEvent(targetFloor);
            SendElevatorStartedServerRpc(targetFloor);

            float delay;
            bool isMovingUp = IsMovingUp(targetFloor);

            while (_currentFloor.Value != _targetFloor.Value)
            {
                delay = _elevatorConfig.MovementDurationPerFloor;

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

        private Floor GetCurrentFloor() =>
            _currentFloor.Value;

        private bool IsElevatorMoving() =>
            _isElevatorMoving.Value;

        private bool IsMovingUp(Floor targetFloor)
        {
            int currentFloorInt = (int)_currentFloor.Value;
            int targetFloorInt = (int)targetFloor;
            bool isMovingUp = targetFloorInt < currentFloorInt;
            return isMovingUp;
        }

        // RPC: -----------------------------------------------------------------------------------

        [ServerRpc(RequireOwnership = false)]
        private void StartElevatorServerRpc(Floor floor) => StartElevatorClientRpc(floor);

        [ServerRpc(RequireOwnership = false)]
        private void SendElevatorStartedServerRpc(Floor targetFloor) =>
            SendElevatorStartedClientRpc(targetFloor);

        [ServerRpc(RequireOwnership = false)]
        private void SendElevatorStoppedServerRpc() => SendElevatorStoppedClientRpc();

        [ServerRpc(RequireOwnership = false)]
        private void OpenElevatorServerRpc(Floor floor) => OpenElevatorClientRpc(floor);

        [ClientRpc]
        private void StartElevatorClientRpc(Floor floor) => TryStartElevator(floor);

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

        [ClientRpc]
        private void OpenElevatorClientRpc(Floor floor)
        {
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnTeleportLocalPlayer(Transform elevatorTransform)
        {
            Floor currentFloor = GetCurrentFloor();
            ElevatorEntity elevatorEntity = ElevatorEntity.Get();

            PlayerEntity playerEntity = PlayerEntity.GetLocalPlayer();
            Transform target = playerEntity.transform;
            Transform parentFrom = elevatorTransform;
            Transform parentTo = elevatorEntity.transform;

            GameUtilities.Teleport(target, parentFrom, parentTo, out Vector3 position, out Quaternion rotation);
            playerEntity.Teleport(position, rotation);

            EntityLocation playerLocation = currentFloor == Floor.Surface
                ? EntityLocation.Surface
                : EntityLocation.Dungeon;
            
            VisualPresetType presetType = currentFloor == Floor.Surface
                ? VisualPresetType.ForestLocation
                : VisualPresetType.Dungeon;
            
            playerEntity.SetEntityLocation(playerLocation);
            playerEntity.SetFloor(currentFloor);
            _visualManager.ChangePreset(presetType);
        }

        private void OnCurrentFloorChanged(Floor previousValue, Floor newValue)
        {
        }
    }
}