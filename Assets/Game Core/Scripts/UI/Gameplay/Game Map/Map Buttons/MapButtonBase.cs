using GameCore.Enums.Gameplay;
using GameCore.Gameplay.GameManagement;
using GameCore.Observers.Gameplay.UI;
using GameCore.UI.Global.Buttons;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.UI.Gameplay.GameMap
{
    public abstract class MapButtonBase : BaseButton
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IGameManagerDecorator gameManagerDecorator, IUIObserver uiObserver)
        {
            _gameManagerDecorator = gameManagerDecorator;
            _uiObserver = uiObserver;
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private LocationName _locationName;
        
        [Title(Constants.References)]
        [SerializeField, Required]
        private MapButtonAnimation _buttonAnimation;

        // PROPERTIES: ----------------------------------------------------------------------------

        protected LocationName LocationName => _locationName;
        
        // FIELDS: --------------------------------------------------------------------------------
        
        private IGameManagerDecorator _gameManagerDecorator;
        private IUIObserver _uiObserver;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();

            LocationChangedLogic();

            _gameManagerDecorator.OnCurrentLocationChangedEvent += OnGameLocationChanged;
            _gameManagerDecorator.OnSelectedLocationChangedEvent += OnGameLocationChanged;
            
            _uiObserver.OnTriggerUIEvent += OnTriggerUIEvent;
        }

        private void OnDestroy()
        {
            _gameManagerDecorator.OnCurrentLocationChangedEvent -= OnGameLocationChanged;
            _gameManagerDecorator.OnSelectedLocationChangedEvent -= OnGameLocationChanged;
            
            _uiObserver.OnTriggerUIEvent -= OnTriggerUIEvent;
        }

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void ClickLogic() => SelectLocation(LocationName);

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        [Button]
        private void LocationChangedLogic()
        {
            bool isCurrentLocationMatches = IsCurrentLocationMatches();
            bool isSelectedLocationMatches = IsSelectedLocationMatches();
            bool playAnimation = isCurrentLocationMatches || isSelectedLocationMatches;
            
            _buttonAnimation.ToggleUseGreenPing(isCurrentLocationMatches);
            
            if (playAnimation)
                PlayButtonAnimation();
            else
                StopButtonAnimation();
        }
        
        private void SelectLocation(LocationName locationName) =>
            _gameManagerDecorator.SelectLocation(locationName);
        
        private void PlayButtonAnimation() =>
            _buttonAnimation.PlayAnimation();

        private void StopButtonAnimation() =>
            _buttonAnimation.StopAnimation();
        
        private bool IsCurrentLocationMatches()
        {
            LocationName currentLocation = _gameManagerDecorator.GetCurrentLocation();
            return currentLocation == _locationName;
        }
        
        private bool IsSelectedLocationMatches()
        {
            LocationName selectedLocation = _gameManagerDecorator.GetSelectedLocation();
            return selectedLocation == _locationName;
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnGameLocationChanged(LocationName locationName) => LocationChangedLogic();

        private void OnTriggerUIEvent(UIEventType eventType)
        {
            if (eventType != UIEventType.UpdateGameMap)
                return;
            
            LocationChangedLogic();
        }
    }
}