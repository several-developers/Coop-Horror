using UnityEngine;

namespace GameCore.Gameplay.Level.Elevator
{
    public class CubicTest : MonoBehaviour
    {
        private static CubicTest _instance;

        private void Awake()
        {
            _instance = this;
        }

        public static CubicTest Get() => _instance;
    }
}