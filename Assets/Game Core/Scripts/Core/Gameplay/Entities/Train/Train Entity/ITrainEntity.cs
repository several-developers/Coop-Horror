using System;
using Cinemachine;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Train
{
    public interface ITrainEntity : IEntity, INetworkObject
    {
        event Action OnOpenQuestsSelectionMenuEvent;
        event Action OnOpenGameOverWarningMenuEvent;
        void OpenDoor();
        void EnableMainLever();
        void ChangePath(CinemachinePath path, float startDistancePercent = 0f, bool stayAtSamePosition = false);
        void TeleportToTheRoad();
        Camera GetOutsideCamera();
    }
}