namespace GameCore.Gameplay.PubSub
{
    public interface IPublisher<T>
    {
        void Publish(T message);
    }
}