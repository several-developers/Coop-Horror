using System;
using GameCore.Enums.Gameplay;

namespace GameCore.Observers.Gameplay.Game
{
    public interface IGameObserver
    {
        event Action<LocationName> OnTrainArrivedAtBaseEvent;
        event Action OnTrainLeavingBaseEvent;
        event Action OnTrainArrivedAtLocationEvent;
        event Action OnTrainStoppedAtLocationEvent;
        event Action OnTrainLeavingLocationEvent;

        void TrainArrivedAtBase(LocationName previousLocationName);
        void TrainLeavingBase();
        void TrainArrivedAtLocation();
        void TrainStoppedAtLocation();
        void TrainLeavingLocation();
    }
}