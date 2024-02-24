using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Levels.Elevator;

namespace GameCore.Gameplay.Levels
{
    public interface ILevelManager
    {
        void AddSurfaceElevator(SurfaceElevator surfaceElevator);
        void AddStairsFireExit(Floor floor, FireExit fireExit);
        void AddOtherFireExit(Floor floor, FireExit fireExit);
        void Clear();
        bool TryGetElevator(Floor floor, out ElevatorBase elevator);
        bool TryGetStairsFireExit(Floor floor, out FireExit fireExit);
        bool TryGetOtherFireExit(Floor floor, out FireExit fireExit);
    }
}