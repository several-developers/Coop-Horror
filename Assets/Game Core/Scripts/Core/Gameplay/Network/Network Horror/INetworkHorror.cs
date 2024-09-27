using System;

namespace GameCore.Gameplay.Network
{
    public interface INetworkHorror
    {
        event Action<ulong> OnPlayerConnectedEvent;
        event Action<ulong> OnPlayerDisconnectedEvent;
        bool IsOwner { get; }
    }
}