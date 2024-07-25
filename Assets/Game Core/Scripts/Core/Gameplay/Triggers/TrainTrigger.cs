using GameCore.Gameplay.Entities.Player;
using UnityEngine;

namespace GameCore.Gameplay.Triggers
{
    public class TrainTrigger : MonoBehaviour
    {
        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out PlayerEntity playerEntity))
                return;

            bool isLocalPlayer = playerEntity.IsLocalPlayer();

            if (!isLocalPlayer)
                return;
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent(out PlayerEntity playerEntity))
                return;

            bool isLocalPlayer = playerEntity.IsLocalPlayer();

            if (!isLocalPlayer)
                return;
        }
    }
}