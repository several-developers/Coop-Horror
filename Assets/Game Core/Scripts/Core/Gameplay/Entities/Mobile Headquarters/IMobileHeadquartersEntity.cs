using Cinemachine;

namespace GameCore.Gameplay.Entities.MobileHeadquarters
{
    public interface IMobileHeadquartersEntity : IEntity, INetworkObject
    {
        void ChangePath(CinemachinePath path);
    }
}