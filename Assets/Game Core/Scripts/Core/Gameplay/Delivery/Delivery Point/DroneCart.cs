using Cinemachine;
using GameCore.Configs.Gameplay.Delivery;
using GameCore.Enums.Gameplay;
using UnityEngine;

namespace GameCore.Gameplay.Delivery
{
    public class DroneCart
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public DroneCart(DeliveryConfigMeta deliveryConfig, DeliveryPoint deliveryPoint, CinemachinePath landingPath,
            CinemachinePath takeOffPath, CinemachineDollyCart deliveryCart)
        {
            _deliveryConfig = deliveryConfig;
            _deliveryPoint = deliveryPoint;
            _landingPath = landingPath;
            _takeOffPath = takeOffPath;
            _deliveryCart = deliveryCart;

            _deliveryCart.m_Path = landingPath;
            _deliveryCart.m_Position = 0f;
            _deliveryCart.m_Speed = 0f;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly DeliveryConfigMeta _deliveryConfig;
        private readonly DeliveryPoint _deliveryPoint;
        private readonly CinemachinePath _landingPath;
        private readonly CinemachinePath _takeOffPath;
        private readonly CinemachineDollyCart _deliveryCart;

        private DroneState _droneState;
        private bool _isFlying;
        private bool _isLanding;

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
            _isLanding = true;
            _deliveryCart.m_Path = _landingPath;
            _deliveryCart.m_Position = 0f;

            ChangeState(DroneState.LandingInProgress);
            StartFlying();
        }

        public void TakeOff()
        {
            _isLanding = false;
            _deliveryCart.m_Path = _takeOffPath;
            _deliveryCart.m_Position = 0f;

            ChangeState(DroneState.TakeOffInProgress);
            StartFlying();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ChangeState(DroneState droneState)
        {
            _droneState = droneState;
            _deliveryPoint.SendDroneStateChanged(droneState);
        }

        private void StartFlying()
        {
            _isFlying = true;
            _deliveryCart.m_Speed = 0f;
        }

        private void StopFlying()
        {
            _isFlying = false;
            _deliveryCart.m_Speed = 0f;

            switch (_droneState)
            {
                case DroneState.LandingInProgress:
                    ChangeState(DroneState.Landed);
                    break;
                
                case DroneState.TakeOffInProgress:
                    ChangeState(DroneState.WaitingForCall);
                    break;
            }
        }
    }
}