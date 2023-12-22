using Unity.Netcode;

namespace GameCore.Gameplay.Entities
{
    public interface INetworkEntity : IEntity
    {
        NetworkObject GetNetworkObject();
    }
}