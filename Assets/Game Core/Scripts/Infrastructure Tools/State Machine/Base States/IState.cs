using Cysharp.Threading.Tasks;

namespace GameCore
{
    public interface IState
    {
    }

    public interface IEnterState : IState
    {
        void Enter();
    }
    
    public interface IEnterState<TEnterParams> : IState
    {
        void Enter(TEnterParams locationName);
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