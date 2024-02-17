using GameCore.Gameplay.Entities.MobileHeadquarters;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Network;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Triggers
{
    public class MobileHQTrigger : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private MobileHeadquartersEntity _mobileHeadquartersEntity;
        
        // GAME ENGINE METHODS: -------------------------------------------------------------------
        
        private void OnTriggerEnter(Collider other)
        {
            if (!_mobileHeadquartersEntity.IsOwner)
                return;

            if (!other.TryGetComponent(out PlayerEntity playerEntity))
                return;
            
            playerEntity.ToggleInsideMobileHQ(isInside: true);
            
            RpcCaller rpcCaller = _mobileHeadquartersEntity.RpcCaller;
            rpcCaller.TogglePlayerInsideMobileHQEvent(playerEntity.OwnerClientId, isInside: true);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!_mobileHeadquartersEntity.IsOwner)
                return;

            if (!other.TryGetComponent(out PlayerEntity playerEntity))
                return;
            
            playerEntity.ToggleInsideMobileHQ(isInside: false);
            
            RpcCaller rpcCaller = _mobileHeadquartersEntity.RpcCaller;
            rpcCaller.TogglePlayerInsideMobileHQEvent(playerEntity.OwnerClientId, isInside: false);
        }
    }
}
