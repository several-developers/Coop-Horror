using CustomEditors;
using GameCore.Enums.Gameplay;
using GameCore.Enums.Global;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Level.Locations
{
    public class LocationMeta : EditorMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private string _locationNameText = "location_name";

        [SerializeField]
        private LocationName _locationName;
        
        [SerializeField]
        private SceneName _sceneName;

        // PROPERTIES: ----------------------------------------------------------------------------

        public string LocationNameText => _locationNameText;
        public LocationName LocationName => _locationName;
        public SceneName SceneName => _sceneName;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override string GetMetaCategory() =>
            EditorConstants.LocationsCategory;
    }
}