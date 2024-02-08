using System;

namespace GameCore.Observers.Gameplay.Dungeons
{
    public interface IDungeonsObserver
    {
        event Action OnDungeonsGenerationCompletedEvent;
        void SendDungeonsGenerationCompleted();
    }
}