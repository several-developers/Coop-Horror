using System;
using GameCore.Gameplay.Dungeons;

namespace GameCore.Observers.Gameplay.Rpc
{
    public interface IRpcObserver
    {
        event Action<DungeonsSeedData> OnGenerateDungeonsEvent;
        void GenerateDungeons(DungeonsSeedData data);
    }
}