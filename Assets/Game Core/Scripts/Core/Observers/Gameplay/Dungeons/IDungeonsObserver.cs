using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Dungeons;

namespace GameCore.Observers.Gameplay.Dungeons
{
    public interface IDungeonsObserver
    {
        event Action<DungeonIndex, DungeonReferences> OnDungeonGenerationCompletedEvent;
        event Action OnDungeonsGenerationCompletedEvent;
        void SendDungeonGenerationCompleted(DungeonIndex dungeonIndex, DungeonReferences dungeonReferences);
        void SendDungeonsGenerationCompleted();
    }
}