using System;
using Cinemachine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Levels.Locations
{
    [Serializable]
    public class RoadPathReference
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField, Min(0)]
        private int _pathID;

        [SerializeField, Required]
        private CinemachinePath _path;

        // PROPERTIES: ----------------------------------------------------------------------------

        public int PathID => _pathID;
        public CinemachinePath Path => _path;
    }
}