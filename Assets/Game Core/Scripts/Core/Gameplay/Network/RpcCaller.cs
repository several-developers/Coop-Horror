using System;
using Unity.Netcode;

namespace GameCore.Gameplay.Network
{
    public class RpcCaller : NetworkBehaviour
    {
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<ulong, ClientState> OnSetNetworkHorrorStateEvent;
        
        private static RpcCaller _instance;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _instance = this;

        private void Start()
        {
            if (IsSpawned)
                return;

            if (!IsServer)
                return;

            NetworkObject.Spawn();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SendSetNetworkHorrorState(ulong clientID, ClientState state) =>
            SetNetworkHorrorStateServerRpc(clientID, (int)state);

        public static RpcCaller Get() => _instance;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        [ServerRpc(RequireOwnership = false)]
        private void SetNetworkHorrorStateServerRpc(ulong clientID, int stateIndex) =>
            SetNetworkHorrorStateClientRpc(clientID, stateIndex);

        [ClientRpc]
        private void SetNetworkHorrorStateClientRpc(ulong clientID, int stateIndex) =>
            OnSetNetworkHorrorStateEvent?.Invoke(clientID, (ClientState)stateIndex);
    }
}