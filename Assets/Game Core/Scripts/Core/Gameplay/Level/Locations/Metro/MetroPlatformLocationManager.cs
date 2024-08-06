using Cinemachine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Level.Locations
{
    public class MetroPlatformLocationManager : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private CinemachinePath _enterPath;
        
        [SerializeField, Required]
        private CinemachinePath _exitPath;
        
        // FIELDS: --------------------------------------------------------------------------------

        private static MetroPlatformLocationManager _instance;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _instance = this;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public static MetroPlatformLocationManager Get() => _instance;

        public CinemachinePath GetEnterPath() => _enterPath;

        public CinemachinePath GetExitPath() => _exitPath;
    }
}