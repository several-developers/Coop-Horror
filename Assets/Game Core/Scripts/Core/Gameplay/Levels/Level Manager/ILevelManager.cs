using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Levels.Elevator;

namespace GameCore.Gameplay.Levels
{
    public interface ILevelManager
    {
        void AddSurfaceElevator(SurfaceElevator surfaceElevator);
        void Clear();
        bool TryGetElevator(ElevatorFloor elevatorFloor, out ElevatorBase elevator);
    }
}