﻿using GameCore.Enums;
using CustomEditors;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Locations
{
    public class LocationMeta : EditorMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private string _locationName = "location_name";

        [SerializeField]
        private SceneName _sceneName;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override string GetMetaCategory() =>
            EditorConstants.LocationsCategory;
    }
}