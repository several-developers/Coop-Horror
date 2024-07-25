using System;
using GameCore.Enums.Gameplay;
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
        private LocationName _locationName;

        [SerializeField, Required]
        private LocationItemsSpawnConfigMeta _config;

        // PROPERTIES: ----------------------------------------------------------------------------

        public LocationName LocationName => _locationName;
        public LocationItemsSpawnConfigMeta Config => _config;
    }
}