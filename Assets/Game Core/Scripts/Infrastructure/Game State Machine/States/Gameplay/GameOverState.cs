using GameCore.Gameplay;

namespace GameCore.Infrastructure.StateMachine
{
    public class GameOverState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public GameOverState(IGameStateMachine gameStateMachine)
        {
            _gameStateMachine = gameStateMachine;

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            
        }
    }
}