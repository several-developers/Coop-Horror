using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Network;
using Unity.Netcode;

namespace GameCore.Observers.Gameplay.Game
{
    public class GameObserverNetwork : NetcodeBehaviour, IGameObserver
    {
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<LocationName> OnTrainArrivedAtBaseEvent = delegate { };
        public event Action OnTrainLeavingBaseEvent = delegate { };
        public event Action OnTrainLeftBaseEvent = delegate { };
        public event Action OnTrainArrivedAtSectorEvent = delegate { };
        public event Action OnTrainStoppedAtSectorEvent = delegate { };
        public event Action OnTrainLeavingSectorEvent = delegate { };
        public event Action OnTrainLeftSectorEvent = delegate { };

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void TrainArrivedAtBase(LocationName previousLocationName) =>
            TrainArrivedAtBaseServerRpc(previousLocationName);

        public void TrainLeavingBase() => TrainLeavingBaseServerRpc();

        public void TrainLeftBase() => TrainLeftBaseServerRpc();

        public void TrainArrivedAtSector() => TrainArrivedAtSectorServerRpc();

        public void TrainStoppedAtSector() => TrainStoppedAtSectorServerRpc();

        public void TrainLeavingSector() => TrainLeavingSectorServerRpc();

        public void TrainLeftSector() => TrainLeftSectorServerRpc();

        // RPC: -----------------------------------------------------------------------------------

        [ServerRpc(RequireOwnership = false)]
        private void TrainArrivedAtBaseServerRpc(LocationName previousLocationName) =>
            TrainArrivedAtBaseClientRpc(previousLocationName);
        
        [ServerRpc(RequireOwnership = false)]
        private void TrainLeavingBaseServerRpc() => TrainLeavingBaseClientRpc();
        
        [ServerRpc(RequireOwnership = false)]
        private void TrainLeftBaseServerRpc() => TrainLeftBaseClientRpc();
        
        [ServerRpc(RequireOwnership = false)]
        private void TrainArrivedAtSectorServerRpc() => TrainArrivedAtSectorClientRpc();
        
        [ServerRpc(RequireOwnership = false)]
        private void TrainStoppedAtSectorServerRpc() => TrainStoppedAtSectorClientRpc();
        
        [ServerRpc(RequireOwnership = false)]
        private void TrainLeavingSectorServerRpc() => TrainLeavingSectorClientRpc();
        
        [ServerRpc(RequireOwnership = false)]
        private void TrainLeftSectorServerRpc() => TrainLeftSectorClientRpc();

        [ClientRpc]
        private void TrainArrivedAtBaseClientRpc(LocationName previousLocationName) =>
            OnTrainArrivedAtBaseEvent.Invoke(previousLocationName);

        [ClientRpc]
        private void TrainLeavingBaseClientRpc() =>
            OnTrainLeavingBaseEvent.Invoke();

        [ClientRpc]
        private void TrainLeftBaseClientRpc() =>
            OnTrainLeftBaseEvent.Invoke();

        [ClientRpc]
        private void TrainArrivedAtSectorClientRpc() =>
            OnTrainArrivedAtSectorEvent.Invoke();

        [ClientRpc]
        private void TrainStoppedAtSectorClientRpc() =>
            OnTrainStoppedAtSectorEvent.Invoke();

        [ClientRpc]
        private void TrainLeavingSectorClientRpc() =>
            OnTrainLeavingSectorEvent.Invoke();

        [ClientRpc]
        private void TrainLeftSectorClientRpc() =>
            OnTrainLeftSectorEvent.Invoke();
    }
}