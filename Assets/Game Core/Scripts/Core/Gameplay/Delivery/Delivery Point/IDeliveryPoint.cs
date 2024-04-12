using System;
using UnityEngine;

namespace GameCore.Gameplay.Delivery
{
    public interface IDeliveryPoint
    {
        event Action OnTeleportDroneToDroneCartEvent;
        Transform GetDroneCartTransform();
    }
}