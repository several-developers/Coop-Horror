using UnityEngine;

namespace GameCore.Gameplay.Other
{
    public class DisableAtAwake : MonoBehaviour
    {
        // GAME ENGINE METHODS: -------------------------------------------------------------------
        
        private void Awake() =>
            gameObject.SetActive(false);
    }
}