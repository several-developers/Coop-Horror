namespace GameCore
{
    public interface IFixedTickableState : IState
    {
        void FixedTick();
    }
}