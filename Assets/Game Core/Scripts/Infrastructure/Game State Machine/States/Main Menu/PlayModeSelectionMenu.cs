using GameCore.Gameplay;

namespace GameCore.Infrastructure.StateMachine
{
    public class PlayModeSelectionMenu : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public PlayModeSelectionMenu(IGameStateMachine gameStateMachine)
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

        public void Exit()
        {
        }
    }
}