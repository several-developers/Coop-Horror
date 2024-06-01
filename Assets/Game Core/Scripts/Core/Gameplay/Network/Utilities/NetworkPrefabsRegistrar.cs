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
        [ListDrawerSettings(AlwaysAddDefaultValue = true)]
        private List<GameObject> _prefabs;
        
        // FIELDS: --------------------------------------------------------------------------------

        private DiContainer _diContainer;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void OnDestroy() => RemovePrefabs();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void RegisterPrefabs()
        {
            NetworkManager networkManager = NetworkManager.Singleton;

            if (networkManager == null)
                return;
            
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

        private void RemovePrefabs()
        {
            NetworkManager networkManager = NetworkManager.Singleton;

            if (networkManager == null)
                return;
            
            foreach (GameObject prefab in _prefabs)
            {
                bool containsNetworkObject = prefab.GetComponent<NetworkObject>() != null;

                if (!containsNetworkObject)
                {
                    Log.PrintError(log: $"Prefab <gb>{prefab.name}</gb> <rb>doesn't contains</rb> Network Object!");
                    continue;
                }
                
                networkManager.RemoveNetworkPrefab(prefab);
            }
        }
    }
}