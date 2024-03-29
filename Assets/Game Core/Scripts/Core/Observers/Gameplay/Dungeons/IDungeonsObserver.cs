using System;
using GameCore.Enums.Gameplay;

namespace GameCore.Observers.Gameplay.Dungeons
{
    public interface IDungeonsObserver
    {
        event Action<Floor> OnDungeonGenerationCompletedEvent;
        event Action OnDungeonsGenerationCompletedEvent;
        void DungeonGenerationCompleted(Floor floor);
        void DungeonsGenerationCompleted();
    }
}