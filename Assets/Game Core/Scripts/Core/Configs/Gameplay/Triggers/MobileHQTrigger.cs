using GameCore.Gameplay.Entities.Player;
using UnityEngine;

namespace GameCore.Gameplay.Triggers
{
    public class MobileHQTrigger : MonoBehaviour
    {
        // GAME ENGINE METHODS: -------------------------------------------------------------------
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out PlayerEntity playerEntity))
                playerEntity.ToggleInsideMobileHQ(isInside: true);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out PlayerEntity playerEntity))
                playerEntity.ToggleInsideMobileHQ(isInside: false);
        }
    }
}
