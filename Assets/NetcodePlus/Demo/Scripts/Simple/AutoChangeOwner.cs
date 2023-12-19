using UnityEngine;

namespace NetcodePlus.Demo
{
    public class AutoChangeOwner : SNetworkBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        public float _refreshRate = 0.2f;

        // FIELDS: --------------------------------------------------------------------------------

        private float _ownerTimer = 0f;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected void Update() => UpdateServer();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void UpdateServer()
        {
            if (!IsServer)
                return;

            _ownerTimer += Time.fixedDeltaTime;
            
            if (_ownerTimer <= _refreshRate)
                return;

            _ownerTimer = 0f;

            //Change owner to nearest player, for smoother pushing
            SNetworkPlayer player = SNetworkPlayer.GetNearest(transform.position, 5f);

            if (player == null)
                return;
            
            ClientData client = TheNetwork.Get().GetClientByPlayerID(player.PlayerID);
            bool changeOwner = client != null && client.ClientID != NetObject.OwnerId; 
            
            if (changeOwner)
                NetObject.ChangeOwner(client.ClientID);
        }
    }
}