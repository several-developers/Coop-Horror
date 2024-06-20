using GameCore.Gameplay.Dungeons;
using GameCore.Observers.Gameplay.Rpc;
using Unity.Netcode;
using Zenject;

namespace GameCore.Gameplay.Network
{
    public class RpcHandler : NetworkBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IRpcHandlerDecorator rpcHandlerDecorator, IRpcObserver rpcObserver)
        {
            _rpcHandlerDecorator = rpcHandlerDecorator;
            _rpcObserver = rpcObserver;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private IRpcHandlerDecorator _rpcHandlerDecorator;
        private IRpcObserver _rpcObserver;

        // RPC: -----------------------------------------------------------------------------------

        [ServerRpc(RequireOwnership = false)]
        private void GenerateDungeonsServerRpc(DungeonsSeedData data) => GenerateDungeonsClientRpc(data);

        [ClientRpc]
        private void GenerateDungeonsClientRpc(DungeonsSeedData data) =>
            _rpcObserver.GenerateDungeons(data);

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            _rpcHandlerDecorator.OnGenerateDungeonsInnerEvent += GenerateDungeonsServerRpc;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            
            _rpcHandlerDecorator.OnGenerateDungeonsInnerEvent -= GenerateDungeonsServerRpc;
        }
    }
}