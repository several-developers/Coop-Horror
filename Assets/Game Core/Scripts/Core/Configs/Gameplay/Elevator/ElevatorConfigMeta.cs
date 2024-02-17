using CustomEditors;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Configs.Gameplay.Elevator
{
    public class ElevatorConfigMeta : EditorMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField, Min(0)]
        private float _floorMovementDuration = 5f;

        [SerializeField, Min(0)]
        private float _reactivationDelay = 2f;

        [SerializeField, Min(0)]
        [InfoBox("NOT IMPLEMENTED", InfoMessageType.Warning)]
        private float _doorsOpenDuration = 1f;
        
        [SerializeField, Min(0)]
        [InfoBox("NOT IMPLEMENTED", InfoMessageType.Warning)]
        private float _doorsCloseDuration = 1f;

        // PROPERTIES: ----------------------------------------------------------------------------

        public float FloorMovementDuration => _floorMovementDuration;
        public float ReactivationDelay => _reactivationDelay;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override string GetMetaCategory() =>
            EditorConstants.GameplayConfigsCategory;
    }
}