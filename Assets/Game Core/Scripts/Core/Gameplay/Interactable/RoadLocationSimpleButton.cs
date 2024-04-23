using GameCore.Enums.Gameplay;
using GameCore.Gameplay.GameManagement;
using Zenject;

namespace GameCore.Gameplay.Interactable
{
    public class RoadLocationSimpleButton : SimpleButton
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IGameManagerDecorator gameManagerDecorator) =>
            _gameManagerDecorator = gameManagerDecorator;

        // FIELDS: --------------------------------------------------------------------------------
        
        private IGameManagerDecorator _gameManagerDecorator;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override bool CanInteract()
        {
            LocationState locationState = _gameManagerDecorator.GetLocationState();
            bool isLocationStateValid = locationState == LocationState.Road;
            return isLocationStateValid && IsInteractionEnabled;
        }
    }
}