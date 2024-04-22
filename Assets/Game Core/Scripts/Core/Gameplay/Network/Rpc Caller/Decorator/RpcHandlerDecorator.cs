using System;
using GameCore.Enums.Gameplay;
using GameCore.Enums.Global;
using GameCore.Gameplay.Dungeons;

namespace GameCore.Gameplay.Network
{
    public class RpcHandlerDecorator : IRpcHandlerDecorator
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action<int, int> OnCreateItemPreviewInnerEvent = delegate { };
        public event Action<int> OnDestroyItemPreviewInnerEvent = delegate { };
        public event Action<SceneName> OnLoadLocationInnerEvent = delegate { };
        public event Action OnStartLeavingLocationInnerEvent = delegate { };
        public event Action OnLocationLeftInnerEvent = delegate { };
        public event Action<DungeonsSeedData> OnGenerateDungeonsInnerEvent = delegate { };
        public event Action<Floor> OnStartElevatorInnerEvent = delegate { };
        public event Action<Floor> OnOpenElevatorInnerEvent = delegate { };
        public event Action<ulong, bool> OnTogglePlayerInsideMobileHQInnerEvent = delegate { };
        public event Action<Floor, bool> OnTeleportToFireExitInnerEvent = delegate { };

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void CreateItemPreview(int slotIndex, int itemID) =>
            OnCreateItemPreviewInnerEvent.Invoke(slotIndex, itemID);

        public void DestroyItemPreview(int slotIndex) =>
            OnDestroyItemPreviewInnerEvent.Invoke(slotIndex);

        public void LoadLocation(SceneName locationName) =>
            OnLoadLocationInnerEvent.Invoke(locationName);

        public void StartLeavingLocation() =>
            OnStartLeavingLocationInnerEvent.Invoke();

        public void LocationLeft() =>
            OnLocationLeftInnerEvent.Invoke();

        public void GenerateDungeons(DungeonsSeedData data) => 
            OnGenerateDungeonsInnerEvent.Invoke(data);

        public void StartElevator(Floor floor) =>
            OnStartElevatorInnerEvent.Invoke(floor);

        public void OpenElevator(Floor floor) => 
            OnOpenElevatorInnerEvent.Invoke(floor);

        public void TogglePlayerInsideMobileHQ(ulong clientID, bool isInside) =>
            OnTogglePlayerInsideMobileHQInnerEvent.Invoke(clientID, isInside);

        public void TeleportToFireExit(Floor floor, bool isInStairsLocation) =>
            OnTeleportToFireExitInnerEvent.Invoke(floor, isInStairsLocation);
    }
}