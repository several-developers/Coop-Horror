using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Dungeons;

namespace GameCore.Observers.Gameplay.Dungeons
{
    public class DungeonsObserver : IDungeonsObserver
    {
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<Floor, DungeonReferences> OnDungeonGenerationCompletedEvent;
        public event Action OnDungeonsGenerationCompletedEvent;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void SendDungeonGenerationCompleted(Floor floor, DungeonReferences dungeonReferences) =>
            OnDungeonGenerationCompletedEvent?.Invoke(floor, dungeonReferences);

        public void SendDungeonsGenerationCompleted() =>
            OnDungeonsGenerationCompletedEvent?.Invoke();
    }
}