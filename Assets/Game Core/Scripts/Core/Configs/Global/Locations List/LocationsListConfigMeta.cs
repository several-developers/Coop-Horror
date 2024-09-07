using System;
using System.Collections.Generic;
using GameCore.Gameplay.Level.Locations;
using GameCore.Infrastructure.Configs;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameCore.Configs.Global.LocationsList
{
    public class LocationsListConfigMeta : ConfigMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField]
        [ListDrawerSettings(ListElementLabelName = "Label")]
        private List<LocationReference> _locationsReferences;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public IEnumerable<LocationReference> GetAllLocationsReferences() => _locationsReferences;

        public override string GetMetaCategory() =>
            EditorConstants.GlobalConfigsListsCategory;

        public override ConfigScope GetConfigScope() =>
            ConfigScope.Global;

        // INNER CLASSES: -------------------------------------------------------------------------

        [Serializable]
        public class LocationReference
        {
            // MEMBERS: -------------------------------------------------------------------------------

            [SerializeField]
            private LocationMeta _locationMeta;

            [SerializeField]
            private AssetReferenceGameObject _locationPrefabAsset;

            // PROPERTIES: ----------------------------------------------------------------------------

            public LocationMeta LocationMeta => _locationMeta;
            public AssetReferenceGameObject LocationPrefabAsset => _locationPrefabAsset;

            private string Label => $"'Location: {(_locationMeta == null ? "none" : _locationMeta.LocationNameText)}'";
        }
    }
}