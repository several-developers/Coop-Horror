using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Dungeons;

namespace GameCore.Observers.Gameplay.Dungeons
{
    public interface IDungeonsObserver
    {
        event Action<ElevatorFloor, DungeonReferences> OnDungeonGenerationCompletedEvent;
        event Action OnDungeonsGenerationCompletedEvent;
        void SendDungeonGenerationCompleted(ElevatorFloor elevatorFloor, DungeonReferences dungeonReferences);
        void SendDungeonsGenerationCompleted();
    }
}