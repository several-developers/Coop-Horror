using Unity.Netcode;

namespace GameCore.Gameplay.Entities
{
    public interface INetworkObject
    {
        NetworkObject GetNetworkObject();
    }
}