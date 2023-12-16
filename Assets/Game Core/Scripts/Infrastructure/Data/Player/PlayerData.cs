using System;
using UnityEngine;

namespace GameCore.Infrastructure.Data
{
    [Serializable]
    public class PlayerData : DataBase
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField, Min(0)]
        private long _gold;

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string DataKey => Constants.PlayerDataKey;
        public long Gold => _gold;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void AddGold(long amount) =>
            _gold += amount;

        public void SetGold(long amount) =>
            _gold = Math.Max(amount, 0);

        public void RemoveGold(long amount) =>
            _gold = Math.Max(_gold - amount, 0);
    }
}