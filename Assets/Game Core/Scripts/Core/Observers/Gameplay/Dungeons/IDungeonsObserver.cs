using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Levels.Elevator;

namespace GameCore.Observers.Gameplay.Dungeons
{
    public interface IDungeonsObserver
    {
        event Action<ElevatorBase> OnRegisterElevatorEvent;
        event Action<Floor> OnDungeonGenerationCompletedEvent;
        event Action OnDungeonsGenerationCompletedEvent;
        void RegisterElevator(ElevatorBase elevatorBase);
        void DungeonGenerationCompleted(Floor floor);
        void DungeonsGenerationCompleted();
    }
}