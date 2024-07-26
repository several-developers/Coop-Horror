namespace GameCore.Gameplay.PubSub
{
    public interface IBufferedMessageChannel<T> : IMessageChannel<T>
    {
        bool HasBufferedMessage { get; }
        T BufferedMessage { get; }
    }
}