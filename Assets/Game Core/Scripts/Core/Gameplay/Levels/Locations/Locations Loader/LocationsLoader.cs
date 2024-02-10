using System;
using GameCore.Enums;
using GameCore.Infrastructure.Services.Global;

namespace GameCore.Gameplay.Levels.Locations
{
    public class LocationsLoader : ILocationsLoader
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public LocationsLoader(IScenesLoaderService scenesLoaderService)
        {
            _scenesLoaderService = scenesLoaderService;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly IScenesLoaderService _scenesLoaderService;

        private SceneName _lastLoadedLocation;
        private bool _isLocationLoaded;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void LoadLocationNetwork(SceneName sceneName, Action callback = null)
        {
            if (_isLocationLoaded)
                return;

            _isLocationLoaded = true;
            _lastLoadedLocation = sceneName;
            
            _scenesLoaderService.LoadSceneNetworkAdditive(sceneName, callback);
        }

        public void UnloadLastLocationNetwork()
        {
            if (!_isLocationLoaded)
                return;

            _isLocationLoaded = false;
            _scenesLoaderService.UnloadSceneNetwork(_lastLoadedLocation);
        }
    }
}