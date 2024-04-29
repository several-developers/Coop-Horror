using System;

namespace GameCore.Gameplay.Entities.MobileHeadquarters
{
    public interface IMobileHeadquartersEntity : IEntity, INetworkObject
    {
        event Action OnOpenQuestsSelectionMenuEvent;
        event Action OnOpenLocationsSelectionMenuEvent;
        event Action OnOpenGameOverWarningMenuEvent;
        event Action OnCallDeliveryDroneEvent;
        void OpenDoor();
        void EnableMainLever();
    }
}