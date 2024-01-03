using MetaEditor;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Configs.Gameplay.MobileHeadquarters
{
    public class MobileHeadquartersConfigMeta : EditorMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField, Min(0)]
        private float _movementSpeed = 2f;
        
        [SerializeField, Min(0)]
        private float _motionSpeed = 2f;

        [SerializeField, Min(0)]
        private float _speedChangeRate = 5f;

        // PROPERTIES: ----------------------------------------------------------------------------

        public float MovementSpeed => _movementSpeed;
        public float MotionSpeed => _motionSpeed;
        public float SpeedChangeRate => _speedChangeRate;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override string GetMetaCategory() =>
            EditorConstants.GameplayConfigsCategory;
    }
}