using System;
using Cinemachine;
using UnityEngine;

namespace GameCore.Gameplay.Entities.MobileHeadquarters
{
    public interface IMobileHeadquartersEntity : IEntity, INetworkObject
    {
        event Action OnOpenQuestsSelectionMenuEvent;
        event Action OnOpenLocationsSelectionMenuEvent;
        event Action OnOpenGameOverWarningMenuEvent;
        void OpenDoor();
        void EnableMainLever();
        void ChangePath(CinemachinePath path, float startDistancePercent = 0f, bool stayAtSamePosition = false);
        void TeleportToTheRoad();
        Camera GetOutsideCamera();
    }
}