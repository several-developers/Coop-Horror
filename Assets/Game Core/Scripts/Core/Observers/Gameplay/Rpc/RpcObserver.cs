using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Dungeons;
using GameCore.Gameplay.Network;

namespace GameCore.Observers.Gameplay.Rpc
{
    public class RpcObserver : IRpcObserver
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action<CreateItemPreviewStaticData> OnCreateItemPreviewEvent = delegate { };
        public event Action<int> OnDestroyItemPreviewEvent = delegate { };
        public event Action OnStartLeavingLocationEvent = delegate { };
        public event Action OnLocationLeftEvent = delegate { };
        public event Action<DungeonsSeedData> OnGenerateDungeonsEvent = delegate { };
        public event Action<Floor> OnStartElevatorEvent = delegate { };
        public event Action<Floor> OnOpenElevatorEvent = delegate { };
        public event Action<ulong, bool> OnTogglePlayerInsideMobileHQEvent = delegate { };
        public event Action<ulong, Floor, bool> OnTeleportToFireExitEvent = delegate { };

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void CreateItemPreview(CreateItemPreviewStaticData data) =>
            OnCreateItemPreviewEvent.Invoke(data);

        public void DestroyItemPreview(int slotIndex) =>
            OnDestroyItemPreviewEvent.Invoke(slotIndex);

        public void StartLeavingLocation() =>
            OnStartLeavingLocationEvent.Invoke();

        public void LocationLeft() =>
            OnLocationLeftEvent.Invoke();

        public void GenerateDungeons(DungeonsSeedData data) =>
            OnGenerateDungeonsEvent.Invoke(data);

        public void StartElevator(Floor floor) =>
            OnStartElevatorEvent.Invoke(floor);

        public void OpenElevator(Floor floor) =>
            OnOpenElevatorEvent.Invoke(floor);

        public void TogglePlayerInsideMobileHQ(ulong clientID, bool isInside) =>
            OnTogglePlayerInsideMobileHQEvent.Invoke(clientID, isInside);

        public void TeleportToFireExit(ulong clientID, Floor floor, bool isInStairsLocation) =>
            OnTeleportToFireExitEvent.Invoke(clientID, floor, isInStairsLocation);
    }
}