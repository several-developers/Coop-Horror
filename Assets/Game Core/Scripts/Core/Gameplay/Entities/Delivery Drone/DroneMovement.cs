using GameCore.Configs.Gameplay.Delivery;
using UnityEngine;

namespace GameCore.Gameplay.Entities.DeliveryDrone
{
    public class DroneMovement
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public DroneMovement(DeliveryConfigMeta deliveryConfig, Transform droneCartTransform, Transform droneTransform)
        {
            _deliveryConfig = deliveryConfig;
            _droneCartTransform = droneCartTransform;
            _droneTransform = droneTransform;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly DeliveryConfigMeta _deliveryConfig;
        private readonly DeliveryDroneEntity _deliveryDroneEntity;
        private readonly Transform _droneCartTransform;
        private readonly Transform _droneTransform;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Tick() => Movement();

        public void TeleportDroneToDroneCart() =>
            _droneTransform.position = _droneCartTransform.position;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void Movement()
        {
            Vector3 currentPosition = _droneTransform.position;
            Vector3 targetPosition = _droneCartTransform.position;
            float flySpeed = _deliveryConfig.DroneFlySpeed * Time.deltaTime;
            _droneTransform.position = Vector3.MoveTowards(currentPosition, targetPosition, flySpeed);
        }
    }
}