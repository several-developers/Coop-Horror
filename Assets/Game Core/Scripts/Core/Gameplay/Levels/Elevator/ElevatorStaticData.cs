using GameCore.Enums.Gameplay;

namespace GameCore.Gameplay.Levels.Elevator
{
    public struct ElevatorStaticData
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ElevatorStaticData(ElevatorFloor currentFloor, ElevatorFloor targetFloor, bool isMovingUp)
        {
            CurrentFloor = currentFloor;
            TargetFloor = targetFloor;
            IsMovingUp = isMovingUp;
            IsTargetFloor = currentFloor == targetFloor;
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public ElevatorFloor CurrentFloor { get; }
        public ElevatorFloor TargetFloor { get; }
        public bool IsMovingUp { get; }
        public bool IsTargetFloor { get; }
    }
}