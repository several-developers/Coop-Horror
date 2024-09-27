using System.Collections.Generic;
using GameCore.Gameplay.Level.Locations;
using GameCore.InfrastructureTools.Configs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Infrastructure.Configs.Global.LocationsDatabase
{
    public class LocationsDatabaseConfigMeta : ConfigMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private List<LocationMeta> _availableLocations;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public IEnumerable<LocationMeta> GetAllAvailableLocationsMeta() => _availableLocations;

        public override string GetMetaCategory() =>
            EditorConstants.GlobalConfigsListsCategory;

        public override ConfigScope GetConfigScope() =>
            ConfigScope.Global;
    }
}