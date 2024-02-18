using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Dungeons;

namespace GameCore.Observers.Gameplay.Dungeons
{
    public interface IDungeonsObserver
    {
        event Action<Floor, DungeonReferences> OnDungeonGenerationCompletedEvent;
        event Action OnDungeonsGenerationCompletedEvent;
        void SendDungeonGenerationCompleted(Floor floor, DungeonReferences dungeonReferences);
        void SendDungeonsGenerationCompleted();
    }
}