using System;
using GameCore.Enums.Gameplay;

namespace GameCore.Observers.Gameplay.Game
{
    public class GameObserver : IGameObserver
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action<LocationName> OnTrainArrivedAtBaseEvent = delegate { };
        public event Action OnTrainLeavingBaseEvent = delegate { };
        public event Action OnTrainArrivedAtLocationEvent = delegate { };
        public event Action OnTrainStoppedAtLocationEvent = delegate { };
        public event Action OnTrainLeavingLocationEvent = delegate { };

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void TrainArrivedAtBase(LocationName previousLocationName) =>
            OnTrainArrivedAtBaseEvent.Invoke(previousLocationName);

        public void TrainLeavingBase() =>
            OnTrainLeavingBaseEvent.Invoke();

        public void TrainArrivedAtLocation() =>
            OnTrainArrivedAtLocationEvent.Invoke();

        public void TrainStoppedAtLocation() =>
            OnTrainStoppedAtLocationEvent.Invoke();

        public void TrainLeavingLocation() =>
            OnTrainLeavingLocationEvent.Invoke();
    }
}