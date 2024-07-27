using System;
using GameCore.Enums.Gameplay;

namespace GameCore.Observers.Gameplay.Game
{
    public interface IGameObserver
    {
        event Action<LocationName> OnTrainArrivedAtBaseEvent;
        event Action OnTrainLeavingBaseEvent;
        event Action OnTrainArrivedAtSectorEvent;
        event Action OnTrainStoppedAtSectorEvent;
        event Action OnTrainLeavingSectorEvent;
        
        void TrainArrivedAtBase(LocationName previousLocationName);
        void TrainLeavingBase();
        void TrainArrivedAtSector();
        void TrainStoppedAtSector();
        void TrainLeavingSector();
    }
}