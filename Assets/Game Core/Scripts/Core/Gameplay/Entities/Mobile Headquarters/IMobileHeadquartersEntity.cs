namespace GameCore.Gameplay.Entities.MobileHeadquarters
{
    public interface IMobileHeadquartersEntity : IEntity, INetworkObject
    {
        void ArriveAtRoadLocation();
        void LeftLocation();
    }
}