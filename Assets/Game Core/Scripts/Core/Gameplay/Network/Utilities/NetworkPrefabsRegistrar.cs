using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Network.Utilities
{
    public class NetworkPrefabsRegistrar : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(DiContainer diContainer)
        {
            _diContainer = diContainer;
            RegisterPrefabs();
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private List<GameObject> _prefabs;
        
        // FIELDS: --------------------------------------------------------------------------------

        private static bool _isInitialized;
        
        private DiContainer _diContainer;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void RegisterPrefabs()
        {
            if (_isInitialized)
                return;
            
            NetworkManager networkManager = NetworkManager.Singleton;

            if (networkManager == null)
                return;
            
            _isInitialized = true;

            foreach (GameObject prefab in _prefabs)
            {
                bool containsNetworkObject = prefab.GetComponent<NetworkObject>() != null;

                if (!containsNetworkObject)
                {
                    Log.PrintError(log: $"Prefab <gb>{prefab.name}</gb> <rb>doesn't contains</rb> Network Object!");
                    continue;
                }
                
                networkManager.AddNetworkPrefab(prefab);

                networkManager.PrefabHandler.AddHandler(prefab,
                    instanceHandler: new ZenjectNetCodeFactory(prefab, _diContainer));
            }
        }
    }
}