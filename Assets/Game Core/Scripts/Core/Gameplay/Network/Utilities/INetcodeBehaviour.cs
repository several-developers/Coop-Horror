namespace GameCore.Gameplay.Network.Utilities
{
    public interface INetcodeBehaviour : INetcodeInitBehaviour, INetcodeUpdateBehaviour, INetcodeDespawnBehaviour
    {
    }

    public interface INetcodeInitBehaviour
    {
        void InitServerAndClient();
        void InitServer();
        void InitClient();
    }
    
    public interface INetcodeDespawnBehaviour
    {
        void DespawnServerAndClient();
        void DespawnServer();
        void DespawnClient();
    }
    
    public interface INetcodeUpdateBehaviour
    {
        void UpdateServerAndClient();
        void UpdateServer();
        void UpdateClient();
    }
}