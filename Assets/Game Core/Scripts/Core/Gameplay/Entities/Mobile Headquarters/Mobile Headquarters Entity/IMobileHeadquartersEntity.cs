using System;

namespace GameCore.Gameplay.Entities.MobileHeadquarters
{
    public interface IMobileHeadquartersEntity : IEntity, INetworkObject
    {
        event Action OnLocationLeftEvent;
        event Action OnOpenQuestsSelectionMenuEvent;
    }
}