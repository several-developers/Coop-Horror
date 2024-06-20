using System;
using GameCore.Gameplay.Dungeons;

namespace GameCore.Gameplay.Network
{
    public class RpcHandlerDecorator : IRpcHandlerDecorator
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action<DungeonsSeedData> OnGenerateDungeonsInnerEvent = delegate { };

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void GenerateDungeons(DungeonsSeedData data) => 
            OnGenerateDungeonsInnerEvent.Invoke(data);
    }
}