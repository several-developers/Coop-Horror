using System;
using GameCore.Enums.Global;
using GameCore.Gameplay.Items.SpawnSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Configs.Gameplay.ItemsSpawn
{
    [Serializable]
    public class LocationItemsSpawnConfigReference
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField]
        private SceneName _locationName = SceneName.Forest;

        [SerializeField, Required]
        private LocationItemsSpawnConfigMeta _config;

        // PROPERTIES: ----------------------------------------------------------------------------

        public SceneName LocationName => _locationName;
        public LocationItemsSpawnConfigMeta Config => _config;
    }
}