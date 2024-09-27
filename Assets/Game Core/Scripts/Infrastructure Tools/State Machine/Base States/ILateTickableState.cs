namespace GameCore
{
    public interface ILateTickableState : IState
    {
        void LateTick();
    }
}