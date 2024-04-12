using Cinemachine;
using GameCore.Configs.Gameplay.Delivery;
using UnityEngine;

namespace GameCore.Gameplay.Delivery
{
    public class DroneCart
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public DroneCart(DeliveryConfigMeta deliveryConfig, CinemachinePath landingPath, CinemachinePath takeOffPath,
            CinemachineDollyCart deliveryCart)
        {
            _deliveryConfig = deliveryConfig;
            _landingPath = landingPath;
            _takeOffPath = takeOffPath;
            _deliveryCart = deliveryCart;

            _deliveryCart.m_Path = landingPath;
            _deliveryCart.m_Position = 0f;
            _deliveryCart.m_Speed = 0f;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly DeliveryConfigMeta _deliveryConfig;
        private readonly CinemachinePath _landingPath;
        private readonly CinemachinePath _takeOffPath;
        private readonly CinemachineDollyCart _deliveryCart;
        
        private bool _isFlying;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Tick()
        {
            if (!_isFlying)
                return;
            
            float targetSpeed = _deliveryConfig.DroneCartFlySpeed;
            float speedChangeRate = _deliveryConfig.DroneFlySpeedChangeRate;
            
            const float speedOffset = 0.1f;
            
            float finalSpeed;
            float currentSpeed = _deliveryCart.m_Speed;
            bool changeSpeedSmoothly = currentSpeed < targetSpeed - speedOffset ||
                                       currentSpeed > targetSpeed + speedOffset;

            // accelerate or decelerate to target speed
            if (changeSpeedSmoothly)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                finalSpeed = Mathf.Lerp(currentSpeed, targetSpeed * 1f, Time.deltaTime * speedChangeRate);

                // round speed to 3 decimal places
                finalSpeed = Mathf.Round(finalSpeed * 1000f) / 1000f;
            }
            else
            {
                finalSpeed = targetSpeed;
            }

            _deliveryCart.m_Speed = finalSpeed;

            float position = _deliveryCart.m_Position;
            float pathLength = _deliveryCart.m_Path.PathLength;
            float percent = Mathf.Clamp01(position / pathLength);
            int percentInt = (int)(percent * 100);

            if (percentInt == 100)
                StopFlying();
        }

        public void Land()
        {
            _deliveryCart.m_Path = _landingPath;
            _deliveryCart.m_Position = 0f;

            StartFlying();
        }

        public void TakeOff()
        {
            _deliveryCart.m_Path = _takeOffPath;
            _deliveryCart.m_Position = 0f;

            StartFlying();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void StartFlying()
        {
            _isFlying = true;
            _deliveryCart.m_Speed = 0f;
        }

        private void StopFlying()
        {
            _isFlying = false;
            _deliveryCart.m_Speed = 0f;
        }
    }
}