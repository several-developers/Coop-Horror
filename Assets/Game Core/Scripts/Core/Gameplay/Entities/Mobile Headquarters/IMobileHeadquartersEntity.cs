﻿namespace GameCore.Gameplay.Entities.MobileHeadquarters
{
    public interface IMobileHeadquartersEntity : IEntity, INetworkObject
    {
        void ArrivedAtRoadLocation(bool sendTeleportEvent = true);
    }
}