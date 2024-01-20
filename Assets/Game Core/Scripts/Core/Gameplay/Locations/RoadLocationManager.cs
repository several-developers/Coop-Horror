using Cinemachine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Locations
{
    public class RoadLocationManager : MonoBehaviour, IRoadLocationManager
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private CinemachinePath _path;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public CinemachinePath GetPath() => _path;
    }
}