using Cinemachine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Levels.Locations
{
    public class RoadLocationManager : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private CinemachinePath _path;

        // FIELDS: --------------------------------------------------------------------------------

        private static RoadLocationManager _instance;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _instance = this;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public CinemachinePath GetPath() => _path;

        public static RoadLocationManager Get() => _instance;
    }
}