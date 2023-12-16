using System;
using UnityEngine;

namespace GameCore.Infrastructure.Data
{
    [Serializable]
    public abstract class DataBase
    {
        // PROPERTIES: ----------------------------------------------------------------------------
        
        public abstract string DataKey { get; }
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public string GetDataPath() =>
            $"{Application.persistentDataPath}/{DataKey}.json";
    }
}