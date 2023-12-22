using UnityEngine;

namespace GameCore.Gameplay.Network
{
    public class HorrorGame : MonoBehaviour
    {
        // FIELDS: --------------------------------------------------------------------------------

        private static HorrorGame _instance;
        
        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _instance = this;
            StartGame();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public static HorrorGame Get() => _instance;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private static void StartGame()
        {
            TheNetworkHorror network = TheNetworkHorror.Get();
            bool isNetworkActive = network.IsActive();

            if (isNetworkActive)
                return;

            network.StartHost();
        }
    }
}