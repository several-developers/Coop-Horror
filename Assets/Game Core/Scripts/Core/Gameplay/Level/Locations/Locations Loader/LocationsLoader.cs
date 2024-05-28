using GameCore.Enums.Global;
using GameCore.Infrastructure.Services.Global;
using UnityEngine.SceneManagement;

namespace GameCore.Gameplay.Level.Locations
{
    public class LocationsLoader : ILocationsLoader
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public LocationsLoader(IScenesLoaderService scenesLoaderService) =>
            _scenesLoaderService = scenesLoaderService;

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly IScenesLoaderService _scenesLoaderService;

        private SceneName _lastLoadedLocation;
        private bool _isLocationLoaded;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void LoadLocationNetwork(SceneName sceneName)
        {
            if (_isLocationLoaded)
                return;

            _isLocationLoaded = true;
            _lastLoadedLocation = sceneName;

            _scenesLoaderService.LoadScene(sceneName, isNetwork: true, LoadSceneMode.Additive);
        }

        public void UnloadLastLocation()
        {
            if (!_isLocationLoaded)
                return;

            _isLocationLoaded = false;
            _scenesLoaderService.UnloadScene(_lastLoadedLocation, isNetwork: true);
        }
    }
}