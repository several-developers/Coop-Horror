using System;
using GameCore.Enums.Gameplay;

namespace GameCore.Observers.Gameplay.Dungeons
{
    public class DungeonsObserver : IDungeonsObserver
    {
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<Floor> OnDungeonGenerationCompletedEvent;
        public event Action OnDungeonsGenerationCompletedEvent;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void DungeonGenerationCompleted(Floor floor) =>
            OnDungeonGenerationCompletedEvent?.Invoke(floor);

        public void DungeonsGenerationCompleted() =>
            OnDungeonsGenerationCompletedEvent?.Invoke();
    }
}