using CustomEditors;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Configs.Gameplay.Delivery
{
    public class DeliveryConfigMeta : EditorMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [TitleGroup(Constants.Settings)]
        [BoxGroup(DroneSettingsTitle)]
        [SerializeField, Min(0)]
        private float _droneCartFlySpeed = 3f;
        
        [BoxGroup(DroneSettingsTitle)]
        [SerializeField, Min(0)]
        private float _droneFlySpeed = 3f;
        
        [BoxGroup(DroneSettingsTitle)]
        [SerializeField, Min(0)]
        private float _droneFlySpeedChangeRate = 3f;

        // PROPERTIES: ----------------------------------------------------------------------------

        public float DroneCartFlySpeed => _droneCartFlySpeed;
        public float DroneFlySpeed => _droneFlySpeed;
        public float DroneFlySpeedChangeRate => _droneFlySpeedChangeRate;
        
        // FIELDS: --------------------------------------------------------------------------------

        private const string DroneSettingsTitle = Constants.Settings + "/Drone"; 
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override string GetMetaCategory() =>
            EditorConstants.GameplayConfigsCategory;
    }
}