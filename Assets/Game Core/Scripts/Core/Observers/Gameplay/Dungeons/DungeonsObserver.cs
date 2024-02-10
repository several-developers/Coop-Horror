using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Dungeons;

namespace GameCore.Observers.Gameplay.Dungeons
{
    public class DungeonsObserver : IDungeonsObserver
    {
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<DungeonIndex, DungeonReferences> OnDungeonGenerationCompletedEvent;
        public event Action OnDungeonsGenerationCompletedEvent;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void SendDungeonGenerationCompleted(DungeonIndex dungeonIndex, DungeonReferences dungeonReferences) =>
            OnDungeonGenerationCompletedEvent?.Invoke(dungeonIndex, dungeonReferences);

        public void SendDungeonsGenerationCompleted() =>
            OnDungeonsGenerationCompletedEvent?.Invoke();
    }
}