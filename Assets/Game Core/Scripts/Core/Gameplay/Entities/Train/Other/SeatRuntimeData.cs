using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Train
{
    public struct SeatRuntimeData : INetworkSerializable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public SeatRuntimeData(int seatIndex)
        {
            _seatIndex = seatIndex;
            _playersOnSeat = 0;
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public int SeatIndex => _seatIndex;
        public bool IsBusy => _playersOnSeat > 0;
        
        // FIELDS: --------------------------------------------------------------------------------
        
        private int _seatIndex;
        private int _playersOnSeat;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _seatIndex);
            serializer.SerializeValue(ref _playersOnSeat);
        }

        public void SetSeatIndex(int seatIndex) =>
            _seatIndex = seatIndex;

        public void IncreasePlayersOnSeat() =>
            _playersOnSeat += 1;

        public void DecreasePlayersOnSeat() =>
            _playersOnSeat = Mathf.Max(a: _playersOnSeat - 1, b: 0);
    }
}