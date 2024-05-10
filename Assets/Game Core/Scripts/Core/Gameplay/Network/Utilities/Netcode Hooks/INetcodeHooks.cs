using System;

namespace GameCore.Gameplay.Network
{
    public interface INetcodeHooks
    {
        event Action OnNetworkSpawnHookEvent;
        event Action OnNetworkDespawnHookEvent;
        ulong OwnerClientId { get; }
        bool IsServerOnly { get; }
        bool IsOwner { get; }
        bool IsSpawned { get; }
    }
}