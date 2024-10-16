﻿using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.Systems.Quests;
using Zenject;

namespace GameCore.Gameplay.Interactable.Train
{
    public class TrainCompleteQuestsButton : SimpleButton
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

        public override void InteractionStarted(IEntity entity = null) =>
            _questsManagerDecorator.OnActiveQuestsDataReceivedEvent += OnActiveQuestsDataReceived;

        public override void InteractionEnded(IEntity entity = null) =>
            _questsManagerDecorator.OnActiveQuestsDataReceivedEvent -= OnActiveQuestsDataReceived;

        public override bool CanInteract()
        {
            int activeQuestsAmount = _questsManagerDecorator.GetActiveQuestsAmount();

            if (activeQuestsAmount <= 0)
                return false;

            bool containsExpiredAndUncompletedQuests = _questsManagerDecorator.ContainsExpiredAndUncompletedQuests();

            if (containsExpiredAndUncompletedQuests)
                return false;

            bool containsCompletedQuests = _questsManagerDecorator.ContainsCompletedQuests();

            if (!containsCompletedQuests)
                return false;
            
            GameState gameState = _gameManagerDecorator.GetGameState();
            bool isGameStateValid = false;

            switch (gameState)
            {
                case GameState.Gameplay:
                    isGameStateValid = true;
                    break;
            }

            if (!isGameStateValid)
                return false;

            return IsInteractionEnabled;
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnActiveQuestsDataReceived() => SendInteractionStateChangedEvent();
    }
}