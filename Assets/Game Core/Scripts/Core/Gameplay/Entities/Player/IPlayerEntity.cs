namespace GameCore.Gameplay.Entities.Player
{
    public interface IPlayerEntity : INetworkEntity, IDamageable
    {
        void Setup();
    }
}