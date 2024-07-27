﻿using System;
using GameCore.Enums.Gameplay;

namespace GameCore.Observers.Gameplay.Game
{
    public class GameObserver : IGameObserver
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action<LocationName> OnTrainArrivedAtBaseEvent = delegate { };
        public event Action OnTrainLeavingBaseEvent = delegate { };
        public event Action OnTrainArrivedAtSectorEvent = delegate { };
        public event Action OnTrainStoppedAtSectorEvent = delegate { };
        public event Action OnTrainLeavingSectorEvent = delegate { };

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void TrainArrivedAtBase(LocationName previousLocationName) =>
            OnTrainArrivedAtBaseEvent.Invoke(previousLocationName);

        public void TrainLeavingBase() =>
            OnTrainLeavingBaseEvent.Invoke();

        public void TrainArrivedAtSector() =>
            OnTrainArrivedAtSectorEvent.Invoke();

        public void TrainStoppedAtSector() =>
            OnTrainStoppedAtSectorEvent.Invoke();

        public void TrainLeavingSector() =>
            OnTrainLeavingSectorEvent.Invoke();
    }
}