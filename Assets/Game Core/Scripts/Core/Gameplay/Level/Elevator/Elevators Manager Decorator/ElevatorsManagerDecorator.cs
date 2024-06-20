using System;
using GameCore.Enums.Gameplay;

namespace GameCore.Gameplay.Level.Elevator
{
    public class ElevatorsManagerDecorator : IElevatorsManagerDecorator
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action<ElevatorStaticData> OnElevatorStartedEvent = delegate {  };
        public event Action<ElevatorStaticData> OnFloorChangedEvent = delegate {  };
        public event Action<Floor> OnElevatorStoppedEvent = delegate {  };
        public event Action<Floor> OnElevatorOpenedEvent = delegate { };

        public event Action<Floor> OnStartElevatorInnerEvent = delegate { };
        public event Action<Floor> OnOpenElevatorInnerEvent = delegate { };
        public event Func<Floor> GetCurrentFloorInnerEvent;
        public event Func<bool> IsElevatorMovingInnerEvent;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void ElevatorStarted(ElevatorStaticData data) =>
            OnElevatorStartedEvent.Invoke(data);

        public void FloorChanged(ElevatorStaticData data) =>
            OnFloorChangedEvent.Invoke(data);

        public void ElevatorStopped(Floor floor) =>
            OnElevatorStoppedEvent.Invoke(floor);

        public void ElevatorOpened(Floor floor) =>
            OnElevatorOpenedEvent.Invoke(floor);

        public void StartElevator(Floor floor) =>
            OnStartElevatorInnerEvent.Invoke(floor);

        public void OpenElevator(Floor floor) =>
            OnOpenElevatorInnerEvent.Invoke(floor);

        public Floor GetCurrentFloor() =>
            GetCurrentFloorInnerEvent?.Invoke() ?? Floor.Surface;

        public bool IsElevatorMoving() =>
            IsElevatorMovingInnerEvent?.Invoke() ?? false;
    }
}