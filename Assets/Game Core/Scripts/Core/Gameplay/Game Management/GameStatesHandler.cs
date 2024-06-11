using System;

namespace GameCore.Gameplay.GameManagement
{
    public class GameStatesHandler : IDisposable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public GameStatesHandler(IGameManagerDecorator gameManagerDecorator) =>
            _gameManagerDecorator = gameManagerDecorator;

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameManagerDecorator _gameManagerDecorator;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Dispose()
        {
            
        }
    }
}