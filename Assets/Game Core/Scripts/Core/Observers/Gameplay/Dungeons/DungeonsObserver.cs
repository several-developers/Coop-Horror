using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Levels.Elevator;

namespace GameCore.Observers.Gameplay.Dungeons
{
    public class DungeonsObserver : IDungeonsObserver
    {
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<ElevatorBase> OnRegisterElevatorEvent;
        public event Action<Floor> OnDungeonGenerationCompletedEvent;
        public event Action OnDungeonsGenerationCompletedEvent;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void RegisterElevator(ElevatorBase elevatorBase) =>
            OnRegisterElevatorEvent?.Invoke(elevatorBase);

        public void DungeonGenerationCompleted(Floor floor) =>
            OnDungeonGenerationCompletedEvent?.Invoke(floor);

        public void DungeonsGenerationCompleted() =>
            OnDungeonsGenerationCompletedEvent?.Invoke();
    }
}