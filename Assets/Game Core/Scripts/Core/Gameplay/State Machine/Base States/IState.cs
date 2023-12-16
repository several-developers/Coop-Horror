using Cysharp.Threading.Tasks;

namespace GameCore.Gameplay
{
    public interface IState
    {
    }

    public interface IEnterState : IState
    {
        void Enter();
    }

    public interface IExitState : IState
    {
        void Exit();
    }

    public interface IEnterStateAsync : IState
    {
        UniTaskVoid Enter();
    }

    public interface IExitStateAsync : IState
    {
        UniTaskVoid Exit();
    }
}