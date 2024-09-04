using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Configs.Gameplay.EntitiesList;
using GameCore.Configs.Gameplay.ItemsList;
using GameCore.Configs.Gameplay.MonstersList;
using GameCore.Gameplay.Entities;
using GameCore.Gameplay.Items;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

namespace GameCore.Gameplay.Network.Utilities
{
    public class NetworkPrefabsRegistrar : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(DiContainer diContainer, IGameplayConfigsProvider gameplayConfigsProvider)
        {
            _diContainer = diContainer;
            _networkManager = NetworkManager.Singleton;
            _entitiesListConfig = gameplayConfigsProvider.GetEntitiesListConfig();
            _itemsListConfig = gameplayConfigsProvider.GetItemsListConfig();
            _monstersListConfig = gameplayConfigsProvider.GetMonstersListConfig();
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        [ListDrawerSettings(AlwaysAddDefaultValue = true)]
        private List<GameObject> _prefabs;

        // FIELDS: --------------------------------------------------------------------------------

        private readonly List<GameObject> _prefabsToRegister = new();

        private DiContainer _diContainer;
        private NetworkManager _networkManager;
        private EntitiesListConfigMeta _entitiesListConfig;
        private ItemsListConfigMeta _itemsListConfig;
        private MonstersListConfigMeta _monstersListConfig;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            RegisterPrefabs();
            StartLoadingPrefabs();
        }

        private void OnDestroy() => RemovePrefabs();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async UniTaskVoid StartLoadingPrefabs()
        {
            IEnumerable<AssetReferenceGameObject>
                allEntitiesReferences = _entitiesListConfig.GetAllEntitiesReferences();

            foreach (AssetReferenceGameObject assetReference in allEntitiesReferences)
            {
                GameObject prefab = await assetReference.LoadAssetAsync().Task;
                RegisterPrefab(prefab);
            }
        }

        private void RegisterPrefabs()
        {
            NetworkManager networkManager = NetworkManager.Singleton;

            if (networkManager == null)
                return;

            AddLocalListPrefabs();
            // AddEntitiesPrefabs();
            AddItemsPrefabs();
            AddMonstersPrefabs();

            foreach (GameObject prefab in _prefabsToRegister)
                RegisterPrefab(prefab);
        }

        private void RegisterPrefab(GameObject prefab)
        {
            bool containsNetworkObject = prefab.GetComponent<NetworkObject>() != null;

            if (!containsNetworkObject)
            {
                Log.PrintError(log: $"Prefab <gb>{prefab.name}</gb> <rb>doesn't contains</rb> Network Object!");
                return;
            }

            _networkManager.AddNetworkPrefab(prefab);

            _networkManager.PrefabHandler.AddHandler(prefab,
                instanceHandler: new ZenjectNetCodeFactory(prefab, _diContainer));
        }

        private void AddLocalListPrefabs() =>
            _prefabsToRegister.AddRange(_prefabs);

        private void AddEntitiesPrefabs()
        {
            IEnumerable<Entity> allEntities = _entitiesListConfig.GetAllEntities();

            foreach (Entity entity in allEntities)
                _prefabsToRegister.Add(entity.gameObject);
        }

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
            IEnumerable<MonsterReference> allItems = _monstersListConfig.GetAllReferences();

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