using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.Network
{
    public class TheNetworkHorror : MonoBehaviour
    {
        // FIELDS: --------------------------------------------------------------------------------

        private static TheNetworkHorror _instance;

        private NetworkManager _networkManager;

        // GAME ENGINE METHODS: -------------------------------------------------------------------
        
        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }
            
            Init();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void StartHost() =>
            _networkManager.StartHost();

        public void StartClient() =>
            _networkManager.StartClient();

        public bool IsActive() =>
            _networkManager.IsServer || _networkManager.IsHost || _networkManager.IsClient;
        
        public static TheNetworkHorror Get() => _instance;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void Init()
        {
            _instance = this;
            _networkManager = GetComponent<NetworkManager>();
        }
    }
}