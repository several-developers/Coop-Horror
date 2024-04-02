using System;
using GameCore.Enums.Gameplay;

namespace GameCore.Gameplay.Levels.Elevator
{
    public class ElevatorsManagerDecorator : IElevatorsManagerDecorator
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action<ElevatorStaticData> OnElevatorStartedEvent = delegate {  };
        public event Action<ElevatorStaticData> OnFloorChangedEvent = delegate {  };
        public event Action<Floor> OnElevatorStoppedEvent = delegate {  };
        
        public event Func<Floor> OnGetCurrentFloorEvent; // Проверить как выше
        public event Func<bool> OnIsElevatorMovingEvent;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void ElevatorStarted(ElevatorStaticData data) =>
            OnElevatorStartedEvent.Invoke(data);

        public void FloorChanged(ElevatorStaticData data) =>
            OnFloorChangedEvent.Invoke(data);

        public void ElevatorStopped(Floor floor) =>
            OnElevatorStoppedEvent.Invoke(floor);

        public Floor GetCurrentFloor()
        {
            if (OnGetCurrentFloorEvent == null)
                return Floor.Surface;
            
            return OnGetCurrentFloorEvent();
        }

        public bool IsElevatorMoving()
        {
            if (OnIsElevatorMovingEvent == null)
                return false;
            
            return OnIsElevatorMovingEvent();
        }
    }
}