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
        event Action<ulong, bool> OnTogglePlayerInsideMobileHQEvent;
        event Action<ulong, Floor, bool> OnTeleportToFireExitEvent;
        
        void GenerateDungeons(DungeonsSeedData data);
        void StartElevator(Floor floor);
        void OpenElevator(Floor floor);
        void TogglePlayerInsideMobileHQ(ulong clientID, bool isInside);
        void TeleportToFireExit(ulong clientID, Floor floor, bool isInStairsLocation);
    }
}