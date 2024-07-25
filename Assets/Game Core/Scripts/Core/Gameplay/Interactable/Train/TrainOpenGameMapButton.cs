using GameCore.Enums.Gameplay;
using GameCore.Gameplay.GameManagement;
using Zenject;

namespace GameCore.Gameplay.Interactable.Train
{
    public class TrainOpenGameMapButton : SimpleButton
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IGameManagerDecorator gameManagerDecorator) =>
            _gameManagerDecorator = gameManagerDecorator;

        // FIELDS: --------------------------------------------------------------------------------

        private IGameManagerDecorator _gameManagerDecorator;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InteractionStarted() =>
            _gameManagerDecorator.OnGameStateChangedEvent += OnGameStateChanged;

        public override void InteractionEnded() =>
            _gameManagerDecorator.OnGameStateChangedEvent -= OnGameStateChanged;

        public override bool CanInteract()
        {
            GameState gameState = _gameManagerDecorator.GetGameState();
            bool isGameStateValid = false;

            switch (gameState)
            {
                case GameState.WaitingForPlayers:
                case GameState.Gameplay:
                    isGameStateValid = true;
                    break;
            }

            if (!isGameStateValid)
                return false;

            LocationName currentLocation = _gameManagerDecorator.GetCurrentLocation();
            bool inBaseLocation = currentLocation == LocationName.Base;

            if (!inBaseLocation)
                return false;

            return IsInteractionEnabled;
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnGameStateChanged(GameState gameState) => SendInteractionStateChangedEvent();
    }
}