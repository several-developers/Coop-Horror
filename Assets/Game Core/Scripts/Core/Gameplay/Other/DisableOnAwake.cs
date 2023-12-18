using UnityEngine;

namespace GameCore.Gameplay.Other
{
    public class DisableOnAwake : MonoBehaviour
    {
        private void Awake() =>
            gameObject.SetActive(false);
    }
}