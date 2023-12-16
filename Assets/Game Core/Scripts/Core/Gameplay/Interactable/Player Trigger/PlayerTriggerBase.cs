using UnityEngine;

namespace GameCore.Gameplay.Interactable
{
    public abstract class PlayerTriggerBase : MonoBehaviour
    {
        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void OnTriggerEnter(Collider other) => TriggerLogic();

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected abstract void TriggerLogic();
    }
}