using GameCore.Gameplay.Network;
using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Player
{
    public class CubicEntity : NetcodeBehaviour
    {
        public ClientNetworkTransform clientNetworkTransform;

        protected override void TickServerOnly()
        {
            if (Input.GetKey(KeyCode.Equals))
            {
                Vector3 position = transform.position;
                position.z += 1f * Time.deltaTime * 1f;
                transform.position = position;
            }
            
            if (Input.GetKey(KeyCode.Minus))
            {
                Vector3 position = transform.position;
                position.z -= 1f * Time.deltaTime * 1f;
                transform.position = position;
            }
        }

        public void SetParent(NetworkObject parentNetworkObject)
        {
            if (NetworkHorror.IsTrueServer)
                TrySetParent(parentNetworkObject);
            else
                TrySetParentServerRpc(parentNetworkObject);
        }

        public void RemoveParent()
        {
            if (NetworkHorror.IsTrueServer)
                TryRemoveParent();
            else
                TryRemoveParentServerRpc();
        }
        
        private void TrySetParent(NetworkObject networkObject)
        {
            if (NetworkObject.TrySetParent(networkObject))
                clientNetworkTransform.InLocalSpace = true;
        }
        
        private void TryRemoveParent()
        {
            if (NetworkObject.TryRemoveParent())
                clientNetworkTransform.InLocalSpace = false;
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void TrySetParentServerRpc(NetworkObjectReference networkObjectReference)
        {
            bool isNetworkObjectFound = networkObjectReference.TryGet(out NetworkObject networkObject);

            if (!isNetworkObjectFound)
                return;
            
            TrySetParent(networkObject);
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void TryRemoveParentServerRpc() => TryRemoveParent();
    }
}