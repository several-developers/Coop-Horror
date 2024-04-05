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

        // FIELDS: --------------------------------------------------------------------------------

        private IRpcHandlerDecorator _rpcHandlerDecorator;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _rpcHandlerDecorator = _mobileHeadquartersEntity.RpcHandlerDecorator;

        private void OnTriggerEnter(Collider other)
        {
            if (!_mobileHeadquartersEntity.IsOwner)
                return;

            if (!other.TryGetComponent(out PlayerEntity playerEntity))
                return;
            
            playerEntity.ToggleInsideMobileHQ(isInside: true);
            _rpcHandlerDecorator.TogglePlayerInsideMobileHQ(playerEntity.OwnerClientId, isInside: true);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!_mobileHeadquartersEntity.IsOwner)
                return;

            if (!other.TryGetComponent(out PlayerEntity playerEntity))
                return;
            
            playerEntity.ToggleInsideMobileHQ(isInside: false);
            _rpcHandlerDecorator.TogglePlayerInsideMobileHQ(playerEntity.OwnerClientId, isInside: false);
        }
    }
}
