using GameCore.Gameplay;

namespace GameCore.Infrastructure.StateMachine
{
    public interface IGameStateMachine
    {
        void AddState(IState state);
        void ChangeState<T>() where T : IState;
        bool TryGetCurrentState(out IState state);
    }
}