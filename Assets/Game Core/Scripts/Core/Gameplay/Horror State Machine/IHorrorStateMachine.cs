namespace GameCore.Gameplay.HorrorStateMachineSpace
{
    public interface IHorrorStateMachine
    {
        void AddState(IState state);
        void ChangeState<TState>() where TState : IState;
        void ChangeState<TState, TEnterParams>(TEnterParams enterParams) where TState : IState;
        bool TryGetCurrentState(out IState state);
    }
}