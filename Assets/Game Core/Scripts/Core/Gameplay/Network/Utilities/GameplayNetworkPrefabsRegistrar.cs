using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Configs.Global.EntitiesList;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.Network.DynamicPrefabs;
using GameCore.Gameplay.Network.PrefabsRegistrar;
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
            INetworkPrefabsRegistrar networkPrefabsRegistrar
        )
        {
            _diContainer = diContainer;
            _assetsProvider = assetsProvider;
            _networkPrefabsRegistrar = networkPrefabsRegistrar;

            _entitiesListConfig = configsProvider.GetConfig<EntitiesListConfigMeta>();
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

            foreach (GameObject prefab in _prefabsToRegister)
                RegisterPrefab(prefab);
        }

        private void RegisterPrefab(GameObject prefab) =>
            _networkPrefabsRegistrar.Register(prefab);

        private void AddLocalListPrefabs() =>
            _prefabsToRegister.AddRange(_prefabs);

        private void RemovePrefabs()
        {
            foreach (GameObject prefab in _prefabsToRegister)
                _networkPrefabsRegistrar.Remove(prefab);
        }
    }
}