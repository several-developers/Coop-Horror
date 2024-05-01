using System;
using GameCore.Enums.Gameplay;

namespace GameCore.Gameplay.Delivery
{
    public interface IDeliveryManagerDecorator
    {
        event Action<DroneState> OnDroneStateChangedEvent;
        void DroneStateChanged(DroneState droneState);
        
        event Action OnResetTakeOffTimerInnerEvent; 
        event Func<DroneState> OnGetDroneStateInnerEvent;
        void ResetTakeOffTimer();
        DroneState GetDroneState();
    }
}