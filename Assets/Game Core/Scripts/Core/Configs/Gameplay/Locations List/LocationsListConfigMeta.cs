using System.Collections.Generic;
using GameCore.Gameplay.Level.Locations;
using CustomEditors;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Configs.Gameplay.LocationsList
{
    public class LocationsListConfigMeta : EditorMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private LocationMeta[] _locationsMeta;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public IEnumerable<LocationMeta> GetAllLocationsMeta() => _locationsMeta;

        public override string GetMetaCategory() =>
            EditorConstants.GameplayConfigsListsCategory;
    }
}