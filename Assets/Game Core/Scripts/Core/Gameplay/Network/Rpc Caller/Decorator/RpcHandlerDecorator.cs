using System;
using GameCore.Enums.Gameplay;
using GameCore.Enums.Global;
using GameCore.Gameplay.Dungeons;

namespace GameCore.Gameplay.Network
{
    public class RpcHandlerDecorator : IRpcHandlerDecorator
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action<int, int> OnCreateItemPreviewEvent = delegate { };
        public event Action<int> OnDestroyItemPreviewEvent = delegate { };
        public event Action<SceneName> OnLoadLocationEvent = delegate { };
        public event Action OnStartLeavingLocationEvent = delegate { };
        public event Action OnLocationLeftEvent = delegate { };
        public event Action<DungeonsSeedData> OnGenerateDungeonsEvent = delegate { };
        public event Action<Floor> OnStartElevatorEvent = delegate { };
        public event Action<Floor> OnOpenElevatorEvent = delegate { };
        public event Action<ulong, bool> OnTogglePlayerInsideMobileHQEvent = delegate { };
        public event Action<Floor, bool> OnTeleportToFireExitEvent = delegate { };

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void CreateItemPreview(int slotIndex, int itemID) =>
            OnCreateItemPreviewEvent.Invoke(slotIndex, itemID);

        public void DestroyItemPreview(int slotIndex) =>
            OnDestroyItemPreviewEvent.Invoke(slotIndex);

        public void LoadLocation(SceneName locationName) =>
            OnLoadLocationEvent.Invoke(locationName);

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

        public void TeleportToFireExit(Floor floor, bool isInStairsLocation) =>
            OnTeleportToFireExitEvent.Invoke(floor, isInStairsLocation);
    }
}