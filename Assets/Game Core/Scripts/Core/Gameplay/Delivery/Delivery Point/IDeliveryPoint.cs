using System;
using GameCore.Enums.Gameplay;
using UnityEngine;

namespace GameCore.Gameplay.Delivery
{
    public interface IDeliveryPoint
    {
        event Action<DroneState> OnDroneStateChangedEvent;
        event Action OnTeleportDroneToDroneCartEvent;
        event Action OnDroneTakeOffTimerFinishedEvent;
        void ShowPoint();
        void HidePoint();
        void LandDrone();
        void TakeOffDrone();
        void StartTakeOffTimer();
        void ResetTakeOffTimer();
        Transform GetDroneCartTransform();
    }
}