using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Factories.Locations;
using GameCore.Gameplay.Utilities;
using UnityEngine;

namespace GameCore.Gameplay.Level.Locations
{
    public class LocationsLoader : ILocationsLoader
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public LocationsLoader(ILocationsFactory locationsFactory) =>
            _locationsFactory = locationsFactory;

        // FIELDS: --------------------------------------------------------------------------------

        private readonly ILocationsFactory _locationsFactory;

        private bool _isLocationLoaded;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void LoadLocation(LocationName locationName)
        {
            bool isBase = locationName == LocationName.Base;

            if (isBase)
            {
                UnloadLastLocation();
                return;
            }

            var spawnParams = new SpawnParams<LocationManager>.Builder()
                .Build();
            
            _locationsFactory.CreateLocation(locationName, spawnParams);
        }

        public void UnloadLastLocation()
        {
            LocationManager locationManager = LocationManager.Get();

            if (locationManager == null)
                return;

            Object.Destroy(locationManager.gameObject);
        }
    }
}