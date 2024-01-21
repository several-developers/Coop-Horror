using System;
using Cinemachine;
using GameCore.Enums;

namespace GameCore.Gameplay.Entities.MobileHeadquarters
{
    public interface IMobileHeadquartersEntity : IEntity, INetworkObject
    {
        event Action<SceneName> OnLoadLocationEvent;
        event Action OnLocationLeftEvent;
        void ChangePath(CinemachinePath path);
        void ArrivedAtLocation();
        void LeftLocation();
    }
}