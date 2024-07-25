using GameCore.Enums.Gameplay;
using GameCore.Gameplay.GameManagement;
using GameCore.Infrastructure.Providers.Gameplay.LocationsMeta;
using GameCore.Observers.Gameplay.UIManager;
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
        private void Construct(
            IGameManagerDecorator gameManagerDecorator,
            IUIManagerObserver uiManagerObserver,
            ILocationsMetaProvider locationsMetaProvider
        )
        {
            _gameManagerDecorator = gameManagerDecorator;
            _uiManagerObserver = uiManagerObserver;

            _locationsItemsFactory = new LocationsItemsFactory(locationsMetaProvider, locationsSelectionMenuView: this,
                gameManagerDecorator, _locationsItemsContainer, _locationItemButtonPrefab);
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
        private IUIManagerObserver _uiManagerObserver;
        private LocationsItemsFactory _locationsItemsFactory;
        private LayoutFixHelper _layoutFixHelper;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();

            _layoutFixHelper = new LayoutFixHelper(coroutineRunner: this, _locationsItemsLayoutGroup,
                _locationsItemsSizeFitter);

            _gameManagerDecorator.OnSelectedLocationChangedEvent += OnSelectedLocationChanged;
        }

        private void Start() => CreateLocationsItems();

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _gameManagerDecorator.OnSelectedLocationChangedEvent -= OnSelectedLocationChanged;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SelectLocation(LocationName locationName) =>
            _gameManagerDecorator.SelectLocation(locationName);

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CreateLocationsItems()
        {
            _locationsItemsFactory.Create();
            _layoutFixHelper.FixLayout();
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        protected override void OnShowMenu() =>
            _uiManagerObserver.MenuShown(menuView: this);

        protected override void OnHideMenu() =>
            _uiManagerObserver.MenuHidden(menuView: this);

        private void OnSelectedLocationChanged(LocationName locationName) =>
            _locationsItemsFactory.UpdateSelectionState();
    }
}