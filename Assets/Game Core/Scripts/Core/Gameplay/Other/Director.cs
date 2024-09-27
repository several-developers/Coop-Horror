using UnityEngine;

namespace GameCore.Gameplay.Other
{
    public class Director : MonoBehaviour
    {
        // FIELDS: --------------------------------------------------------------------------------

        private static Director _instance;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}