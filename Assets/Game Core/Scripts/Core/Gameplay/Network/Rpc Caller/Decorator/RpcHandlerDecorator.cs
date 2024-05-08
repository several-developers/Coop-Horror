using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Dungeons;

namespace GameCore.Gameplay.Network
{
    public class RpcHandlerDecorator : IRpcHandlerDecorator
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action<DungeonsSeedData> OnGenerateDungeonsInnerEvent = delegate { };
        public event Action<Floor> OnStartElevatorInnerEvent = delegate { };
        public event Action<Floor> OnOpenElevatorInnerEvent = delegate { };
        public event Action<Floor, bool> OnTeleportToFireExitInnerEvent = delegate { };

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void GenerateDungeons(DungeonsSeedData data) => 
            OnGenerateDungeonsInnerEvent.Invoke(data);

        public void StartElevator(Floor floor) =>
            OnStartElevatorInnerEvent.Invoke(floor);

        public void OpenElevator(Floor floor) => 
            OnOpenElevatorInnerEvent.Invoke(floor);

        public void TeleportToFireExit(Floor floor, bool isInStairsLocation) =>
            OnTeleportToFireExitInnerEvent.Invoke(floor, isInStairsLocation);
    }
}