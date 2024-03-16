using System;
using Unity.Netcode;

namespace GameCore.Gameplay.Network
{
    // Useful for classes that can't be NetworkBehaviours themselves (for example, with dedicated servers,
    // you can't have a NetworkBehaviour that exists on clients but gets stripped on the server,
    // this will mess with your NetworkBehaviour indexing.
    public class NetcodeHooks : NetworkBehaviour
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action OnNetworkSpawnHook;
        public event Action OnNetworkDespawnHook;

        // EVENTS RECEIVERS: ----------------------------------------------------------------------
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            OnNetworkSpawnHook?.Invoke();
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            OnNetworkDespawnHook?.Invoke();
        }
    }
}