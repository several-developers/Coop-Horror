using System;
using GameCore.Gameplay.Dungeons;

namespace GameCore.Observers.Gameplay.Rpc
{
    public class RpcObserver : IRpcObserver
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action<DungeonsSeedData> OnGenerateDungeonsEvent = delegate { };

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void GenerateDungeons(DungeonsSeedData data) =>
            OnGenerateDungeonsEvent.Invoke(data);
    }
}