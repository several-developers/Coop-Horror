using GameCore.Enums.Gameplay;
using GameCore.Enums.Global;
using GameCore.Infrastructure.Providers.Gameplay.LocationsMeta;
using GameCore.Infrastructure.Services.Global;
using UnityEngine.SceneManagement;

namespace GameCore.Gameplay.Level.Locations
{
    public class LocationsLoader : ILocationsLoader
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public LocationsLoader(IScenesLoaderService scenesLoaderService, ILocationsMetaProvider locationsMetaProvider)
        {
            _scenesLoaderService = scenesLoaderService;
            _locationsMetaProvider = locationsMetaProvider;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly IScenesLoaderService _scenesLoaderService;
        private readonly ILocationsMetaProvider _locationsMetaProvider;

        private SceneName _lastLoadedScene;
        private bool _isLocationLoaded;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void LoadSceneNetwork(SceneName sceneName)
        {
            if (_isLocationLoaded)
                return;

            _isLocationLoaded = true;
            _lastLoadedScene = sceneName;

            _scenesLoaderService.LoadScene(sceneName, isNetwork: true, LoadSceneMode.Additive);
        }

        public void LoadLocationNetwork(LocationName locationName)
        {
            bool isBase = locationName == LocationName.Base;

            if (isBase)
            {
                UnloadLastScene();
                return;
            }

            bool isLocationMetaFound =
                _locationsMetaProvider.TryGetLocationMeta(locationName, out LocationMeta locationMeta);

            if (!isLocationMetaFound)
            {
                Log.PrintError(log: $"Location Meta <gb>{locationName}<gb> <rb>not found</rb>!");
                return;
            }

            SceneName sceneName = locationMeta.SceneName;
            LoadSceneNetwork(sceneName);
        }

        public void UnloadLastScene()
        {
            if (!_isLocationLoaded)
                return;

            _isLocationLoaded = false;
            _scenesLoaderService.UnloadScene(_lastLoadedScene, isNetwork: true);
        }
    }
}