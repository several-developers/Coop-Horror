﻿using GameCore.Enums.Gameplay;
using GameCore.Gameplay.GameManagement;
using Zenject;

namespace GameCore.Gameplay.Interactable.MobileHeadquarters
{
    public class MobileHQQuestsButton : SimpleButton
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