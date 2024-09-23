using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GameCore.Configs.Global.LocationsDatabase;
using GameCore.Gameplay.Level.Locations;
using GameCore.Infrastructure.Providers.Global;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace GameCore.Gameplay.Storages.Assets
{
    public class ScenesAssetsStorage : AssetsStorage<string>, IScenesAssetsStorage
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ScenesAssetsStorage(IAssetsProvider assetsProvider, IConfigsProvider configsProvider) : base(assetsProvider)
        {
            _locationsDatabaseConfig = configsProvider.GetConfig<LocationsDatabaseConfigMeta>();
            _scenesPaths = new List<string>();
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly LocationsDatabaseConfigMeta _locationsDatabaseConfig;
        private readonly List<string> _scenesPaths;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override async UniTask WarmUp() =>
            await SetupAssetsReferences();

        public IEnumerable<string> GetAllScenesPath() =>
            _scenesPaths.ToList();

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private async UniTask SetupAssetsReferences()
        {
            IEnumerable<LocationMeta> allAvailableLocationsMeta = _locationsDatabaseConfig.GetAllAvailableLocationsMeta();

            foreach (LocationMeta locationMeta in allAvailableLocationsMeta)
            {
                AssetReference sceneAsset = locationMeta.SceneAsset;
                var operationHandle = Addressables.LoadResourceLocationsAsync(sceneAsset);

                IList<IResourceLocation> operationHandleTask = await operationHandle.Task;

                foreach (IResourceLocation resourceLocation in operationHandleTask)
                {
                    string scenePath = resourceLocation.InternalId;
                    
                    _scenesPaths.Add(scenePath);
                }
                
                Addressables.Release(operationHandle);
            }
        }
    }
}