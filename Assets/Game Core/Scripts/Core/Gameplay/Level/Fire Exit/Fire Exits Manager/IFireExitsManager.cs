using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities;

namespace GameCore.Gameplay.Level
{
    public interface IFireExitsManager
    {
        void TeleportLocalPlayerToFireExit(Floor floor, bool isInStairsLocation);
        void TeleportEntityToFireExit(ITeleportableEntity entity, Floor floor, bool isInStairsLocation);
    }
}