using UnityEngine;

namespace GameCore.Gameplay.Network
{
    public class HorrorGame : MonoBehaviour
    {
        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() => StartGame();

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