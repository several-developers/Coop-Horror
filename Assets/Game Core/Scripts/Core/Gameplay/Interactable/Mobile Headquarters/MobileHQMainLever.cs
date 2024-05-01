using GameCore.Enums.Gameplay;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.Quests;
using Zenject;

namespace GameCore.Gameplay.Interactable.MobileHeadquarters
{
    public class MobileHQMainLever : LeverBase
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IGameManagerDecorator gameManagerDecorator,
            IQuestsManagerDecorator questsManagerDecorator)
        {
            _gameManagerDecorator = gameManagerDecorator;
            _questsManagerDecorator = questsManagerDecorator;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private IGameManagerDecorator _gameManagerDecorator;
        private IQuestsManagerDecorator _questsManagerDecorator;

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

            if (activeQuestsAmount <= 0)
                return false;

            GameState gameState = _gameManagerDecorator.GetGameState();
            bool isGameStateValid = false;

            switch (gameState)
            {
                case GameState.WaitingForPlayers:
                case GameState.ReadyToLeaveTheRoad:
                case GameState.ReadyToLeaveTheLocation:
                    isGameStateValid = true;
                    break;
            }

            return isGameStateValid && IsInteractionEnabled;
        }
        
        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnGameStateChanged(GameState gameState) => SendInteractionStateChangedEvent();

        private void OnActiveQuestsDataReceived() => SendInteractionStateChangedEvent();
    }
}