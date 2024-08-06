using System.Collections.Generic;
using Cinemachine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Level.Locations
{
    public class MetroLocationManager : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private bool _drawDebug = true;
        
        [Title(Constants.References)]
        [SerializeField, Required]
        private CinemachinePath _mainPath;

        [SerializeField, Required, Space(height: 5)]
        private List<RoadPathReference> _enterPathsReferences;

        // PROPERTIES: ----------------------------------------------------------------------------

        public bool DrawDebug => _drawDebug;
        
        // FIELDS: --------------------------------------------------------------------------------

        private static MetroLocationManager _instance;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _instance = this;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public IReadOnlyList<RoadPathReference> GetAllEnterPathsReferences() => _enterPathsReferences;

        public CinemachinePath GetMainPath() => _mainPath;

        public bool TryGetEnterPathByID(int pathID, out CinemachinePath result)
        {
            foreach (RoadPathReference pathReference in _enterPathsReferences)
            {
                bool isMatches = pathReference.PathID == pathID;

                if (!isMatches)
                    continue;

                result = pathReference.Path;
                bool isPathValid = result != null;
                return isPathValid;
            }

            result = null;
            return false;
        }

        public static MetroLocationManager Get() => _instance;
    }
}