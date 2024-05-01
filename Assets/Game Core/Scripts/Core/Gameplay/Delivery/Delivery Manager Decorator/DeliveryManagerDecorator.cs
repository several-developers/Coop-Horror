using System;
using GameCore.Enums.Gameplay;

namespace GameCore.Gameplay.Delivery
{
    public class DeliveryManagerDecorator : IDeliveryManagerDecorator
    {
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<DroneState> OnDroneStateChangedEvent = delegate { };

        public event Action OnResetTakeOffTimerInnerEvent = delegate { };
        public event Func<DroneState> OnGetDroneStateInnerEvent;
        public event Func<bool> OnCanCallDeliveryDroneInnerEvent;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void DroneStateChanged(DroneState droneState) =>
            OnDroneStateChangedEvent.Invoke(droneState);

        public void ResetTakeOffTimer() =>
            OnResetTakeOffTimerInnerEvent.Invoke();

        public DroneState GetDroneState() =>
            OnGetDroneStateInnerEvent?.Invoke() ?? DroneState.WaitingForCall;

        public bool CanCallDeliveryDrone() =>
            OnCanCallDeliveryDroneInnerEvent?.Invoke() ?? false;
    }
}