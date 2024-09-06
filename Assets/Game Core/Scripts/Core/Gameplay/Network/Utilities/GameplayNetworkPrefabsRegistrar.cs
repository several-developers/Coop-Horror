using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Configs.Gameplay.ItemsList;
using GameCore.Configs.Global.EntitiesList;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.Items;
using GameCore.Gameplay.Network.DynamicPrefabs;
using GameCore.Gameplay.Network.PrefabsRegistrar;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using GameCore.Infrastructure.Providers.Global;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

namespace GameCore.Gameplay.Network.Utilities
{
    public class GameplayNetworkPrefabsRegistrar : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(
            DiContainer diContainer,
            IAssetsProvider assetsProvider,
            IConfigsProvider configsProvider,
            IGameplayConfigsProvider gameplayConfigsProvider,
            INetworkPrefabsRegistrar networkPrefabsRegistrar
        )
        {
            _diContainer = diContainer;
            _assetsProvider = assetsProvider;
            _networkPrefabsRegistrar = networkPrefabsRegistrar;

            _entitiesListConfig = configsProvider.GetConfig<EntitiesListConfigMeta>();
            _itemsListConfig = gameplayConfigsProvider.GetConfig<ItemsListConfigMeta>();
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        [ListDrawerSettings(AlwaysAddDefaultValue = true)]
        private List<GameObject> _prefabs;

        // FIELDS: --------------------------------------------------------------------------------

        private readonly List<GameObject> _prefabsToRegister = new();

        private DiContainer _diContainer;
        private IAssetsProvider _assetsProvider;
        private INetworkPrefabsRegistrar _networkPrefabsRegistrar;
        
        private EntitiesListConfigMeta _entitiesListConfig;
        private ItemsListConfigMeta _itemsListConfig;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            DynamicPrefabLoadingUtilities.SetDiContainer(_diContainer);
            RegisterPrefabs();
        }

        private void Start() => RegisterAddressables();

        private void OnDestroy() => RemovePrefabs();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async void RegisterAddressables()
        {
            await RegisterEntities();
            GameManager.Instance.SendPlayerLoadedServerRpc();
        }

        private async UniTask RegisterEntities()
        {
            IEnumerable<AssetReferenceGameObject> allReferences = _entitiesListConfig.GetAllReferences();

            foreach (AssetReferenceGameObject assetReference in allReferences)
                await LoadAndRegisterAsset(assetReference);
        }

        private async UniTask LoadAndRegisterAsset(AssetReference assetReference)
        {
            var prefab = await _assetsProvider.LoadAsset<GameObject>(assetReference);
            RegisterPrefab(prefab);
            _assetsProvider.ReleaseAsset(assetReference);
        }

        private void RegisterPrefabs()
        {
            AddLocalListPrefabs();
            AddItemsPrefabs();

            foreach (GameObject prefab in _prefabsToRegister)
                RegisterPrefab(prefab);
        }

        private void RegisterPrefab(GameObject prefab) =>
            _networkPrefabsRegistrar.Register(prefab);

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
            foreach (GameObject prefab in _prefabsToRegister)
                _networkPrefabsRegistrar.Remove(prefab);
        }
    }
}