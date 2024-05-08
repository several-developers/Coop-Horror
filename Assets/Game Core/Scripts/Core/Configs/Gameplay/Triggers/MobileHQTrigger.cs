using GameCore.Gameplay.Entities.MobileHeadquarters;
using GameCore.Gameplay.Entities.Player;
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
        }

        private void OnTriggerExit(Collider other)
        {
            if (!_mobileHeadquartersEntity.IsOwner)
                return;

            if (!other.TryGetComponent(out PlayerEntity playerEntity))
                return;

            playerEntity.ToggleInsideMobileHQ(isInside: false);
        }
    }
}