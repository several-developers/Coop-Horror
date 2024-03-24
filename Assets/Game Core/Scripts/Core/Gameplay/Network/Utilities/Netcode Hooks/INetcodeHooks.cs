using System;

namespace GameCore.Gameplay.Network
{
    public interface INetcodeHooks
    {
        event Action OnNetworkSpawnHookEvent;
        event Action OnNetworkDespawnHookEvent;
    }
}