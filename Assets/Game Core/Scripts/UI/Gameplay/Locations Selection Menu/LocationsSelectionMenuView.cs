using GameCore.Enums.Global;
using GameCore.Gameplay.GameManagement;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using GameCore.UI.Global.MenuView;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GameCore.UI.Gameplay.LocationsSelectionMenu
{
    public class LocationsSelectionMenuView : MenuView
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IGameManagerDecorator gameManagerDecorator,
            IGameplayConfigsProvider gameplayConfigsProvider)
        {
            _gameManagerDecorator = gameManagerDecorator;

            _locationsItemsFactory = new LocationsItemsFactory(gameplayConfigsProvider,
                locationsSelectionMenuView: this, gameManagerDecorator, _locationsItemsContainer,
                _locationItemButtonPrefab);
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Transform _locationsItemsContainer;

        [SerializeField, Required]
        private LocationItemButtonView _locationItemButtonPrefab;

        [SerializeField, Required]
        private LayoutGroup _locationsItemsLayoutGroup;
        
        [SerializeField, Required]
        private ContentSizeFitter _locationsItemsSizeFitter;

        // FIELDS: --------------------------------------------------------------------------------

        private IGameManagerDecorator _gameManagerDecorator;
        private LocationsItemsFactory _locationsItemsFactory;
        private LayoutFixHelper _layoutFixHelper;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _layoutFixHelper = new LayoutFixHelper(coroutineRunner: this, _locationsItemsLayoutGroup,
                _locationsItemsSizeFitter);

            _gameManagerDecorator.OnSelectedLocationChangedEvent += OnSelectedLocationChanged;
        }

        private void Start() => CreateLocationsItems();

        private void OnDestroy() =>
            _gameManagerDecorator.OnSelectedLocationChangedEvent -= OnSelectedLocationChanged;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SelectLocation(SceneName locationName) =>
            _gameManagerDecorator.SelectLocation(locationName);

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CreateLocationsItems()
        {
            _locationsItemsFactory.Create();
            _layoutFixHelper.FixLayout();
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnSelectedLocationChanged(SceneName locationName) =>
            _locationsItemsFactory.UpdateSelectionState();
    }
}