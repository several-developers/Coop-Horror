using System;
using GameCore.Configs.Gameplay.Delivery;
using GameCore.Gameplay.Delivery;
using GameCore.Gameplay.Network.Utilities;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Entities.DeliveryDrone
{
    public class DeliveryDroneEntity : NetworkBehaviour, IEntity, INetcodeBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IGameplayConfigsProvider gameplayConfigsProvider, IDeliveryPoint deliveryPoint)
        {
            _deliveryPoint = deliveryPoint;
            
            DeliveryConfigMeta deliveryConfig = gameplayConfigsProvider.GetDeliveryConfig();
            Transform droneCartTransform = deliveryPoint.GetDroneCartTransform();

            _droneMovement = new DroneMovement(deliveryConfig, droneCartTransform, droneTransform: transform);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private IDeliveryPoint _deliveryPoint;
        private DroneMovement _droneMovement;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Update()
        {
            TickServerAndClient();
            TickServer();
            TickClient();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void InitServerAndClient()
        {
        }

        public void InitServer()
        {
            if (!IsOwner)
                return;

            _droneMovement.TeleportDroneToDroneCart();
            
            _deliveryPoint.OnTeleportDroneToDroneCartEvent += OnTeleportDroneToDroneCart;
        }

        public void InitClient()
        {
            if (IsOwner)
                return;
        }
        
        public void TickServerAndClient()
        {
        }

        public void TickServer()
        {
            if (!IsOwner)
                return;
            
            _droneMovement.Tick();
        }

        public void TickClient()
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
            
            _deliveryPoint.OnTeleportDroneToDroneCartEvent -= OnTeleportDroneToDroneCart;
        }

        public void DespawnClient()
        {
            if (IsOwner)
                return;
        }
        
        public Transform GetTransform() => transform;

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

        private void OnTeleportDroneToDroneCart() =>
            _droneMovement.TeleportDroneToDroneCart();
    }
}