using GameCore.Gameplay;
using GameCore.Gameplay.Observers;

namespace GameCore.Infrastructure.StateMachine
{
    public class GameStateMachine : IGameStateMachine
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public GameStateMachine(IGameStateMachineObserver gameStateMachineObserver)
        {
            _gameStateMachineObserver = gameStateMachineObserver;
            _stateMachine = new Gameplay.StateMachine();
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly Gameplay.StateMachine _stateMachine;
        private readonly IGameStateMachineObserver _gameStateMachineObserver;

        private IState _currentState;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void AddState(IState state) =>
            _stateMachine.AddStateWithRemove(state);

        public void ChangeState<T>() where T : IState
        {
            _stateMachine.ChangeState<T>();
            _gameStateMachineObserver.SendStateChanged();
        }

        public bool TryGetCurrentState(out IState state) =>
            _stateMachine.TryGetCurrentState(out state);
    }
}