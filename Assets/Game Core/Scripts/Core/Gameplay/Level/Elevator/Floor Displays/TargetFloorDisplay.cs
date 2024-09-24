using GameCore.Enums.Gameplay;

namespace GameCore.Gameplay.Level.Elevator
{
    public class TargetFloorDisplay : FloorDisplayBase
    {
        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void Start()
        {
            base.Start();

            ElevatorEntity.OnTargetFloorChangedEvent += OnTargetFloorChanged;
        }

        private void OnDestroy()
        {
            ElevatorEntity.OnTargetFloorChangedEvent -= OnTargetFloorChanged;
        }

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void UpdateFloorNumber()
        {
            Floor targetFloor = ElevatorEntity.GetTargetFloor();
            UpdateFloorNumber(targetFloor);
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnTargetFloorChanged(Floor floor) => UpdateFloorNumber(floor);
    }
}