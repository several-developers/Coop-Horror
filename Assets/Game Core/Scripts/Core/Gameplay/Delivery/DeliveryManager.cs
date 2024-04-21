using GameCore.Gameplay.Entities.MobileHeadquarters;
using GameCore.Gameplay.Network.Utilities;
using Unity.Netcode;
using Zenject;

namespace GameCore.Gameplay.Delivery
{
    public class DeliveryManager : NetworkBehaviour, INetcodeInitBehaviour, INetcodeDespawnBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IDeliveryPoint deliveryPoint, IMobileHeadquartersEntity mobileHeadquartersEntity)
        {
            _deliveryPoint = deliveryPoint;
            _mobileHeadquartersEntity = mobileHeadquartersEntity;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private IDeliveryPoint _deliveryPoint;
        private IMobileHeadquartersEntity _mobileHeadquartersEntity;
        private bool _canCallDeliveryDrone;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void InitServerAndClient()
        {
            
        }

        public void InitServer()
        {
            if (!IsOwner)
                return;

            _canCallDeliveryDrone = true;
            
            _deliveryPoint.OnDroneLandedEvent += OnDroneLanded;
            _deliveryPoint.OnDroneLeftEvent += OnDroneLeft;
            _deliveryPoint.OnDroneTakeOffTimerFinishedEvent += OnDroneTakeOffTimerFinished;

            _mobileHeadquartersEntity.OnCallDeliveryDroneEvent += OnCallDeliveryDrone;
        }

        public void InitClient()
        {
            if (IsOwner)
                return;
        }

        public void DespawnServerAndClient()
        {
            
        }

        public void DespawnServer()
        {
            if (!IsOwner)
                return;
            
            _deliveryPoint.OnDroneLandedEvent -= OnDroneLanded;
            _deliveryPoint.OnDroneLeftEvent -= OnDroneLeft;
            _deliveryPoint.OnDroneTakeOffTimerFinishedEvent -= OnDroneTakeOffTimerFinished;

            _mobileHeadquartersEntity.OnCallDeliveryDroneEvent -= OnCallDeliveryDrone;
        }

        public void DespawnClient()
        {
            if (IsOwner)
                return;
        }

        // RPC: -----------------------------------------------------------------------------------

        [ServerRpc(RequireOwnership = false)]
        private void CallDeliveryDroneServerRpc() => CallDeliveryDroneClientRpc();

        [ServerRpc]
        private void ShowDeliveryPointServerRpc() => ShowDeliveryPointClientRpc();

        [ServerRpc]
        private void HideDeliveryPointServerRpc() => HideDeliveryPointClientRpc();

        [ClientRpc]
        private void CallDeliveryDroneClientRpc()
        {
            if (!IsOwner)
                return;

            if (!_canCallDeliveryDrone)
                return;

            _canCallDeliveryDrone = false;
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

        private void OnDroneLanded() =>
            _deliveryPoint.StartTakeOffTimer();

        private void OnDroneLeft() =>
            _canCallDeliveryDrone = true;

        private void OnDroneTakeOffTimerFinished() => HideDeliveryPointServerRpc();

        private void OnCallDeliveryDrone() => CallDeliveryDroneServerRpc();
    }
}