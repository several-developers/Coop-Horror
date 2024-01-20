namespace GameCore.Gameplay.HorrorStateMachineSpace
{
    public interface IHorrorStateMachine
    {
        void AddState(IState state);
        void ChangeState<T>() where T : IState;
        bool TryGetCurrentState(out IState state);
    }
}