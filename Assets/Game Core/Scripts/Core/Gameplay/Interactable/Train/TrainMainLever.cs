using GameCore.Configs.Gameplay.Quests;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.Quests;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using Zenject;

namespace GameCore.Gameplay.Interactable.Train
{
    public class TrainMainLever : LeverBase
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IGameManagerDecorator gameManagerDecorator,
            IQuestsManagerDecorator questsManagerDecorator, IGameplayConfigsProvider gameplayConfigsProvider)
        {
            _gameManagerDecorator = gameManagerDecorator;
            _questsManagerDecorator = questsManagerDecorator;
            _questsConfig = gameplayConfigsProvider.GetQuestsConfig();
        }

        // FIELDS: --------------------------------------------------------------------------------

        private IGameManagerDecorator _gameManagerDecorator;
        private IQuestsManagerDecorator _questsManagerDecorator;
        private QuestsConfigMeta _questsConfig;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InteractionStarted()
        {
            _gameManagerDecorator.OnGameStateChangedEvent += OnGameStateChanged;
            _questsManagerDecorator.OnActiveQuestsDataReceivedEvent += OnActiveQuestsDataReceived;
        }

        public override void InteractionEnded()
        {
            _gameManagerDecorator.OnGameStateChangedEvent -= OnGameStateChanged;
            _questsManagerDecorator.OnActiveQuestsDataReceivedEvent -= OnActiveQuestsDataReceived;
        }

        public override InteractionType GetInteractionType() =>
            InteractionType.MobileHQMainLever;

        public override bool CanInteract()
        {
            int activeQuestsAmount = _questsManagerDecorator.GetActiveQuestsAmount();
            bool ignoreQuestsCheck = _questsConfig.IgnoreMobileHQQuestsCheck;

            if (activeQuestsAmount <= 0 && !ignoreQuestsCheck)
                return false;

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
            LocationName selectedLocation = _gameManagerDecorator.GetSelectedLocation();
            bool isLocationsMatches = currentLocation == selectedLocation;

            if (isLocationsMatches)
                return false;

            return IsInteractionEnabled;
        }
        
        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnGameStateChanged(GameState gameState) => SendInteractionStateChangedEvent();

        private void OnActiveQuestsDataReceived() => SendInteractionStateChangedEvent();
    }
}