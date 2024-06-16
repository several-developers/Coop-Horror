using System.Collections.Generic;
using GameCore.Configs.Gameplay.ItemsList;
using GameCore.Configs.Gameplay.MonstersList;
using GameCore.Gameplay.Items;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
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
        private void Construct(DiContainer diContainer, IGameplayConfigsProvider gameplayConfigsProvider)
        {
            _itemsListConfig = gameplayConfigsProvider.GetItemsListConfig();
            _monstersListConfig = gameplayConfigsProvider.GetMonstersListConfig();
            
            RegisterPrefabs(diContainer);
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        [ListDrawerSettings(AlwaysAddDefaultValue = true)]
        private List<GameObject> _prefabs;
        
        // FIELDS: --------------------------------------------------------------------------------

        private readonly List<GameObject> _prefabsToRegister = new();
        
        private ItemsListConfigMeta _itemsListConfig;
        private MonstersListConfigMeta _monstersListConfig;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void OnDestroy() => RemovePrefabs();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void RegisterPrefabs(DiContainer diContainer)
        {
            NetworkManager networkManager = NetworkManager.Singleton;

            if (networkManager == null)
                return;

            AddLocalListPrefabs();
            AddItemsPrefabs();
            AddMonstersPrefabs();

            foreach (GameObject prefab in _prefabsToRegister)
            {
                bool containsNetworkObject = prefab.GetComponent<NetworkObject>() != null;

                if (!containsNetworkObject)
                {
                    Log.PrintError(log: $"Prefab <gb>{prefab.name}</gb> <rb>doesn't contains</rb> Network Object!");
                    continue;
                }
                
                networkManager.AddNetworkPrefab(prefab);

                networkManager.PrefabHandler.AddHandler(prefab,
                    instanceHandler: new ZenjectNetCodeFactory(prefab, diContainer));
            }
        }

        private void AddLocalListPrefabs() =>
            _prefabsToRegister.AddRange(_prefabs);

        private void AddItemsPrefabs()
        {
            IEnumerable<ItemMeta> allItems = _itemsListConfig.GetAllItems();

            foreach (ItemMeta itemMeta in allItems)
            {
                GameObject prefab = itemMeta.ItemPrefab.gameObject;
                _prefabsToRegister.Add(prefab);
            }
        }
        
        private void AddMonstersPrefabs()
        {
            IReadOnlyList<MonsterReference> allItems = _monstersListConfig.GetAllReferences();

            foreach (MonsterReference monsterReference in allItems)
            {
                GameObject prefab = monsterReference.MonsterPrefab.gameObject;
                _prefabsToRegister.Add(prefab);
            }
        }

        private void RemovePrefabs()
        {
            NetworkManager networkManager = NetworkManager.Singleton;

            if (networkManager == null)
                return;
            
            foreach (GameObject prefab in _prefabsToRegister)
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