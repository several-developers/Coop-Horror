using Cinemachine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Levels.Locations
{
    public class LocationManager : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private CinemachinePath _enterPath;
        
        [SerializeField, Required]
        private CinemachinePath _exitPath;
        
        // FIELDS: --------------------------------------------------------------------------------

        private static LocationManager _instance;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _instance = this;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public CinemachinePath GetEnterPath() => _enterPath;
        
        public CinemachinePath GetExitPath() => _exitPath;
        
        public static LocationManager Get() => _instance;
    }
}