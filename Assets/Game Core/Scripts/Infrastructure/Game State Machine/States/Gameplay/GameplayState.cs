using GameCore.Gameplay;
using GameCore.Gameplay.Observers.UI;

namespace GameCore.Infrastructure.StateMachine
{
    public class GameplayState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public GameplayState(IGameStateMachine gameStateMachine, IUIObserver uiObserver)
        {
            _gameStateMachine = gameStateMachine;
            _uiObserver = uiObserver;

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;
        private readonly IUIObserver _uiObserver;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter() =>
            _uiObserver.ShowGameplayHUD();

        public void Exit() =>
            _uiObserver.HideGameplayHUD();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void EnterGameOverState() =>
            _gameStateMachine.ChangeState<GameOverState>();
    }
}