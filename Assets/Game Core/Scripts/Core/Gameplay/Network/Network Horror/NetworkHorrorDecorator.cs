using System;

namespace GameCore.Gameplay.Network
{
    public class NetworkHorrorDecorator : INetworkHorrorDecorator
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Func<bool> OnIsServerEvent;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public bool IsServer() =>
            OnIsServerEvent != null && OnIsServerEvent();
    }
}