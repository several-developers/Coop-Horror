using GameCore.InfrastructureTools.Configs;
using Sirenix.OdinInspector;
using Sonity;
using UnityEngine;

namespace GameCore.Infrastructure.Configs.Gameplay.Elevator
{
    public class ElevatorConfigMeta : ConfigMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField, Min(0f), SuffixLabel(Seconds, overlay: true)]
        private float _movementDelay = 3f;
        
        [SerializeField, Min(0f), SuffixLabel(Seconds, overlay: true)]
        private float _movementDurationPerFloor = 10f;

        [SerializeField, Min(0f)]
        private float _doorOpenDelay = 3f;

        [SerializeField, Min(0f)]
        private float _movementOffsetY = 200f;

        [SerializeField]
        private AnimationCurve _speedUpCurve;
        
        [SerializeField]
        private AnimationCurve _slowDownCurve;
        
        [Title(SFXTitle)]
        [SerializeField, Required]
        private SoundEvent _doorOpeningSE;
        
        [SerializeField, Required]
        private SoundEvent _doorClosingSE;

        [SerializeField, Required]
        private SoundEvent _floorChangeSE;

        [SerializeField, Required]
        private SoundEvent _buttonPushSE;

        // PROPERTIES: ----------------------------------------------------------------------------

        public float MovementDelay => _movementDelay;
        public float MovementDurationPerFloor => _movementDurationPerFloor;
        public float DoorOpenDelay => _doorOpenDelay;
        public float MovementOffsetY => _movementOffsetY;
        public AnimationCurve SpeedUpCurve => _speedUpCurve;
        public AnimationCurve SlowDownCurve => _slowDownCurve;
        
        public SoundEvent DoorOpeningSE => _doorOpeningSE;
        public SoundEvent DoorClosingSE => _doorClosingSE;
        public SoundEvent FloorChangeSE => _floorChangeSE;
        public SoundEvent ButtonPushSE => _buttonPushSE;

        // FIELDS: --------------------------------------------------------------------------------
        
        private const string SFXTitle = "SFX";
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override string GetMetaCategory() =>
            EditorConstants.GameplayConfigsCategory;
        
        public override ConfigScope GetConfigScope() =>
            ConfigScope.GameplayScene;
    }
}