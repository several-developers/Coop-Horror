using System.Collections.Generic;
using GameCore.Configs.Global.LocationsDatabase;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Level.Locations;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using GameCore.Infrastructure.Providers.Global;

namespace GameCore.Infrastructure.Providers.Gameplay.LocationsMeta
{
    public class LocationsMetaProvider : ILocationsMetaProvider
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public LocationsMetaProvider(IConfigsProvider configsProvider)
        {
            _locationsDatabaseConfig = configsProvider.GetConfig<LocationsDatabaseConfigMeta>();
            _locationsMeta = new Dictionary<LocationName, LocationMeta>();

            SetupLocationsDictionary();
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly LocationsDatabaseConfigMeta _locationsDatabaseConfig;
        private readonly Dictionary<LocationName, LocationMeta> _locationsMeta;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public bool TryGetLocationMeta(LocationName locationName, out LocationMeta locationMeta) =>
            _locationsMeta.TryGetValue(locationName, out locationMeta);

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SetupLocationsDictionary()
        {
            IEnumerable<LocationMeta> allAvailableLocationsMeta = _locationsDatabaseConfig.GetAllAvailableLocationsMeta();

            foreach (LocationMeta locationMeta in allAvailableLocationsMeta)
            {
                LocationName locationName = locationMeta.LocationName;
                bool isAdded = _locationsMeta.TryAdd(locationName, locationMeta);

                if (isAdded)
                    continue;

                Log.PrintError(log: $"Location Meta <gb>{locationName}</gb> <rb>already exists</rb>!");
            }
        }
    }
}