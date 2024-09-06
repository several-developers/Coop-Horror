using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Configs.Gameplay.ItemsList;
using GameCore.Configs.Global.EntitiesList;
using GameCore.Configs.Global.MonstersList;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.Items;
using GameCore.Gameplay.Network.DynamicPrefabs;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using GameCore.Infrastructure.Providers.Global;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

namespace GameCore.Gameplay.Network.Utilities
{
    public class NetworkPrefabsService
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public NetworkPrefabsService()
        {
            
        }
    }
    public class NetworkPrefabsRegistrar : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(
            DiContainer diContainer,
            IAssetsProvider assetsProvider,
            IConfigsProvider configsProvider,
            IGameplayConfigsProvider gameplayConfigsProvider
        )
        {
            _diContainer = diContainer;
            _networkManager = NetworkManager.Singleton;
            _assetsProvider = assetsProvider;

            _entitiesListConfig = configsProvider.GetConfig<EntitiesListConfigMeta>();
            _itemsListConfig = gameplayConfigsProvider.GetConfig<ItemsListConfigMeta>();
            _monstersListConfig = configsProvider.GetConfig<MonstersListConfigMeta>();
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
        private IAssetsProvider _assetsProvider;
        private EntitiesListConfigMeta _entitiesListConfig;
        private ItemsListConfigMeta _itemsListConfig;
        private MonstersListConfigMeta _monstersListConfig;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            DynamicPrefabLoadingUtilities.SetDiContainer(_diContainer);
            RegisterPrefabs();
            RegisterAddressables();
        }

        private void OnDestroy() => RemovePrefabs();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async void RegisterAddressables()
        {
            await RegisterEntities();
            await RegisterMonsterEntities();
            GameManager.Instance.SendPlayerLoadedServerRpc();
        }

        private async UniTask RegisterEntities()
        {
            IEnumerable<AssetReferenceGameObject> allReferences = _entitiesListConfig.GetAllReferences();

            foreach (AssetReferenceGameObject assetReference in allReferences)
                await LoadAndRegisterAsset(assetReference);
        }

        private async UniTask RegisterMonsterEntities()
        {
            IEnumerable<MonsterReference> allReferences = _monstersListConfig.GetAllReferences();

            foreach (MonsterReference monsterReference in allReferences)
            {
                AssetReferenceGameObject assetReference = monsterReference.AssetReference;
                await LoadAndRegisterAsset(assetReference);
            }
        }

        private async UniTask LoadAndRegisterAsset(AssetReferenceGameObject assetReference)
        {
            var prefab = await _assetsProvider.LoadAsset<GameObject>(assetReference);
            RegisterPrefab(prefab);
            _assetsProvider.ReleaseAsset(assetReference);
        }

        private void RegisterPrefabs()
        {
            NetworkManager networkManager = NetworkManager.Singleton;

            if (networkManager == null)
                return;

            AddLocalListPrefabs();
            AddItemsPrefabs();

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

        private void AddItemsPrefabs()
        {
            IEnumerable<ItemMeta> allItems = _itemsListConfig.GetAllItems();

            foreach (ItemMeta itemMeta in allItems)
            {
                GameObject prefab = itemMeta.ItemPrefab.gameObject;
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