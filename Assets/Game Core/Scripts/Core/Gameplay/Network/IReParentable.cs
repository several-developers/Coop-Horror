using Unity.Netcode;

namespace GameCore.Gameplay.Network
{
    public interface IReParentable
    {
        void SetParent(NetworkObject newParent);
        void RemoveParent();
    }
}