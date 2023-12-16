using System;
using UnityEngine;

namespace GameCore.Infrastructure.Data
{
    [Serializable]
    public class GameData : DataBase
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField, Min(1)]
        private int _currentLevel = 1;
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public override string DataKey => Constants.GameDataKey;
        public int CurrentLevel => _currentLevel;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetCurrentLevel(int level) =>
            _currentLevel = Mathf.Max(level, 1);
    }
}