using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.MobileHeadquarters;
using GameCore.Gameplay.Network.Utilities;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Delivery
{
    public class DeliveryManager : NetworkBehaviour, INetcodeInitBehaviour, INetcodeDespawnBehaviour
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

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void InitServerAndClient()
        {
            _droneState.OnValueChanged += OnDroneStateChanged;
            
            _mobileHeadquartersEntity.OnCallDeliveryDroneEvent += OnCallDeliveryDrone;
        }

        public void InitServer()
        {
            if (!IsOwner)
                return;

            _deliveryPoint.OnDroneTakeOffTimerFinishedEvent += OnDroneTakeOffTimerFinished;
            _deliveryPoint.OnDroneStateChangedEvent += ChangeDroneState;
        }

        public void InitClient()
        {
            if (IsOwner)
                return;
        }

        public void DespawnServerAndClient()
        {
            _droneState.OnValueChanged -= OnDroneStateChanged;
            
            _mobileHeadquartersEntity.OnCallDeliveryDroneEvent -= OnCallDeliveryDrone;
        }

        public void DespawnServer()
        {
            if (!IsOwner)
                return;

            _deliveryPoint.OnDroneTakeOffTimerFinishedEvent -= OnDroneTakeOffTimerFinished;
            _deliveryPoint.OnDroneStateChangedEvent -= ChangeDroneState;
        }

        public void DespawnClient()
        {
            if (IsOwner)
                return;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ChangeDroneState(DroneState droneState) =>
            _droneState.Value = droneState;

        private void HandleDroneStateServerLogic(DroneState droneState)
        {
            switch (droneState)
            {
                case DroneState.Landed:
                    _deliveryPoint.StartTakeOffTimer();
                    break;
            }
        }
        
        private void ResetTakeOffTimer()
        {
            _deliveryPoint.ResetTakeOffTimer();
        }

        private DroneState GetDroneState() =>
            _droneState.Value;
        
        // RPC: -----------------------------------------------------------------------------------

        [ServerRpc(RequireOwnership = false)]
        private void CallDeliveryDroneServerRpc() => CallDeliveryDroneClientRpc();

        [ServerRpc]
        private void ShowDeliveryPointServerRpc() => ShowDeliveryPointClientRpc();

        [ServerRpc]
        private void HideDeliveryPointServerRpc() => HideDeliveryPointClientRpc();
        
        [ServerRpc(RequireOwnership = false)]
        private void ResetTakeOffTimerServerRpc() => ResetTakeOffTimer();

        [ClientRpc]
        private void CallDeliveryDroneClientRpc()
        {
            if (!IsOwner)
                return;

            bool canCallDeliveryDrone = _droneState.Value == DroneState.WaitingForCall;
            
            if (!canCallDeliveryDrone)
                return;

            _deliveryPoint.LandDrone();
            _mobileHeadquartersEntity.OpenDoor();
            ShowDeliveryPointServerRpc();
        }

        [ClientRpc]
        private void ShowDeliveryPointClientRpc() =>
            _deliveryPoint.ShowPoint();

        [ClientRpc]
        private void HideDeliveryPointClientRpc() =>
            _deliveryPoint.HidePoint();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            InitServerAndClient();
            InitServer();
            InitClient();
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            DespawnServerAndClient();
            DespawnServer();
            DespawnClient();
        }

        private void OnResetTakeOffTimer()
        {
            if (IsOwner)
                ResetTakeOffTimer();
            else
                ResetTakeOffTimerServerRpc();
        }

        private void OnDroneTakeOffTimerFinished() => HideDeliveryPointServerRpc();

        private void OnDroneStateChanged(DroneState previousValue, DroneState newValue)
        {
            if (IsOwner)
                HandleDroneStateServerLogic(droneState: newValue);
            
            _deliveryManagerDecorator.DroneStateChanged(droneState: newValue);
        }

        private void OnCallDeliveryDrone() => CallDeliveryDroneServerRpc();
    }
}