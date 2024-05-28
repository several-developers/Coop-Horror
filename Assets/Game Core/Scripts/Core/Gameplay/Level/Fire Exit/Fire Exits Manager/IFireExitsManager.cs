using GameCore.Enums.Gameplay;

namespace GameCore.Gameplay.Level
{
    public interface IFireExitsManager
    {
        void TeleportToFireExit(Floor floor, bool isInStairsLocation);
    }
}