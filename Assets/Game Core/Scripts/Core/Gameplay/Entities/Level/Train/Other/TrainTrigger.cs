using GameCore.Gameplay.Entities.Player;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Level.Train
{
    public class TrainTrigger : MonoBehaviour
    {
        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void OnTriggerEnter(Collider other) => CheckForPlayer(other, enter: true);

        private void OnTriggerExit(Collider other) => CheckForPlayer(other, enter: false);

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private static void CheckForPlayer(Component other, bool enter)
        {
            if (!other.TryGetComponent(out PlayerEntity playerEntity))
                return;
            
            playerEntity.ToggleInsideTrainState(enter);
        }
    }
}