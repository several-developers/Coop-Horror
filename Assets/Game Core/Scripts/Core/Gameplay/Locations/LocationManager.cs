using Cinemachine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Locations
{
    public class LocationManager : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private CinemachinePath _path;
        
        // FIELDS: --------------------------------------------------------------------------------

        private static LocationManager _instance;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _instance = this;

        private void OnDestroy() =>
            _instance = null;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public CinemachinePath GetPath() => _path;
        
        public static LocationManager Get() => _instance;
    }
}