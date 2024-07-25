using System;

namespace GameCore.Observers.Gameplay.Game
{
    public interface IGameObserver
    {
        event Action OnTrainArrivedAtBaseEvent;
        event Action OnTrainLeavingBaseEvent;
        event Action OnTrainArrivedAtSectorEvent;
        event Action OnTrainStoppedAtSectorEvent;
        event Action OnTrainLeavingSectorEvent;
        
        void TrainArrivedAtBase();
        void TrainLeavingBase();
        void TrainArrivedAtSector();
        void TrainStoppedAtSector();
        void TrainLeavingSector();
    }
}