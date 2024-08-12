using CustomEditors;
using DG.Tweening;
using Sirenix.OdinInspector;
using Sonity;
using UnityEngine;

namespace GameCore.Configs.Gameplay.Train
{
    public class TrainConfigMeta : EditorMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField, Min(0f)]
        private float _roadLocationMoveSpeed = 40f;
        
        [SerializeField, Min(0f)]
        private float _gameplayLocationMoveSpeed = 10f;
        
        [SerializeField, Min(0f)]
        private float _speedChangeRate = 1f;

        [SerializeField, Range(0f, 1f), SuffixLabel("%", overlay: true)]
        private float _leavingMainRoadSpeedMultiplier = 0.5f;

        [SerializeField, Range(0f, 1f), SuffixLabel("%", overlay: true)]
        private float _startPositionAtRoadLocation;

        [Title("New Settings")]
        [SerializeField, Min(0f)]
        private float _movementSpeed = 15f;

        [SerializeField, Min(0f)]
        private float _slowDownDuration = 5f;
        
        [SerializeField, Min(0f)]
        private float _speedUpDuration = 5f;

        [SerializeField]
        private Ease _slowDownEase;
        
        [SerializeField]
        private Ease _speedUpEase;

        [Title(SFXTitle)]
        [SerializeField, Required]
        private SoundEvent _doorOpenSE;
        
        [SerializeField, Required]
        private SoundEvent _doorCloseSE;
        
        [SerializeField, Required]
        private SoundEvent _departureSE;
        
        [SerializeField, Required]
        private SoundEvent _arrivalSE;
        
        [SerializeField, Required]
        private SoundEvent _movementLoopSE;

        // PROPERTIES: ----------------------------------------------------------------------------

        public float MovementSpeed => _roadLocationMoveSpeed;
        public float RoadLocationMoveSpeed => _roadLocationMoveSpeed;
        public float GameplayLocationMoveSpeed => _gameplayLocationMoveSpeed;
        public float SpeedChangeRate => _speedChangeRate;
        public float LeavingMainRoadSpeedMultiplier => _leavingMainRoadSpeedMultiplier;
        public float StartPositionAtRoadLocation => _startPositionAtRoadLocation;
        
        public float MovementSpeed2 => _movementSpeed;
        public float SlowDownDuration => _slowDownDuration;
        public float SpeedUpDuration => _speedUpDuration;
        public Ease SlowDownEase => _slowDownEase;
        public Ease SpeedUpEase => _speedUpEase;

        // SFX
        public SoundEvent DoorOpenSE => _doorOpenSE;
        public SoundEvent DoorCloseSE => _doorCloseSE;
        public SoundEvent DepartureSE => _departureSE;
        public SoundEvent ArrivalSE => _arrivalSE;
        public SoundEvent MovementLoopSE => _movementLoopSE;

        // FIELDS: --------------------------------------------------------------------------------
        
        private const string SFXTitle = "SFX";

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override string GetMetaCategory() =>
            EditorConstants.GameplayConfigsCategory;
    }
}