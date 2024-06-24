using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.Entities.MobileHeadquarters
{
    public struct SeatsRuntimeDataContainer : INetworkSerializable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public SeatsRuntimeDataContainer(int seatsAmount)
        {
            _seatsData = new SeatRuntimeData[seatsAmount];

            for (int i = 0; i < seatsAmount; i++)
                _seatsData[i].SetSeatIndex(i);
        }
        
        // PROPERTIES: ----------------------------------------------------------------------------

        // FIELDS: --------------------------------------------------------------------------------

        private SeatRuntimeData[] _seatsData;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter =>
            serializer.SerializeValue(ref _seatsData);

        public void TakeSeat(int seatIndex)
        {
            int iterations = _seatsData.Length;

            for (int i = 0; i < iterations; i++)
            {
                bool isMatches = _seatsData[i].SeatIndex == seatIndex;

                if (!isMatches)
                    continue;
                
                _seatsData[i].IncreasePlayersOnSeat();
                break;
            }
        }

        public void LeftSeat(int seatIndex)
        {
            int iterations = _seatsData.Length;

            for (int i = 0; i < iterations; i++)
            {
                bool isMatches = _seatsData[i].SeatIndex == seatIndex;

                if (!isMatches)
                    continue;
                
                _seatsData[i].DecreasePlayersOnSeat();
                break;
            }
        }
        
        public IEnumerable<SeatRuntimeData> GetAllSeatsData() => _seatsData;

        public int GetRandomFreeSeatIndex()
        {
            int freeSeatsAmount = GetFreeSeatsAmount();
            int totalSeats = _seatsData.Length;
            int seatIndex;

            if (freeSeatsAmount == 0)
            {
                seatIndex = Random.Range(0, totalSeats);
                return seatIndex;
            }

            List<SeatRuntimeData> freeSeats = new();

            foreach (SeatRuntimeData seatRuntimeData in _seatsData)
            {
                if (seatRuntimeData.IsBusy)
                    continue;

                freeSeats.Add(seatRuntimeData);
            }

            int randomIndex = Random.Range(0, freeSeats.Count);
            seatIndex = freeSeats[randomIndex].SeatIndex;
            return seatIndex;
        }
        
        public int GetFreeSeatsAmount()
        {
            int freeSeatsAmount = 0;

            foreach (SeatRuntimeData seatRuntimeData in _seatsData)
            {
                if (seatRuntimeData.IsBusy)
                    continue;

                freeSeatsAmount += 1;
            }

            return freeSeatsAmount;
        }
    }
}