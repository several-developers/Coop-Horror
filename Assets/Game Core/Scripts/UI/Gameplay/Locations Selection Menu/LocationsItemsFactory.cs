using System.Collections.Generic;
using GameCore.Configs.Gameplay.LocationsList;
using GameCore.Enums.Gameplay;
using GameCore.Enums.Global;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.Level.Locations;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using GameCore.Infrastructure.Providers.Gameplay.LocationsMeta;
using UnityEngine;

namespace GameCore.UI.Gameplay.LocationsSelectionMenu
{
    public class LocationsItemsFactory
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public LocationsItemsFactory(
            ILocationsMetaProvider locationsMetaProvider,
            LocationsSelectionMenuView locationsSelectionMenuView,
            IGameManagerDecorator gameManagerDecorator,
            Transform container,
            LocationItemButtonView prefab
        )
        {
            _locationsMetaProvider = locationsMetaProvider;
            _locationsSelectionMenuView = locationsSelectionMenuView;
            _gameManagerDecorator = gameManagerDecorator;
            _container = container;
            _prefab = prefab;
            _locationsItemsButtons = new List<LocationItemButtonView>(capacity: 5);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly ILocationsMetaProvider _locationsMetaProvider;
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
            LocationName selectedLocation = _gameManagerDecorator.GetSelectedLocation();

            foreach (LocationItemButtonView locationItemButtonView in _locationsItemsButtons)
            {
                LocationName locationName = locationItemButtonView.LocationName;
                bool isSelected = locationName == selectedLocation;
                locationItemButtonView.ToggleSelected(isSelected);
            }
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CreationLogic()
        {
            IEnumerable<LocationMeta> allLocationsMeta = _locationsMetaProvider.GetAllLocationsMeta();
            LocationName selectedLocation = _gameManagerDecorator.GetSelectedLocation();

            foreach (LocationMeta locationMeta in allLocationsMeta)
            {
                string locationName = locationMeta.LocationNameText;
                LocationName sceneName = locationMeta.LocationName;
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

        private void OnLocationItemClicked(LocationName locationName) =>
            _locationsSelectionMenuView.SelectLocation(locationName);
    }
}