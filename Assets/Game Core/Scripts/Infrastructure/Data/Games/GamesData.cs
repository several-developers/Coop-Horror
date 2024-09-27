using System;
using GameCore.InfrastructureTools.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Infrastructure.Data
{
    [Serializable]
    public class GamesData : DataBase
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public GamesData() =>
            _gamesData = new GameData[Constants.SaveCellsAmount];

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(EditorConstants.DataTitle)]
        [SerializeField]
        [ListDrawerSettings(ListElementLabelName = "Label")]
        private GameData[] _gamesData;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public bool TryGetGameData(int saveCellIndex, out GameData gameData)
        {
            bool isDataExists = saveCellIndex < _gamesData.Length;
            gameData = isDataExists ? _gamesData[saveCellIndex] : null;
            
            return isDataExists;
        }
    }
}