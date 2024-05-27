using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Dungeons;

namespace GameCore.Observers.Gameplay.Rpc
{
    public class RpcObserver : IRpcObserver
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action<DungeonsSeedData> OnGenerateDungeonsEvent = delegate { };
        public event Action<Floor> OnStartElevatorEvent = delegate { };
        public event Action<Floor> OnOpenElevatorEvent = delegate { };

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void GenerateDungeons(DungeonsSeedData data) =>
            OnGenerateDungeonsEvent.Invoke(data);

        public void StartElevator(Floor floor) =>
            OnStartElevatorEvent.Invoke(floor);

        public void OpenElevator(Floor floor) =>
            OnOpenElevatorEvent.Invoke(floor);
    }
}