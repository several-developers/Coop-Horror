using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Dungeons;

namespace GameCore.Observers.Gameplay.Dungeons
{
    public class DungeonsObserver : IDungeonsObserver
    {
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<ElevatorFloor, DungeonReferences> OnDungeonGenerationCompletedEvent;
        public event Action OnDungeonsGenerationCompletedEvent;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void SendDungeonGenerationCompleted(ElevatorFloor elevatorFloor, DungeonReferences dungeonReferences) =>
            OnDungeonGenerationCompletedEvent?.Invoke(elevatorFloor, dungeonReferences);

        public void SendDungeonsGenerationCompleted() =>
            OnDungeonsGenerationCompletedEvent?.Invoke();
    }
}