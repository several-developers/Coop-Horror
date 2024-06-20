using System;
using GameCore.Gameplay.Dungeons;

namespace GameCore.Gameplay.Network
{
    public interface IRpcHandlerDecorator
    {
        event Action<DungeonsSeedData> OnGenerateDungeonsInnerEvent;
        void GenerateDungeons(DungeonsSeedData data);
    }
}