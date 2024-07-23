using System;
using GameCore.Enums.Gameplay;

namespace GameCore.Gameplay.GameManagement
{
    public class GameStatesHandler : IDisposable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public GameStatesHandler(IGameManagerDecorator gameManagerDecorator)
        {
            _gameManagerDecorator = gameManagerDecorator;

            _gameManagerDecorator.OnGameStateChangedEvent += OnGameStateChanged;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameManagerDecorator _gameManagerDecorator;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Dispose()
        {
            _gameManagerDecorator.OnGameStateChangedEvent -= OnGameStateChanged;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void HandleState(GameState gameState)
        {
            switch (gameState)
            {
                
            }
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnGameStateChanged(GameState gameState)
        {
            
        }
    }
}