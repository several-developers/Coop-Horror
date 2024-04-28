using GameCore.Enums.Gameplay;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.Quests;
using Zenject;

namespace GameCore.Gameplay.Interactable.MobileHeadquarters
{
    public class MobileHQCompleteQuestsButton : SimpleButton
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

        public override bool CanInteract()
        {
            int activeQuestsAmount = _questsManagerDecorator.GetActiveQuestsAmount();

            if (activeQuestsAmount <= 0)
                return false;

            bool containsExpiredAndUncompletedQuests = _questsManagerDecorator.ContainsExpiredAndUncompletedQuests();

            if (containsExpiredAndUncompletedQuests)
                return false;
            
            GameState gameState = _gameManagerDecorator.GetGameState();
            bool isGameStateValid = false;

            switch (gameState)
            {
                case GameState.ReadyToLeaveTheRoad:
                    isGameStateValid = true;
                    break;
            }

            return isGameStateValid && IsInteractionEnabled;
        }
    }
}