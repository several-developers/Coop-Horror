using GameCore.Enums.Gameplay;

namespace GameCore.Gameplay.Levels
{
    public interface IFireExitsManager
    {
        void TeleportToFireExit(Floor floor, bool isInStairsLocation);
    }
}