using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Dungeons;
using GameCore.Gameplay.Network;

namespace GameCore.Observers.Gameplay.Rpc
{
    public interface IRpcObserver
    {
        event Action<CreateItemPreviewStaticData> OnCreateItemPreviewEvent;
        event Action<int> OnDestroyItemPreviewEvent;
        event Action OnLocationLeftEvent;
        event Action<DungeonsSeedData> OnGenerateDungeonsEvent;
        event Action<Floor> OnStartElevatorEvent;
        event Action<Floor> OnOpenElevatorEvent;
        event Action<ulong, bool> OnTogglePlayerInsideMobileHQEvent;
        event Action<ulong, Floor, bool> OnTeleportToFireExitEvent;
        
        void CreateItemPreview(CreateItemPreviewStaticData data);
        void DestroyItemPreview(int slotIndex);
        void LocationLeft();
        void GenerateDungeons(DungeonsSeedData data);
        void StartElevator(Floor floor);
        void OpenElevator(Floor floor);
        void TogglePlayerInsideMobileHQ(ulong clientID, bool isInside);
        void TeleportToFireExit(ulong clientID, Floor floor, bool isInStairsLocation);
    }
}