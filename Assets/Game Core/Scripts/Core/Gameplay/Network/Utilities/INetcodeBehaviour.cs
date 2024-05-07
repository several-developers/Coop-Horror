namespace GameCore.Gameplay.Network.Utilities
{
    public interface INetcodeBehaviour
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
    
    public interface INetcodeTickBehaviour
    {
        void TickServerAndClient();
        void TickServer();
        void TickClient();
    }
}