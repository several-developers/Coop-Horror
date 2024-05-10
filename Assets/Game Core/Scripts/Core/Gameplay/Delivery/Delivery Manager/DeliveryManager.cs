using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.MobileHeadquarters;
using GameCore.Gameplay.Network;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Delivery
{
    public class DeliveryManager : NetcodeBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IDeliveryManagerDecorator deliveryManagerDecorator, IDeliveryPoint deliveryPoint,
            IMobileHeadquartersEntity mobileHeadquartersEntity)
        {
            _deliveryManagerDecorator = deliveryManagerDecorator;
            _deliveryPoint = deliveryPoint;
            _mobileHeadquartersEntity = mobileHeadquartersEntity;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly NetworkVariable<DroneState> _droneState = new();

        private IDeliveryManagerDecorator _deliveryManagerDecorator;
        private IDeliveryPoint _deliveryPoint;
        private IMobileHeadquartersEntity _mobileHeadquartersEntity;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _deliveryManagerDecorator.OnResetTakeOffTimerInnerEvent += OnResetTakeOffTimer;
            _deliveryManagerDecorator.OnGetDroneStateInnerEvent += GetDroneState;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            _deliveryManagerDecorator.OnResetTakeOffTimerInnerEvent -= OnResetTakeOffTimer;
            _deliveryManagerDecorator.OnGetDroneStateInnerEvent -= GetDroneState;
        }

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void InitAll()
        {
            _droneState.OnValueChanged += OnDroneStateChanged;
            
            _mobileHeadquartersEntity.OnCallDeliveryDroneEvent += OnCallDeliveryDrone;
        }

        protected override void InitServerOnly()
        {
            _droneState.OnValueChanged += OnTryStartTakeOffTimer;
            
            _deliveryPoint.OnDroneTakeOffTimerFinishedEvent += OnDroneTakeOffTimerFinished;
            _deliveryPoint.OnDroneStateChangedEvent += ChangeDroneState;
        }

        protected override void DespawnAll()
        {
            _droneState.OnValueChanged -= OnDroneStateChanged;
            
            _mobileHeadquartersEntity.OnCallDeliveryDroneEvent -= OnCallDeliveryDrone;
        }

        protected override void DespawnServerOnly()
        {
            _droneState.OnValueChanged -= OnTryStartTakeOffTimer;
            
            _deliveryPoint.OnDroneTakeOffTimerFinishedEvent -= OnDroneTakeOffTimerFinished;
            _deliveryPoint.OnDroneStateChangedEvent -= ChangeDroneState;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ChangeDroneState(DroneState droneState) =>
            _droneState.Value = droneState;

        private void ResetTakeOffTimer() =>
            _deliveryPoint.ResetTakeOffTimer();

        private DroneState GetDroneState() =>
            _droneState.Value;
        
        // RPC: -----------------------------------------------------------------------------------

        [ServerRpc(RequireOwnership = false)]
        private void CallDeliveryDroneServerRpc()
        {
            bool canCallDeliveryDrone = _droneState.Value == DroneState.WaitingForCall;
            
            if (!canCallDeliveryDrone)
                return;

            _deliveryPoint.LandDrone();
            _mobileHeadquartersEntity.OpenDoor();
            ShowDeliveryPointServerRpc();
        }

        [ServerRpc]
        private void ShowDeliveryPointServerRpc() => ShowDeliveryPointClientRpc();

        [ServerRpc]
        private void HideDeliveryPointServerRpc() => HideDeliveryPointClientRpc();
        
        [ServerRpc(RequireOwnership = false)]
        private void ResetTakeOffTimerServerRpc() => ResetTakeOffTimer();
        
        [ClientRpc]
        private void ShowDeliveryPointClientRpc() =>
            _deliveryPoint.ShowPoint();

        [ClientRpc]
        private void HideDeliveryPointClientRpc() =>
            _deliveryPoint.HidePoint();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnTryStartTakeOffTimer(DroneState previousValue, DroneState newValue)
        {
            if (newValue != DroneState.Landed)
                return;

            _deliveryPoint.StartTakeOffTimer();
        }
        
        private void OnResetTakeOffTimer()
        {
            if (IsServerOnly)
                ResetTakeOffTimer();
            else
                ResetTakeOffTimerServerRpc();
        }

        private void OnDroneTakeOffTimerFinished() => HideDeliveryPointServerRpc();

        private void OnDroneStateChanged(DroneState previousValue, DroneState newValue) =>
            _deliveryManagerDecorator.DroneStateChanged(droneState: newValue);

        private void OnCallDeliveryDrone() => CallDeliveryDroneServerRpc();
    }
}