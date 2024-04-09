using System;

namespace GameCore.Gameplay.Network
{
    public interface INetworkHorrorDecorator
    {
        event Func<bool> OnIsServerEvent; 
        bool IsServer();
    }
}