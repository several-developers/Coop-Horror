using GameCore.Enums.Gameplay;

namespace GameCore.Gameplay.Levels.Elevator
{
    public struct ElevatorStaticData
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ElevatorStaticData(Floor currentFloor, Floor targetFloor, bool isMovingUp)
        {
            CurrentFloor = currentFloor;
            TargetFloor = targetFloor;
            IsMovingUp = isMovingUp;
            IsTargetFloor = currentFloor == targetFloor;
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public Floor CurrentFloor { get; }
        public Floor TargetFloor { get; }
        public bool IsMovingUp { get; }
        public bool IsTargetFloor { get; }
    }
}