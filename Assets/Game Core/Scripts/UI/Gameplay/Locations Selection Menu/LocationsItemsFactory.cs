using System.Collections.Generic;
using GameCore.Configs.Gameplay.LocationsList;
using GameCore.Enums.Global;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.Level.Locations;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using UnityEngine;

namespace GameCore.UI.Gameplay.LocationsSelectionMenu
{
    public class LocationsItemsFactory
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public LocationsItemsFactory(IGameplayConfigsProvider gameplayConfigsProvider,
            LocationsSelectionMenuView locationsSelectionMenuView, IGameManagerDecorator gameManagerDecorator,
            Transform container, LocationItemButtonView prefab)
        {
            _locationsListConfig = gameplayConfigsProvider.GetLocationsListConfig();
            _locationsSelectionMenuView = locationsSelectionMenuView;
            _gameManagerDecorator = gameManagerDecorator;
            _container = container;
            _prefab = prefab;
            _locationsItemsButtons = new List<LocationItemButtonView>(capacity: 5);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly LocationsListConfigMeta _locationsListConfig;
        private readonly LocationsSelectionMenuView _locationsSelectionMenuView;
        private readonly IGameManagerDecorator _gameManagerDecorator;
        private readonly Transform _container;
        private readonly LocationItemButtonView _prefab;
        private readonly List<LocationItemButtonView> _locationsItemsButtons;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Create()
        {
            Clear();
            CreationLogic();
        }

        public void UpdateSelectionState()
        {
            SceneName selectedLocation = _gameManagerDecorator.GetSelectedLocation();

            foreach (LocationItemButtonView locationItemButtonView in _locationsItemsButtons)
            {
                SceneName sceneName = locationItemButtonView.SceneName;
                bool isSelected = sceneName == selectedLocation;
                locationItemButtonView.ToggleSelected(isSelected);
            }
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CreationLogic()
        {
            IEnumerable<LocationMeta> allLocationsMeta = _locationsListConfig.GetAllLocationsMeta();
            SceneName selectedLocation = _gameManagerDecorator.GetSelectedLocation();

            foreach (LocationMeta locationMeta in allLocationsMeta)
            {
                string locationName = locationMeta.LocationName;
                SceneName sceneName = locationMeta.SceneName;
                bool isSelected = sceneName == selectedLocation;

                LocationItemButtonView locationItemButton = Object.Instantiate(_prefab, _container);
                locationItemButton.Setup(sceneName, locationName);
                locationItemButton.ToggleSelected(isSelected);
                locationItemButton.OnLocationItemClickedEvent += OnLocationItemClicked;

                _locationsItemsButtons.Add(locationItemButton);
            }
        }

        private void Clear()
        {
            foreach (LocationItemButtonView questItemButtonView in _locationsItemsButtons)
                Object.Destroy(questItemButtonView.gameObject);

            _locationsItemsButtons.Clear();
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnLocationItemClicked(SceneName sceneName) =>
            _locationsSelectionMenuView.SelectLocation(sceneName);
    }
}