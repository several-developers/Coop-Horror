using System;
using UnityEngine;

namespace GameCore.Gameplay.Delivery
{
    public interface IDeliveryPoint
    {
        event Action OnDroneLandedEvent;
        event Action OnDroneLeftEvent;
        event Action OnTeleportDroneToDroneCartEvent;
        event Action OnTakeOffTimerFinishedEvent;
        void ShowPoint();
        void HidePoint();
        void LandDrone();
        void TakeOffDrone();
        void StartTakeOffTimer();
        Transform GetDroneCartTransform();
    }
}