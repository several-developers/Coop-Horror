using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Dungeons;

namespace GameCore.Observers.Gameplay.Rpc
{
    public interface IRpcObserver
    {
        event Action<DungeonsSeedData> OnGenerateDungeonsEvent;
        event Action<Floor> OnStartElevatorEvent;
        event Action<Floor> OnOpenElevatorEvent;
        
        void GenerateDungeons(DungeonsSeedData data);
        void StartElevator(Floor floor);
        void OpenElevator(Floor floor);
    }
}