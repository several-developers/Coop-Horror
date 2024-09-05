namespace GameCore
{
    public interface ITickableState : IState
    {
        void Tick();
    }
}