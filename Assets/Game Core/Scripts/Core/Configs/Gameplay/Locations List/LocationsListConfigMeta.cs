using System.Collections.Generic;
using GameCore.Gameplay.Level.Locations;
using GameCore.Infrastructure.Configs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Configs.Gameplay.LocationsList
{
    public class LocationsListConfigMeta : ConfigMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private List<LocationMeta> _availableLocations;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public IEnumerable<LocationMeta> GetAllAvailableLocationsMeta() => _availableLocations;

        public override string GetMetaCategory() =>
            EditorConstants.GameplayConfigsListsCategory;

        public override ConfigScope GetConfigScope() =>
            ConfigScope.GameplayScene;
    }
}