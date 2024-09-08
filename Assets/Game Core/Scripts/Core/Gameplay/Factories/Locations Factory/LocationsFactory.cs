using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Configs.Global.LocationsList;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Level.Locations;
using GameCore.Gameplay.Network.DynamicPrefabs;
using GameCore.Gameplay.Utilities;
using GameCore.Infrastructure.Providers.Global;
using GameCore.Utilities;
using UnityEngine.AddressableAssets;

namespace GameCore.Gameplay.Factories.Locations
{
    public class LocationsFactory : AddressablesFactoryBase<LocationName>, ILocationsFactory
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public LocationsFactory(
            IAssetsProvider assetsProvider,
            IDynamicPrefabsLoaderDecorator dynamicPrefabsLoaderDecorator,
            IConfigsProvider configsProvider
        ) : base(assetsProvider, dynamicPrefabsLoaderDecorator)
        {
            _locationsListConfig = configsProvider.GetConfig<LocationsListConfigMeta>();
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly LocationsListConfigMeta _locationsListConfig;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override async UniTask WarmUp() =>
            await SetupAssetsReferences();

        public async UniTask CreateLocation<TLocation>(LocationName locationName, SpawnParams<TLocation> spawnParams)
            where TLocation : LocationManager
        {
            await LoadAndCreateGameObject(locationName, spawnParams);
        }

        public void CreateLocationDynamic<TLocation>(LocationName locationName, SpawnParams<TLocation> spawnParams)
            where TLocation : LocationManager
        {
            LoadAndCreateDynamicGameObject(locationName, spawnParams);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async UniTask SetupAssetsReferences()
        {
            IEnumerable<LocationsListConfigMeta.LocationReference> allLocationsReferences =
                _locationsListConfig.GetAllLocationsReferences();

            foreach (LocationsListConfigMeta.LocationReference locationReference in allLocationsReferences)
            {
                AssetReferenceGameObject locationPrefabAsset = locationReference.LocationPrefabAsset;

                await LoadAndReleaseAsset<LocationManager>(locationPrefabAsset);

                LocationName locationName = locationReference.LocationMeta.LocationName;
                AddAsset(locationName, locationPrefabAsset);
            }
        }
    }
}