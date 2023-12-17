using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Infrastructure.Data
{
    [Serializable]
    public class GameData
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public GameData()
        {
            _playersData = new List<PlayerData>();
        }
        
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField, Min(0)]
        private int _saveCellIndex;

        [SerializeField, Space(height: 5)]
        private List<PlayerData> _playersData;

        // PROPERTIES: ----------------------------------------------------------------------------

        public int SaveCellIndex => _saveCellIndex;
        
        private string Label => $"Game Data #{_saveCellIndex}";

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetSaveCellIndex(int index) =>
            _saveCellIndex = index;
    }
}