using GameCore.Enums.Gameplay;
using GameCore.Gameplay.GameManagement;
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
        private void Construct(IGameManagerDecorator gameManagerDecorator) =>
            _gameManagerDecorator = gameManagerDecorator;

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

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();

            LocationChangedLogic();

            _gameManagerDecorator.OnSelectedLocationChangedEvent += OnSelectedLocationChanged;
        }

        private void OnDestroy() =>
            _gameManagerDecorator.OnSelectedLocationChangedEvent -= OnSelectedLocationChanged;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void ClickLogic() => SelectLocation(LocationName);

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void LocationChangedLogic()
        {
            bool isLocationMatches = IsLocationMatches();
            
            if (isLocationMatches)
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
        
        private bool IsLocationMatches()
        {
            LocationName selectedLocation = _gameManagerDecorator.GetSelectedLocation();
            return selectedLocation == _locationName;
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnSelectedLocationChanged(LocationName locationName) => LocationChangedLogic();
    }
}