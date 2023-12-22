using Unity.Netcode;

namespace GameCore.Gameplay.NetworkDepricated2
{
    public class HorrorGameDepricated2 : NetworkBehaviour
    {
        // FIELDS: --------------------------------------------------------------------------------

        private static HorrorGameDepricated2 _instance;
        
        // GAME ENGINE METHODS: -------------------------------------------------------------------
        
        private void Awake()
        {
            _instance = this;
            
            StartGame();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public static HorrorGameDepricated2 Get() => _instance;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private static void StartGame()
        {
            TheNetworkHorrorDepricated2 network = TheNetworkHorrorDepricated2.Get();
            bool isActive = network.IsActive();

            if (isActive)
                return;

            network.StartHost(1111);
        }
    }
}